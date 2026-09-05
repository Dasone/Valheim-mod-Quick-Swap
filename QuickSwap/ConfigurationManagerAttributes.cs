using System;

/// <summary>
/// Optional metadata read by ConfigurationManager (the F1 settings window) through duck
/// typing: it looks up public fields by name on whatever object it finds, so this has to
/// stay in the global namespace with public fields and cannot be trimmed to properties.
/// ConfigurationManager defines a good many more fields than this; only the ones the mod
/// actually sets are declared, since an absent field simply keeps its default behaviour.
/// </summary>
[AttributeUsage(AttributeTargets.All)]
// ReSharper disable once CheckNamespace
public sealed class ConfigurationManagerAttributes : Attribute
{
    /// <summary>Higher values sort nearer the top of a section.</summary>
    public int? Order;
}
