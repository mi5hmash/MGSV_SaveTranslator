using System;

namespace MGSV_SaveTranslator.Helpers;

/// <summary>
/// Container for Global Read-Only Variables.
/// </summary>
public static class AppInfo
{
    #region APP INFO

    public static string Name => "MGSV - SaveData Translator";

    public static string RootPath => AppDomain.CurrentDomain.BaseDirectory;

    #endregion


    #region OTHER INFO

    public static string BackupFolder => "backup";

    public static string UnpackedFilesFolder => "unpacked";

    public static string PackedFilesFolder => "packed";

    public static string ProfilesFolder => "profiles";

    #endregion
}