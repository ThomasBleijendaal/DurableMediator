using System.Diagnostics.CodeAnalysis;
using DurableTask.Core;
using DurableTask.Core.Entities.OperationFormat;

namespace DurableMediator.HostedService.Plumbing;

internal class WorkflowOrchestrationContext
{
    private readonly Dictionary<string, Queue<IEventSource>> _externalEventSources = new(StringComparer.OrdinalIgnoreCase);
    private readonly NamedQueue<string> _externalEventBuffer = new();

    public required OrchestrationContext OrchestrationContext { get; init; }

    public async Task<T?> GetEventTaskAsync<T>(string eventName, TimeSpan timeout)
    {
        // Return immediately if this external event has already arrived.
        if (_externalEventBuffer.TryTake(eventName, out var bufferedEventPayload))
        {
            return OrchestrationContext.MessageDataConverter.Deserialize<T>(bufferedEventPayload)!;
        }

        // Create a task completion source that will be set when the external event arrives.
        using var eventCts = new CancellationTokenSource();
        var eventSource = new EventTaskCompletionSource<T?>();
        eventCts.Token.Register(() =>
        {
            eventSource.TrySetCanceled(eventCts.Token);
        });

        using var timerCts = new CancellationTokenSource();

        Task? timerTask = null;

        if (timeout != Timeout.InfiniteTimeSpan)
        {
            timerTask = OrchestrationContext.CreateTimer(OrchestrationContext.CurrentUtcDateTime.Add(timeout), eventName, timerCts.Token);
        }

        AddEvent(eventName, eventSource);

        if (timerTask != null)
        {
            var winner = await Task.WhenAny(timerTask, eventSource.Task);
            if (winner == timerTask)
            {
                eventCts.Cancel();
                return default;
            }
            else
            {
                timerCts.Cancel();
                return await eventSource.Task;
            }
        }
        else
        {
            return await eventSource.Task;
        }
    }

    public void CompleteTask(string eventName, string rawEventPayload)
    {
        if (_externalEventSources.TryGetValue(eventName, out var waiters))
        {
            object? value;

            var waiter = waiters.Dequeue();
            if (waiter.EventType == typeof(OperationResult))
            {
                // use the framework-defined deserialization for entity responses, not the application-defined data converter,
                // because we are just unwrapping the entity response without yet deserializing any application-defined data.

                // TODO: unsure how to fix this
                value = null;

                // value = this.entityFeature!.EntityContext.DeserializeEntityResponseEvent(rawEventPayload);
            }
            else
            {
                value = OrchestrationContext.MessageDataConverter.Deserialize(rawEventPayload, waiter.EventType);
            }

            // Events are completed in FIFO order. Remove the key if the last event was delivered.
            if (waiters.Count == 0)
            {
                _externalEventSources.Remove(eventName);
            }

            waiter.TrySetResult(value);
        }
        else
        {
            // The orchestrator isn't waiting for this event (yet?). Save it in case
            // the orchestrator wants it later.
            _externalEventBuffer.Add(eventName, rawEventPayload);
        }
    }

    private void AddEvent<T>(string eventName, EventTaskCompletionSource<T?> eventSource)
    {
        if (_externalEventSources.TryGetValue(eventName, out var existing))
        {
            if (existing.Count > 0 && existing.Peek().EventType != typeof(T))
            {
                throw new ArgumentException("Events with the same name must have the same type argument. Expected"
                    + $" {existing.Peek().GetType().FullName} but was requested {typeof(T).FullName}.");
            }

            existing.Enqueue(eventSource);
        }
        else
        {
            Queue<IEventSource> eventSourceQueue = new();
            eventSourceQueue.Enqueue(eventSource);
            _externalEventSources.Add(eventName, eventSourceQueue);
        }
    }

    interface IEventSource
    {
        /// <summary>
        /// Gets the type of the event stored in the completion source.
        /// </summary>
        Type EventType { get; }

        /// <summary>
        /// Tries to set the result on tcs.
        /// </summary>
        /// <param name="result">The result.</param>
        void TrySetResult(object result);
    }

    private sealed class EventTaskCompletionSource<T> : TaskCompletionSource<T>, IEventSource
    {
        /// <inheritdoc/>
        public Type EventType => typeof(T);

        /// <inheritdoc/>
        void IEventSource.TrySetResult(object result) => TrySetResult((T)result);
    }

    private sealed class NamedQueue<TValue>
    {
        readonly Dictionary<string, Queue<TValue>> _buffers = new(StringComparer.OrdinalIgnoreCase);

        public void Add(string name, TValue value)
        {
            if (!_buffers.TryGetValue(name, out var queue))
            {
                queue = new Queue<TValue>();
                _buffers[name] = queue;
            }

            queue.Enqueue(value);
        }

        public bool TryTake(string name, [NotNullWhen(true)] out TValue? value)
        {
            if (_buffers.TryGetValue(name, out var queue))
            {
                value = queue.Dequeue()!;
                if (queue.Count == 0)
                {
                    _buffers.Remove(name);
                }

                return true;
            }

            value = default;
            return false;
        }
    }
}
