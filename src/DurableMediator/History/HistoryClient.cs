using DurableTask.Core;
using DurableTask.Core.History;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DurableMediator.History;

internal class HistoryClient
{
    private readonly DurabilityProvider _provider;

    private static readonly JValue NullJValue = JValue.CreateNull();

    public HistoryClient(
        IDurabilityProviderFactory factory)
    {
        _provider = factory.GetDurabilityProvider();
    }

    public async Task<JArray?> GetDurableOrchestrationStatusAsync(string instanceId)
    {
        JArray? historyArray = null;

        var history = await _provider.GetOrchestrationHistoryAsync(instanceId, null);

        if (!string.IsNullOrEmpty(history))
        {
            historyArray = ConvertToJArray(history);

            if (historyArray == null)
            {
                return null;
            }

            var eventMapper = new Dictionary<string, EventIndexDateMapping>();
            var indexList = new List<int>();

            for (var i = 0; i < historyArray.Count; i++)
            {
                var historyItem = (JObject)historyArray[i];
                if (Enum.TryParse(historyItem["EventType"].Value<string>(), out EventType eventType))
                {
                    // Changing the value of EventType from integer to string for better understanding in the history output
                    historyItem["EventType"] = eventType.ToString();
                    switch (eventType)
                    {
                        case EventType.TaskScheduled:
                        case EventType.SubOrchestrationInstanceCreated:
                            TrackNameAndScheduledTime(historyItem, eventType, i, eventMapper);
                            historyItem.Remove("Version");
                            ConvertInputToJToken(historyItem);
                            break;
                        case EventType.EventSent:
                            historyItem.Remove("Version");
                            ConvertInputToJToken(historyItem, "Request");
                            break;
                        case EventType.EventRaised:
                            historyItem.Remove("Version");
                            ConvertOutputToJToken(historyItem, "Response");
                            break;
                        case EventType.TaskCompleted:
                        case EventType.TaskFailed:
                            AddScheduledEventDataAndAggregate(ref eventMapper, "TaskScheduled", historyItem, indexList);
                            historyItem["TaskScheduledId"]?.Parent.Remove();

                            ConvertOutputToJToken(historyItem);
                            break;
                        case EventType.SubOrchestrationInstanceCompleted:
                        case EventType.SubOrchestrationInstanceFailed:
                            AddScheduledEventDataAndAggregate(ref eventMapper, "SubOrchestrationInstanceCreated", historyItem, indexList);
                            historyItem.Remove("TaskScheduledId");

                            ConvertOutputToJToken(historyItem);
                            break;
                        case EventType.ExecutionStarted:
                            var functionName = historyItem["Name"];
                            historyItem.Remove("Name");
                            historyItem["FunctionName"] = functionName;
                            historyItem.Remove("OrchestrationInstance");
                            historyItem.Remove("ParentInstance");
                            historyItem.Remove("Version");
                            historyItem.Remove("Tags");
                            ConvertInputToJToken(historyItem);
                            break;
                        case EventType.ExecutionCompleted:
                            if (Enum.TryParse(historyItem["OrchestrationStatus"].Value<string>(), out OrchestrationStatus orchestrationStatus))
                            {
                                historyItem["OrchestrationStatus"] = orchestrationStatus.ToString();
                            }

                            ConvertOutputToJToken(historyItem);
                            break;
                        case EventType.TimerFired:
                            break;
                        case EventType.OrchestratorStarted:
                        case EventType.OrchestratorCompleted:
                            indexList.Add(i);
                            break;
                    }

                    historyItem.Remove("EventId");
                    historyItem.Remove("IsPlayed");
                }
            }

            var counter = 0;
            indexList.Sort();
            foreach (var indexValue in indexList)
            {
                historyArray.RemoveAt(indexValue - counter);
                counter++;
            }
        }


        return historyArray;
    }

    private static JArray? ConvertToJArray(string input)
    {
        JArray? jArray = null;
        if (input != null)
        {
            using (var stringReader = new StringReader(input))
            using (var jsonTextReader = new JsonTextReader(stringReader) { DateParseHandling = DateParseHandling.None })
            {
                jArray = JArray.Load(jsonTextReader);
            }
        }

        return jArray;
    }

    private static void ConvertInputToJToken(JObject jsonObject)
    {
        jsonObject["Input"] = ParseToJToken((string?)jsonObject["Input"]);
    }

    private static void ConvertInputToJToken(JObject jsonObject, string inputName)
    {
        var input = ParseToJToken((string?)jsonObject["Input"]);

        var wrappedRequest = JObject.Parse(input?.Value<string>("input"));

        var request = wrappedRequest?[inputName];

        jsonObject["Input"] = request;
    }

    private static void ConvertOutputToJToken(JObject jsonObject, string inputName)
    {
        var input = ParseToJToken((string?)jsonObject["Input"]);

        var wrappedRequest = JObject.Parse(input?.Value<string>("result"));

        var request = wrappedRequest?[inputName];

        jsonObject.Remove("Input");
        jsonObject["Result"] = request;
    }

    private static void ConvertOutputToJToken(JObject jsonObject)
    {
        jsonObject["Result"] = ParseToJToken((string?)jsonObject["Result"]);
    }

    internal static JToken? ParseToJToken(string? value)
    {
        if (value == null)
        {
            return NullJValue;
        }

        // Ignore whitespace
        value = value.Trim();
        if (value.Length == 0)
        {
            return string.Empty;
        }

        try
        {
            return ConvertToJToken(value);
        }
        catch (JsonReaderException)
        {
            // Return the raw string value as the fallback. This is common in terminate scenarios.
            return value;
        }
    }

    public static JToken? ConvertToJToken(string input)
    {
        JToken? token = null;
        if (input != null)
        {
            using (var stringReader = new StringReader(input))
            using (var jsonTextReader = new JsonTextReader(stringReader) { DateParseHandling = DateParseHandling.None })
            {
                token = JToken.Load(jsonTextReader);
            }
        }

        return token;
    }

    private static void TrackNameAndScheduledTime(JObject historyItem, EventType eventType, int index, Dictionary<string, EventIndexDateMapping> eventMapper)
    {
        eventMapper.Add($"{eventType}_{historyItem["EventId"]}", new EventIndexDateMapping { Index = index, Name = (string)historyItem["Name"], Date = (DateTime)historyItem["Timestamp"] });
    }

    private static void AddScheduledEventDataAndAggregate(ref Dictionary<string, EventIndexDateMapping> eventMapper, string prefix, JToken historyItem, List<int> indexList)
    {
        if (eventMapper.TryGetValue($"{prefix}_{historyItem["TaskScheduledId"]}", out var taskScheduledData))
        {
            historyItem["ScheduledTime"] = taskScheduledData.Date;
            historyItem["FunctionName"] = taskScheduledData.Name;
            indexList.Add(taskScheduledData.Index);
        }
    }

    private sealed class EventIndexDateMapping
    {
        public int Index { get; set; }

        public DateTime Date { get; set; }

        public string? Name { get; set; }
    }
}
