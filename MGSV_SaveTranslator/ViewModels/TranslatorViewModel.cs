using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MGSV_SaveTranslator.Controls;
using MGSV_SaveTranslator.Helpers;
using MGSV_SaveTranslator.Services;
using MGSVST_Core.Helpers;
using MGSVST_Core.Models;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Wpf.Ui.Common.Interfaces;

namespace MGSV_SaveTranslator.ViewModels;

public partial class TranslatorViewModel : ObservableObject, INavigationAware
{
    private bool _visited;

    // Paths
    [ObservableProperty] private string _saveFilePath = AppInfo.RootPath;

    // InfoBarFeeder
    [ObservableProperty] private InfoBarFeeder _infoBarFeeder = new();

    // Fields
    [ObservableProperty] private string _profileName = "";
    [ObservableProperty] private int _selectedProfileName;
    [ObservableProperty] private ObservableCollection<string> _profileNames = new();

    // Controls States
    [ObservableProperty] private bool _isAnalyzeEnabled;
    [ObservableProperty] private bool _isDeencryptEnabled;
    [ObservableProperty] private bool _isMakeBackupChecked = true;
    [ObservableProperty] private Visibility _progressVisibility = Visibility.Collapsed;
    [ObservableProperty] private string _deencryptButtonContent = "";

    [ObservableProperty] private MGSVSaveData _mgsvSaveData = new();

    private MGSVProfilesService _mgsvProfilesService;

    public TranslatorViewModel(MGSVProfilesService mgsvProfilesService)
    {
        _mgsvProfilesService = mgsvProfilesService;
    }

    public void OnNavigatedTo()
    {
        RepopulateCollections();
        PokeDecryptButtonState();

        if (_visited) return;
        _visited = true;
    }

    public void OnNavigatedFrom()
    {
    }

    [RelayCommand] private void SelectSaveFilePath()
    {
        InfoBarFeederReset();
        OpenFileDialog openFileDialog = new();
        if (openFileDialog.ShowDialog() == true) SaveFilePath = openFileDialog.FileName;
        OnSaveFilePathChanged(SaveFilePath);
    }

    [RelayCommand]
    private void LoadFile()
    {
        _mgsvSaveData.Load(_saveFilePath);
        InfoBarFeederConsume(_mgsvSaveData.Reporter);
    }

    [RelayCommand] private void OpenShortcut1() => IoHelpers.OpenDirectory(Directory.GetParent(SaveFilePath)!.FullName);

    partial void OnSaveFilePathChanged(string value)
    {
        var result = File.Exists(value);
        if (result) _mgsvSaveData.Load(value);
        IsAnalyzeEnabled = result;
        PokeDecryptButtonState();
    }

    partial void OnProfileNameChanged(string value)
    {
        PokeDecryptButtonState();
    }

    [RelayCommand]
    private void Analyze()
    {
        if (!_mgsvSaveData.IsEncrypted)
        {
            _mgsvSaveData.Reporter.Information("The file is decrypted so it can be encrypted with any profile.");
            InfoBarFeederConsume(_mgsvSaveData.Reporter);
            return;
        }
        var keyCandidates = _mgsvProfilesService.GameProfilesJson.Profiles.Select(x => x.Key);
        // test out all key candidates
        var result = _mgsvSaveData.Load(SaveFilePath);
        if (!result)
        {
            InfoBarFeederConsume(_mgsvSaveData.Reporter);
            return;
        }
        result = _mgsvSaveData.BruteforceKeyLite(keyCandidates.ToArray());
        if (result) 
        {
            var query = _mgsvProfilesService.GameProfilesJson.Profiles.Where(x => x.Key == _mgsvSaveData.Key);
            var profileName = query.Select(x => x.Name).First();
            SelectedProfileName = ProfileNames.IndexOf(profileName);
            _mgsvSaveData.Reporter.Success($"The appropriate profile for that file is '{ProfileName}'.");
        }
        else
        {
            _mgsvSaveData.Reporter.Error("Couldn't find matching Profile for that file.");
        }
        InfoBarFeederConsume(_mgsvSaveData.Reporter);
    }

    private void RepopulateCollections()
    {
        var rProfileName = ProfileName;
        var profileNames = _mgsvProfilesService.GameProfilesJson.Profiles.Select(x => x.Name);
        _profileNames.Clear();
        foreach (var item in profileNames) _profileNames.Add(item);
        SelectedProfileName = ProfileNames.IndexOf(rProfileName);
    }

    [RelayCommand]
    private void Deencrypt()
    {
        _mgsvSaveData.Key = _mgsvProfilesService.GameProfilesJson.Profiles.Where(x => x.Name == ProfileName).Select(x => x.Key).First();
        if (_mgsvSaveData.IsEncrypted)
        {
            _mgsvSaveData.Decrypt(IsMakeBackupChecked);
        }
        else
        {
            _mgsvSaveData.Encrypt(IsMakeBackupChecked);
        }
        PokeDecryptButtonState();
        InfoBarFeederConsume(_mgsvSaveData.Reporter);
    }

    private void PokeDecryptButtonState()
    {
        IsDeencryptEnabled = _isAnalyzeEnabled && SelectedProfileName > -1;
        DeencryptButtonContent = _mgsvSaveData.IsEncrypted ? "DECRYPT" : "ENCRYPT";
    }
    
    private void InfoBarFeederReset()
    {
        InfoBarFeeder.Reset();
        OnPropertyChanged(nameof(InfoBarFeeder));
    }
    private void InfoBarFeederConsume(SimpleStatusReporter simpleStatusReporter)
    {
        InfoBarFeeder.Consume(simpleStatusReporter);
        OnPropertyChanged(nameof(InfoBarFeeder));
    }
}