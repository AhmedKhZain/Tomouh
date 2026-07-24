namespace Common.DataConvrters;

/// <summary>
/// Specifies that an enum property should be stored as a string in the database 
/// rather than its default integer value.
/// </summary>
/// <remarks>
/// Apply this attribute to an enum definition or an enum property to enable string-based mapping.
/// </remarks>
[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Property)]
public sealed class StoreEnumAsStringAttribute : Attribute
{
    /// <summary>
    /// Gets the maximum length allowed for the database column. Default is 50.
    /// </summary>
    public int MaxLength { get; } = 50;

    /// <summary>
    /// Gets or sets the transformation strategy applied to the enum string value. 
    /// Default is <see cref="EnumNamingStrategy.Default"/>.
    /// </summary>
    public EnumNamingStrategy NamingStrategy { get; set; } = EnumNamingStrategy.Default;

    /// <summary>
    /// Gets or sets a value indicating whether string-to-enum conversion should be case-insensitive. 
    /// Default is true.
    /// </summary>
    public bool CaseInsensitive { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="StoreEnumAsStringAttribute"/> class.
    /// </summary>
    /// <param name="maxLength">The maximum length for the database column.</param>
    /// <param name="namingStrategy">The naming transformation strategy.</param>
    /// <param name="caseInsensitive">Whether to ignore case during conversion.</param>
    public StoreEnumAsStringAttribute(
        int maxLength = 50,
        EnumNamingStrategy namingStrategy = EnumNamingStrategy.Default,
        bool caseInsensitive = true)
    {
        MaxLength = maxLength;
        NamingStrategy = namingStrategy;
        CaseInsensitive = caseInsensitive;
    }
}

/// <summary>
/// Defines the transformation strategies for enum string values.
/// </summary>
public enum EnumNamingStrategy
{
    /// <summary> The enum name is stored as-is (PascalCase). </summary>
    Default,

    /// <summary> The enum name is converted to UPPERCASE (e.g., "PENDING"). </summary>
    Uppercase,

    /// <summary> The enum name is converted to lowercase (e.g., "pending"). </summary>
    Lowercase
}