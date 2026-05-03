namespace illusion.Common.Constants;

/// <summary>
/// GGTP(global ground truth plane)
/// </summary>
public static class GGTPConstants
{
    public const string DefaultServerUrl = "https://ggtp.illusionary-ai.org";
    public const int DefaultTimeoutSeconds = 30;
    public const bool DefaultAutoSync = true;
    public const bool DefaultSignatureVerify = true;

    // module extension name
    public const string ModuleFileExtension = ".iamodule";
    public const string SignatureFileExtension = ".sig";

    // namespace prefix
    public const string NamespacePrefix = "ggtp://";
}