using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MGSV_SaveTranslator.Helpers;
using MGSVST_Core.Models;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MGSV_SaveTranslator.Controls;
using Wpf.Ui.Common.Interfaces;
using MGSVST_Core.Helpers;
using System.Collections.ObjectModel;
using MGSV_SaveTranslator.Services;

namespace MGSV_SaveTranslator.ViewModels;

public partial class ResearchViewModel : ObservableObject, INavigationAware
{
    private bool _visited;

    // Paths
    [ObservableProperty] private string _saveFilePath = AppInfo.RootPath;
    // InfoBarFeeder
    [ObservableProperty] private InfoBarFeeder _infoBarFeeder = new();
    // Progress
    [ObservableProperty] private ProgressModel _progressModel = new();
    // Fields
    [ObservableProperty] private string _profileName = "";
    [ObservableProperty] private int _selectedProfileName;
    [ObservableProperty] private ObservableCollection<string> _profileNames = new();
    [ObservableProperty] private uint _key;
    // Controls States
    [ObservableProperty] private bool _isBruteforceEnabled;
    [ObservableProperty] private Visibility _bruteforceButtonVisibility = Visibility.Visible;
    [ObservableProperty] private Visibility _bruteforceAbortButtonVisibility = Visibility.Collapsed;
    [ObservableProperty] private Visibility _progressVisibility = Visibility.Collapsed;
    
    private CancellationTokenSource _cts = new();

    // Dependencies
    private MGSVProfilesService _mgsvProfilesService;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    /// <param name="mgsvProfilesService"></param>
    public ResearchViewModel(MGSVProfilesService mgsvProfilesService)
    {
        _mgsvProfilesService = mgsvProfilesService;
    }

    public void OnNavigatedTo()
    {
        if (_visited) return;
        RepopulateCollections();
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
    private void GuessKeyUsingFileType()
    {
        _mgsvProfilesService.Reporter.Information("Trying to guess a key for the PC version of the game...");
        InfoBarFeederConsume(_mgsvProfilesService.Reporter);

        MGSVSaveData mgsvst = new();
        mgsvst.Load(_saveFilePath);
        var ft = _profileName;
        try
        {
            ft = _profileName.Split("] ")[1];
        }
        catch
        {
            // ignored
        }
        mgsvst.GenerateUsingFileType(ft);
        Key = mgsvst.Key;
        InfoBarFeederConsume(mgsvst.Reporter);
    }

    [RelayCommand]
    private async Task BruteforceKeyAsync()
    {
        _mgsvProfilesService.Reporter.Information("Trying to bruteforce a decryption key...");
        InfoBarFeederConsume(_mgsvProfilesService.Reporter);
        BruteforceButtonVisibility = Visibility.Collapsed;
        BruteforceAbortButtonVisibility = Visibility.Visible;
        ProgressVisibility = Visibility.Visible;

        _cts = new CancellationTokenSource();
        MGSVSaveData mgsvst = new();
        mgsvst.Load(_saveFilePath);
        
        try
        {
            // progress bar update hack 
            Task.Run(async () => {
                while (!_cts.Token.IsCancellationRequested)
                {
                    OnPropertyChanged(nameof(ProgressModel));
                    await Task.Delay(TimeSpan.FromSeconds(1), _cts.Token);
                }
            }, _cts.Token);
        }
        catch (OperationCanceledException)
        {
        }

        _progressModel.TasksTotal = uint.MaxValue;
        try
        {
            await Task.Run(() => mgsvst.BruteforceKey(_cts.Token, _progressModel));
            Key = mgsvst.Key;
            ProgressModel.Numeric = 100;
            OnPropertyChanged(nameof(ProgressModel));
            InfoBarFeederConsume(mgsvst.Reporter);
        }
        catch (OperationCanceledException)
        {
            mgsvst.Reporter.Information("Operation has been cancelled.");
            InfoBarFeederConsume(mgsvst.Reporter);
            Key = 0;
            ProgressModel.Numeric = 0;
            OnPropertyChanged(nameof(ProgressModel));
            BruteforceButtonVisibility = Visibility.Visible;
            BruteforceAbortButtonVisibility = Visibility.Collapsed;
            return;
        }
        BruteforceKeyAbort();
    }
    
    [RelayCommand]
    private void BruteforceKeyAbort()
    {
        _cts.Cancel();
        _cts.Dispose();
        BruteforceButtonVisibility = Visibility.Visible;
        BruteforceAbortButtonVisibility = Visibility.Collapsed;
    }

    [RelayCommand]
    private void LoadFile()
    {
        _mgsvProfilesService.LoadData();
        InfoBarFeederConsume(_mgsvProfilesService.Reporter);
        RepopulateCollections();
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
    private void SaveFile()
    {
        _mgsvProfilesService.SaveData();
        InfoBarFeederConsume(_mgsvProfilesService.Reporter);
        RepopulateCollections();
    }

    [RelayCommand]
    private void AddProfile()
    {
        MGSVProfile profile = new()
        {
            Name = ProfileName,
            Key = _key
        };
        var result = _mgsvProfilesService.Add(profile);
        if (result)
        {
            if (!_profileNames.Contains(profile.Name)) _profileNames.Add(profile.Name);
        }
        InfoBarFeederConsume(_mgsvProfilesService.Reporter);
    }

    [RelayCommand]
    private void RemoveProfile()
    {
        MGSVProfile profile = new()
        {
            Name = ProfileName,
            Key = _key
        };
        _mgsvProfilesService.Remove(profile);
        InfoBarFeederConsume(_mgsvProfilesService.Reporter);
    }

    partial void OnSaveFilePathChanged(string value)
    {
        var result = File.Exists(value);
        IsBruteforceEnabled = result;
        ProfileName = result ? $"[PLATFORM_NAME] {Path.GetFileNameWithoutExtension(value).RemoveSuffixNumbers()}" : "";
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