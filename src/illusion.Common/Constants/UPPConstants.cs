namespace illusion.Common.Constants;

/// <summary>
/// UPP(user private plane)
/// </summary>
public static class UPPConstants
{
    public const bool DefaultEncryptionEnabled = true;
    public const bool DefaultAutoBackup = true;
    public const int DefaultBackupRetentionDays = 30;

    // file extension name
    public const string StorageFileExtension = ".db";
    public const string BackupFileExtension = ".zip";

    // namespace prefix
    public const string NamespacePrefix = "upp://";
    public const string DefaultNamespace = "upp://default/";
}