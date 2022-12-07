using System.Runtime.Serialization;

namespace AutoIoc;

/// <summary>
///     Any explicit errors caught within this package will throw this exception type.
/// </summary>
[Serializable]
public class AutoIocException : Exception
{
    /// <summary>
    ///     Creates a new exception with the specified <paramref name="message" />
    /// </summary>
    /// <param name="message"></param>
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