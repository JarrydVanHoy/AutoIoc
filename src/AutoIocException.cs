using System.Runtime.Serialization;

namespace AutoIoc;

[Serializable]
public class AutoIocException : Exception
{
    public AutoIocException
    (
        string message
    )
        : base(message)
    {
    }

    private AutoIocException
    (
        SerializationInfo info,
        StreamingContext context
    )
        : base(info, context)
    {
    }
}