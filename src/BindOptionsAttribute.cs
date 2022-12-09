namespace AutoIoc;

/// <summary>
///     Automatically binds your application settings to the class you've attributed. By default we use the class name, minus the '*Configuration' or '*Config' postfix, to find the configuration
///     section within application settings to bind to.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class BindOptionsAttribute : Attribute
{
    internal readonly string? ConfigurationSection;

    /// <summary>
    ///     Automatically binds your application settings to the class you've attributed. By default we use the class name, minus the '*Configuration' or '*Config' postfix, to find the configuration
    ///     section within application settings to bind to.
    /// </summary>
    /// <param name="configurationSection">If your application settings key does not match the class name or its nested, here is where you'd specify the specific section key</param>
    public BindOptionsAttribute
    (
        string? configurationSection = null
    )
    {
        ConfigurationSection = configurationSection;
    }

    /// <summary>
    /// If unable to add to the IOC, and exception will be thrown, otherwise a console log will indicate that it was unable to be added.
    /// Default: true
    /// </summary>
    public bool Required { get; set; } = true;
}