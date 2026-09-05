using System;

/// <summary>
/// Optional metadata read by ConfigurationManager (the F1 settings window) through
/// duck typing. Must stay in the global namespace with public fields — that is the
/// contract ConfigurationManager looks for.
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
// ReSharper disable once CheckNamespace
public sealed class ConfigurationManagerAttributes : Attribute
{
    /// <summary>Higher values sort nearer the top of a section.</summary>
    public int? Order;

    public bool? Browsable;
    public string Category;
    public bool? HideDefaultButton;
    public bool? HideSettingName;
    public bool? IsAdvanced;
    public string Description;
    public string DispName;
    public bool? ReadOnly;
}
