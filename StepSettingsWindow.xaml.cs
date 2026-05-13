using System.Windows;
using PythonEnvBuilder.ViewModels;

namespace PythonEnvBuilder;

public partial class StepSettingsWindow : Window
{
    private readonly MainViewModel _viewModel;

    public StepSettingsWindow(MainViewModel viewModel, StepViewModel step)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        TitleText.Text = step.Title + " の設定";
        NotesText.Text = step.Notes;
        ShowPanel(step.SettingsKind);
    }

    private void ShowPanel(string settingsKind)
    {
        NoSettingsPanel.Visibility = Visibility.Collapsed;
        DiagnosticsPanel.Visibility = Visibility.Collapsed;
        ProxyPanel.Visibility = Visibility.Collapsed;
        VenvPanel.Visibility = Visibility.Collapsed;
        PackagesPanel.Visibility = Visibility.Collapsed;
        RequirementsPanel.Visibility = Visibility.Collapsed;

        switch (settingsKind)
        {
            case "Diagnostics":
                DiagnosticsPanel.Visibility = Visibility.Visible;
                break;
            case "Proxy":
                ProxyPanel.Visibility = Visibility.Visible;
                break;
            case "Venv":
            case "Pip":
            case "VSCode":
                VenvPanel.Visibility = Visibility.Visible;
                break;
            case "Packages":
                PackagesPanel.Visibility = Visibility.Visible;
                break;
            case "Requirements":
                RequirementsPanel.Visibility = Visibility.Visible;
                break;
            default:
                NoSettingsPanel.Visibility = Visibility.Visible;
                break;
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.RefreshSettingSummaries();
        Close();
    }
}
