using System.Windows;
using PythonEnvBuilder.ViewModels;

namespace PythonEnvBuilder;

public partial class CommandPreviewWindow : Window
{
    public CommandPreviewWindow(StepViewModel step)
    {
        InitializeComponent();
        DataContext = step;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
