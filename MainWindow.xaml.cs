using System.Windows;
using PythonEnvBuilder.ViewModels;

namespace PythonEnvBuilder;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private void ShowCommand_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: StepViewModel step })
        {
            new CommandPreviewWindow(step) { Owner = this }.ShowDialog();
        }
    }

    private void ShowSettings_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: StepViewModel step } && DataContext is MainViewModel viewModel)
        {
            new StepSettingsWindow(viewModel, step) { Owner = this }.ShowDialog();
            viewModel.RefreshSettingSummaries();
        }
    }
}
