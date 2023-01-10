using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using MGSV_SaveTranslator.Helpers;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace MGSV_SaveTranslator.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private bool _isInitialized;

        [ObservableProperty]
        private string _applicationTitle = string.Empty;

        [ObservableProperty]
        private ObservableCollection<INavigationControl> _navigationItems = new();

        [ObservableProperty]
        private ObservableCollection<INavigationControl> _navigationFooter = new();

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new();

        public MainWindowViewModel(INavigationService navigationService)
        {
            if (!_isInitialized)
                InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            ApplicationTitle = AppInfo.Name;

            NavigationItems = new ObservableCollection<INavigationControl>
            {
                new NavigationItem()
                {
                    Content = "Translator",
                    PageTag = "translator",
                    Icon = SymbolRegular.Translate24,
                    PageType = typeof(Views.Pages.TranslatorPage)
                },
                new NavigationItem()
                {
                    Content = "Research",
                    PageTag = "research",
                    Icon = SymbolRegular.Beaker24,
                    PageType = typeof(Views.Pages.ResearchPage)
                }
            };

            NavigationFooter = new ObservableCollection<INavigationControl>
            {
                new NavigationItem()
                {
                    Content = "About",
                    PageTag = "about",
                    Icon = SymbolRegular.QuestionCircle24,
                    PageType = typeof(Views.Pages.AboutPage)
                }
            };
            
            _isInitialized = true;
        }
    }
}
