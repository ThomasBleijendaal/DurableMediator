using System.Runtime.Serialization;

namespace WorkflowHandlers.Exceptions;

[Serializable]
public class RequestException : Exception
{
    public RequestException(string? message) : base(message)
    {
    }

    protected RequestException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
    {
    }
}
