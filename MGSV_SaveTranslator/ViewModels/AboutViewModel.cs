using CommunityToolkit.Mvvm.ComponentModel;
using MGSV_SaveTranslator.Helpers;
using Wpf.Ui.Common.Interfaces;

namespace MGSV_SaveTranslator.ViewModels;

public partial class AboutViewModel : ObservableObject, INavigationAware
{
    private bool _isInitialized;

    [ObservableProperty]
    private string _appVersion = string.Empty;

    [ObservableProperty]
    private string _appAuthor = string.Empty;

    public void OnNavigatedTo()
    {
        if (!_isInitialized)
            InitializeViewModel();
    }

    public void OnNavigatedFrom()
    {
    }

    private void InitializeViewModel()
    {
        AppVersion = AppInfo.Version;
        AppAuthor = AppInfo.Author;
        _isInitialized = true;
    }
}