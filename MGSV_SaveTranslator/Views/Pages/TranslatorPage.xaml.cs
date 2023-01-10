using System.Windows;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Controls;

namespace MGSV_SaveTranslator.Views.Pages;

/// <summary>
/// Interaction logic for TranslatorPage.xaml
/// </summary>
public partial class TranslatorPage : INavigableView<ViewModels.TranslatorViewModel>
{
    public ViewModels.TranslatorViewModel ViewModel
    {
        get;
    }

    public TranslatorPage(ViewModels.TranslatorViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();
    }

    private void TextBox_Drop(object sender, DragEventArgs e)
    {
        if (e.Data is not DataObject dataObject || !dataObject.ContainsFileDropList()) return;

        // Many files
        //foreach (string filePath in ((DataObject)e.Data).GetFileDropList())
        //{
        //}

        // Single file
        if (sender is TextBox textBox) textBox.Text = dataObject.GetFileDropList()[0];
    }
    private void TextBox_PreviewDragEnter(object sender, DragEventArgs e)
    {
        var dropPossible = e.Data != null && ((DataObject)e.Data).ContainsFileDropList();
        if (dropPossible) e.Effects = DragDropEffects.Copy;
    }
    private void TextBox_PreviewDragOver(object sender, DragEventArgs e) => e.Handled = true;
}