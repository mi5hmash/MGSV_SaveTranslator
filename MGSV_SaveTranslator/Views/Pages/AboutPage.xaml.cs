using MGSV_SaveTranslator.ViewModels;
using Wpf.Ui.Common.Interfaces;

namespace MGSV_SaveTranslator.Views.Pages;

/// <summary>
/// Interaction logic for AboutPage.xaml
/// </summary>
public partial class AboutPage : INavigableView<AboutViewModel>
{
    public AboutViewModel ViewModel { get; }

    public AboutPage(AboutViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();
    }
}