using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using PythonEnvBuilder.Models;
using PythonEnvBuilder.Services;
using PythonEnvBuilder.Utilities;

namespace PythonEnvBuilder.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly CommandCatalog _commands = new();
    private readonly ShellService _shell = new();
    private readonly EnvironmentDiagnosticService _diagnostics;
    private readonly ProxyService _proxy = new();
    private readonly VenvService _venv;
    private readonly PipService _pip;
    private readonly VSCodeService _vscode;
    private string _venvName = ".venv";
    private string _proxyUrl = "http://proxy:8080";
    private string _customPackage = "";
    private bool _setHttpProxy = true;
    private bool _setHttpsProxy = true;
    private bool _useMachineTarget;

    public MainViewModel()
    {
        _diagnostics = new EnvironmentDiagnosticService(_commands, _shell);
        _venv = new VenvService(_commands, _shell);
        _pip = new PipService(_commands, _shell);
        _vscode = new VSCodeService(_commands, _shell);

        Packages = new ObservableCollection<PackageOptionViewModel>
        {
            new("numpy", true),
            new("pandas", true),
            new("matplotlib", true),
            new("scipy", false),
            new("jupyter", false)
        };

        RunDiagnosticsCommand = new AsyncRelayCommand(RunDiagnosticsAsync);
        ApplyProxyCommand = new AsyncRelayCommand(ApplyProxyAsync);
        CreateVenvCommand = new AsyncRelayCommand(CreateVenvAsync);
        UpgradePipCommand = new AsyncRelayCommand(UpgradePipAsync);
        InstallPackagesCommand = new AsyncRelayCommand(InstallPackagesAsync);
        ExportRequirementsCommand = new AsyncRelayCommand(ExportRequirementsAsync);
        ImportRequirementsCommand = new AsyncRelayCommand(ImportRequirementsAsync);
        OpenVSCodeCommand = new AsyncRelayCommand(OpenVSCodeAsync);

        Steps = new ObservableCollection<StepViewModel>
        {
            new(0, "準備1 環境確認", "Python、py launcher、PATH、VSCode の検出状態を確認します。", "python --version / py --version / where python / where code", "Diagnostics", "設定なし", "診断結果は設定画面で確認できます。", RunDiagnosticsCommand),
            new(1, "準備2 Proxy設定", "社内Proxy環境で pip が通信できるように環境変数を設定します。", "set HTTP_PROXY / HTTPS_PROXY", "Proxy", ProxySummary, "Proxy変更後は新しいシェルで反映されます。", ApplyProxyCommand),
            new(2, "STEP1 仮想環境作成", "プロジェクト専用の venv を作成します。", _commands.Get("CreateVenv", new Dictionary<string, string> { ["venv_name"] = VenvName }), "Venv", VenvSummary, "既定ではプロジェクト直下に .venv を作成します。", CreateVenvCommand),
            new(3, "STEP2 pip更新", "古い pip によるインストール失敗を避けるため pip を更新します。", _commands.Get("UpgradePip", Replacements), "Pip", VenvSummary, "仮想環境内の Python から pip を更新します。", UpgradePipCommand),
            new(4, "STEP3 ライブラリ導入", "選択した定番パッケージと任意入力パッケージをインストールします。", _commands.Get("InstallPackage", ReplacementsWithPackage("numpy")), "Packages", PackageSummary, "複数パッケージは順番に実行され、失敗もログに残ります。", InstallPackagesCommand),
            new(5, "STEP4 requirements管理", "現在の環境を出力、または requirements.txt から復元します。", "pip freeze > requirements.txt / pip install -r requirements.txt", "Requirements", "Export実行 / Importは設定画面", "メインの実行ボタンは Export です。Import は設定画面から実行できます。", ExportRequirementsCommand),
            new(6, "STEP5 VSCode連携", "VSCodeのPython interpreter設定を書き込み、code . を実行します。", "code .", "VSCode", VenvSummary, ".vscode/settings.json を生成してから VSCode を開きます。", OpenVSCodeCommand)
        };

        AddInfo("PythonEnvBuilder を起動しました。作業フォルダ: " + WorkingDirectory);
    }

    public ObservableCollection<StepViewModel> Steps { get; }
    public ObservableCollection<PackageOptionViewModel> Packages { get; }
    public ObservableCollection<DiagnosticItem> Diagnostics { get; } = new();
    public ObservableCollection<LogEntry> Logs { get; } = new();

    public AsyncRelayCommand RunDiagnosticsCommand { get; }
    public AsyncRelayCommand ApplyProxyCommand { get; }
    public AsyncRelayCommand CreateVenvCommand { get; }
    public AsyncRelayCommand UpgradePipCommand { get; }
    public AsyncRelayCommand InstallPackagesCommand { get; }
    public AsyncRelayCommand ExportRequirementsCommand { get; }
    public AsyncRelayCommand ImportRequirementsCommand { get; }
    public AsyncRelayCommand OpenVSCodeCommand { get; }

    public string WorkingDirectory => Environment.CurrentDirectory;

    public string VenvName
    {
        get => _venvName;
        set
        {
            if (SetProperty(ref _venvName, value))
            {
                UpdateCommandTexts();
            }
        }
    }

    public string ProxyUrl
    {
        get => _proxyUrl;
        set
        {
            if (SetProperty(ref _proxyUrl, value))
            {
                RefreshSettingSummaries();
            }
        }
    }

    public string CustomPackage
    {
        get => _customPackage;
        set
        {
            if (SetProperty(ref _customPackage, value))
            {
                RefreshSettingSummaries();
            }
        }
    }

    public bool SetHttpProxy
    {
        get => _setHttpProxy;
        set
        {
            if (SetProperty(ref _setHttpProxy, value))
            {
                RefreshSettingSummaries();
            }
        }
    }

    public bool SetHttpsProxy
    {
        get => _setHttpsProxy;
        set
        {
            if (SetProperty(ref _setHttpsProxy, value))
            {
                RefreshSettingSummaries();
            }
        }
    }

    public bool UseUserTarget
    {
        get => !_useMachineTarget;
        set
        {
            if (SetProperty(ref _useMachineTarget, !value))
            {
                OnPropertyChanged(nameof(UseMachineTarget));
                RefreshSettingSummaries();
            }
        }
    }

    public bool UseMachineTarget
    {
        get => _useMachineTarget;
        set
        {
            if (SetProperty(ref _useMachineTarget, value))
            {
                OnPropertyChanged(nameof(UseUserTarget));
                RefreshSettingSummaries();
            }
        }
    }

    public string ProxySummary => $"{(UseMachineTarget ? "Machine" : "User")} / {(SetHttpProxy ? "HTTP" : "-")} {(SetHttpsProxy ? "HTTPS" : "-")}";
    public string VenvSummary => VenvName;
    public string PackageSummary
    {
        get
        {
            var selected = Packages.Count(x => x.IsSelected);
            var custom = string.IsNullOrWhiteSpace(CustomPackage) ? "" : " + custom";
            return $"{selected} selected{custom}";
        }
    }

    private IReadOnlyDictionary<string, string> Replacements => new Dictionary<string, string>
    {
        ["venv_python"] = Path.Combine(VenvName, "Scripts", "python.exe")
    };

    private IReadOnlyDictionary<string, string> ReplacementsWithPackage(string package) => new Dictionary<string, string>
    {
        ["venv_python"] = Path.Combine(VenvName, "Scripts", "python.exe"),
        ["package"] = package
    };

    private async Task RunDiagnosticsAsync()
    {
        SetStepStatus(0, "実行中");
        Diagnostics.Clear();
        var results = await _diagnostics.RunAsync(WorkingDirectory);
        foreach (var (item, result) in results)
        {
            Diagnostics.Add(item);
            AddResult(result);
        }

        SetStepStatus(0, results.All(x => x.Result.IsSuccess) ? "成功" : "警告あり");
    }

    private async Task ApplyProxyAsync()
    {
        SetStepStatus(1, "実行中");
        try
        {
            var target = UseMachineTarget ? EnvironmentVariableTarget.Machine : EnvironmentVariableTarget.User;
            _proxy.Apply(ProxyUrl, SetHttpProxy, SetHttpsProxy, target);
            AddSuccess($"Proxy環境変数を {target} に設定しました。");
            await RestartShellAsync("Proxy変更後");
            SetStepStatus(1, "成功");
        }
        catch (Exception ex)
        {
            AddError(ex.Message);
            SetStepStatus(1, "失敗");
        }
    }

    private async Task CreateVenvAsync()
    {
        SetStepStatus(2, "実行中");
        var result = await _venv.CreateAsync(VenvName, WorkingDirectory);
        AddResult(result);
        if (result.IsSuccess)
        {
            await RestartShellAsync("venv作成後");
        }

        SetStepStatus(2, result.IsSuccess ? "成功" : "失敗");
    }

    private async Task UpgradePipAsync()
    {
        SetStepStatus(3, "実行中");
        var result = await _pip.UpgradeAsync(VenvName, WorkingDirectory);
        AddResult(result);
        SetStepStatus(3, result.IsSuccess ? "成功" : "失敗");
    }

    private async Task InstallPackagesAsync()
    {
        SetStepStatus(4, "実行中");
        var packages = Packages.Where(x => x.IsSelected).Select(x => x.Name).ToList();
        if (!string.IsNullOrWhiteSpace(CustomPackage))
        {
            packages.Add(CustomPackage.Trim());
        }

        if (packages.Count == 0)
        {
            AddInfo("インストール対象パッケージが選択されていません。");
            SetStepStatus(4, "未選択");
            return;
        }

        var failed = false;
        foreach (var package in packages.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var result = await _pip.InstallPackageAsync(VenvName, package, WorkingDirectory);
            AddResult(result);
            failed |= !result.IsSuccess;
        }

        SetStepStatus(4, failed ? "失敗あり" : "成功");
    }

    private async Task ExportRequirementsAsync()
    {
        SetStepStatus(5, "Export中");
        var result = await _pip.ExportRequirementsAsync(VenvName, WorkingDirectory);
        AddResult(result, " > requirements.txt");
        SetStepStatus(5, result.IsSuccess ? "Export成功" : "Export失敗");
    }

    private async Task ImportRequirementsAsync()
    {
        SetStepStatus(5, "Import中");
        var result = await _pip.ImportRequirementsAsync(VenvName, WorkingDirectory);
        AddResult(result);
        SetStepStatus(5, result.IsSuccess ? "Import成功" : "Import失敗");
    }

    private async Task OpenVSCodeAsync()
    {
        SetStepStatus(6, "実行中");
        try
        {
            await _vscode.WriteSettingsAsync(WorkingDirectory, VenvName);
            AddSuccess(".vscode\\settings.json を生成しました。");
            var result = await _vscode.OpenAsync(WorkingDirectory);
            AddResult(result);
            SetStepStatus(6, result.IsSuccess ? "成功" : "設定生成済み");
        }
        catch (Exception ex)
        {
            AddError(ex.Message);
            SetStepStatus(6, "失敗");
        }
    }

    private async Task RestartShellAsync(string reason)
    {
        AddInfo($"{reason}: Shell終了 -> Shell再生成 -> ログ継続");
        await _shell.RestartAsync();
    }

    private void AddResult(CommandResult result, string commandSuffix = "")
    {
        AddCommand(result.Command + commandSuffix);
        AddText("stdout", result.StandardOutput, Brushes.White);
        AddText("stderr", result.StandardError, Brushes.IndianRed);
        if (result.IsSuccess)
        {
            AddSuccess("return code: " + result.ExitCode);
        }
        else
        {
            AddError("return code: " + result.ExitCode);
        }
    }

    private void AddCommand(string message) => Logs.Add(new LogEntry("command", message, Brushes.Cyan));
    private void AddSuccess(string message) => Logs.Add(new LogEntry("success", message, Brushes.LightGreen));
    private void AddError(string message) => Logs.Add(new LogEntry("stderr", message, Brushes.IndianRed));
    private void AddInfo(string message) => Logs.Add(new LogEntry("info", message, Brushes.LightGray));

    private void AddText(string kind, string message, Brush color)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        foreach (var line in message.Replace("\r\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            Logs.Add(new LogEntry(kind, line, color));
        }
    }

    private void SetStepStatus(int index, string status)
    {
        Steps[index].Status = status;
    }

    private void UpdateCommandTexts()
    {
        if (Steps.Count < 7)
        {
            return;
        }

        Steps[2].CommandText = _commands.Get("CreateVenv", new Dictionary<string, string> { ["venv_name"] = VenvName });
        Steps[3].CommandText = _commands.Get("UpgradePip", Replacements);
        Steps[4].CommandText = _commands.Get("InstallPackage", ReplacementsWithPackage("numpy"));
        Steps[2].SettingsSummary = VenvSummary;
        Steps[3].SettingsSummary = VenvSummary;
        Steps[6].SettingsSummary = VenvSummary;
    }

    public void RefreshSettingSummaries()
    {
        if (Steps.Count < 7)
        {
            return;
        }

        Steps[1].SettingsSummary = ProxySummary;
        Steps[2].SettingsSummary = VenvSummary;
        Steps[3].SettingsSummary = VenvSummary;
        Steps[4].SettingsSummary = PackageSummary;
        Steps[6].SettingsSummary = VenvSummary;
    }
}
