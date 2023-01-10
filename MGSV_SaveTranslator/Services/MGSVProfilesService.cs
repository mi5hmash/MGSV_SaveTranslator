using System;
using MGSV_SaveTranslator.Helpers;
using MGSVST_Core.Helpers;
using MGSVST_Core.Models;
using System.IO;
using System.Text.Json;
using static MGSVST_Core.Helpers.IoHelpers;

namespace MGSV_SaveTranslator.Services;

public class MGSVProfilesService
{
    /// <summary>
    /// A status reporter.
    /// </summary>
    public SimpleStatusReporter Reporter { get; set; } = new();

    /// <summary>
    /// A path to the "MGSVProfilesService.json" file.
    /// </summary>
    public string FilePath { get; } = Path.Combine(AppInfo.RootPath, "MGSVProfiles.json");
    
    /// <summary>
    /// A container with all known profiles.
    /// </summary>
    public MGSVProfilesJson GameProfilesJson { get; set; } = new();

    public MGSVProfilesService()
    {
        if (!LoadData()) throw new FileLoadException();
    }

    /// <summary>
    /// Loads known game profiles configurations from a file.
    /// </summary>
    /// <returns></returns>
    public bool LoadData()
    {
        // Create a new file if it does not exist.
        if (!File.Exists(FilePath)) SaveData();
        
        // Try to write file.
        try
        {
            var jsonData = ReadFile(FilePath);
            GameProfilesJson = JsonSerializer.Deserialize<MGSVProfilesJson>(jsonData)!;
        }
        catch
        {
            Reporter.Error("File \"MGSVProfilesService.json\" could not be loaded.");
            return false;
        }
        Reporter.Success("File \"MGSVProfilesService.json\" loaded successfully.");
        return true;
    }

    /// <summary>
    /// Saves known game profiles configurations into file.
    /// </summary>
    /// <returns></returns>
    public bool SaveData()
    {
        // Try to write file.
        try
        {
            GameProfilesJson.Profiles.Sort((p, q) => string.Compare(p.Name, q.Name, StringComparison.Ordinal));
            var jsonOptions = new JsonSerializerOptions { WriteIndented = false };
            var jsonString = JsonSerializer.Serialize(GameProfilesJson, jsonOptions);
            File.WriteAllText(FilePath, jsonString);
        }
        catch
        {
            Reporter.Error("File \"MGSVProfilesService.json\" could not be written.");
            return false;
        }
        Reporter.Success("File \"MGSVProfilesService.json\" saved successfully.");
        return true;
    }

    /// <summary>
    /// Adds new profile to the collection of known profiles.
    /// </summary>
    /// <param name="profile"></param>
    /// <returns></returns>
    public bool Add(MGSVProfile profile)
    {
        var test = GameProfilesJson.Profiles.Exists(x => x.Name == profile.Name);
        if (!test)
        {
            GameProfilesJson.Profiles.Add(profile);
            Reporter.Success("Profile Entry added successfully.");
            return true;
        }
        Reporter.Information("That Profile Entry already exists.");
        return false;
    }

    /// <summary>
    /// Removes all occurrences of a specified profile from the collection of known profiles.
    /// </summary>
    /// <param name="profile"></param>
    public void Remove(MGSVProfile profile)
    {
        GameProfilesJson.Profiles.RemoveAll(x => x.Name == profile.Name);
        Reporter.Information("All existing instances of the specified profile have been deleted.");
    }
}