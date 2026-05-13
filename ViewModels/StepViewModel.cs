using System.Windows.Input;
using PythonEnvBuilder.Utilities;

namespace PythonEnvBuilder.ViewModels;

public sealed class StepViewModel : ObservableObject
{
    private string _status = "未実行";
    private string _commandText;
    private string _settingsSummary;

    public StepViewModel(
        int index,
        string title,
        string description,
        string commandText,
        string settingsKind,
        string settingsSummary,
        string notes,
        ICommand command)
    {
        Index = index;
        Title = title;
        Description = description;
        _commandText = commandText;
        SettingsKind = settingsKind;
        _settingsSummary = settingsSummary;
        Notes = notes;
        Command = command;
    }

    public int Index { get; }
    public string Title { get; }
    public string Description { get; }
    public string SettingsKind { get; }
    public string Notes { get; }
    public ICommand Command { get; }

    public string CommandText
    {
        get => _commandText;
        set => SetProperty(ref _commandText, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string SettingsSummary
    {
        get => _settingsSummary;
        set => SetProperty(ref _settingsSummary, value);
    }
}
