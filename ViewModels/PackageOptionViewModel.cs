using PythonEnvBuilder.Utilities;

namespace PythonEnvBuilder.ViewModels;

public sealed class PackageOptionViewModel : ObservableObject
{
    private bool _isSelected;

    public PackageOptionViewModel(string name, bool isSelected)
    {
        Name = name;
        _isSelected = isSelected;
    }

    public string Name { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
