/*
 TrayAppContext.cs
 RunCatNeo.Windows

 Copyright 2026 Kyome22 (Takuto Nakamura)

 Licensed under the Apache License, Version 2.0 (the "License");
 you may not use this file except in compliance with the License.
 You may obtain a copy of the License at

 http://www.apache.org/licenses/LICENSE-2.0

 Unless required by applicable law or agreed to in writing, software
 distributed under the License is distributed on an "AS IS" BASIS,
 WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 See the License for the specific language governing permissions and
 limitations under the License.
 */

using System.Diagnostics;
using Microsoft.Win32;
using RunCatNeo.Core;
using RunCatNeo.Windows.Forms;
using RunCatNeo.Windows.SystemInfo;

namespace RunCatNeo.Windows;

public sealed class TrayAppContext : ApplicationContext
{
    private static readonly string AppDataDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RunCatNeo");

    private readonly SettingsStore settingsStore;
    private readonly SystemInfoObserver systemInfoObserver = new();
    private readonly RunnerFrameProvider frameProvider;
    private readonly NotifyIcon notifyIcon;
    private readonly System.Windows.Forms.Timer animationTimer = new();
    private readonly System.Windows.Forms.Timer metricsTimer = new();
    private readonly RingBuffer cpuRingBuffer = new();
    private readonly RingBuffer memoryRingBuffer = new();

    private Runner currentRunner = Runner.Default;
    private IReadOnlyList<Icon> icons = [];
    private int frameIndex;
    private SystemInfoBundle latestBundle = new();
    private DashboardForm? dashboardForm;
    private SettingsForm? settingsForm;

    public AppSettings Settings => settingsStore.Settings;
    public CustomMetricsService CustomMetricsService { get; } = new();
    public SystemInfoBundle LatestBundle => latestBundle;
    public RingBuffer CpuRingBuffer => cpuRingBuffer;
    public RingBuffer MemoryRingBuffer => memoryRingBuffer;
    public RunnerFrameProvider FrameProvider => frameProvider;
    public Runner CurrentRunner => currentRunner;

    public TrayAppContext()
    {
        settingsStore = new SettingsStore(Path.Combine(AppDataDirectory, "settings.json"));
        frameProvider = new RunnerFrameProvider(
            Path.Combine(AppContext.BaseDirectory, "Resources", "Runners"),
            new CustomRunnerRepository(Path.Combine(AppDataDirectory, "Runners"))
        );

        notifyIcon = new NotifyIcon
        {
            Text = "RunCat Neo",
            Visible = true,
            ContextMenuStrip = BuildMenu(),
        };
        notifyIcon.DoubleClick += (_, _) => ShowDashboard();

        ApplySystemMetricsConfiguration();
        ApplyRunner(ResolveConfiguredRunner());
        CustomMetricsService.Configure(Settings.CustomMetricsConfiguration);
        CustomMetricsService.BundlesChanged += () => dashboardForm?.RefreshCustomMetrics();

        animationTimer.Tick += (_, _) => AdvanceFrame();
        metricsTimer.Tick += (_, _) => UpdateMetrics();
        metricsTimer.Interval = Settings.UpdateInterval.Seconds() * 1000;
        metricsTimer.Start();
        UpdateMetrics();

        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        Application.ApplicationExit += (_, _) => Cleanup();
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add(Strings.Get("dashboard"), null, (_, _) => ShowDashboard());
        menu.Items.Add(Strings.Get("settings"), null, (_, _) => ShowSettings());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(Strings.Get("openTaskManager"), null, (_, _) => OpenTaskManager());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(Strings.Get("about"), null, (_, _) => ShowAbout());
        menu.Items.Add(Strings.Get("reportIssue"), null, (_, _) => OpenUrl("https://github.com/runcat-dev/RunCatNeo/issues"));
        menu.Items.Add(Strings.Get("quit"), null, (_, _) => ExitThread());
        return menu;
    }

    private Runner ResolveConfiguredRunner()
    {
        var runnerId = Settings.RunnerId;
        if (RunnerKindExtensions.FromId(runnerId) is { } kind)
        {
            return new Runner(kind);
        }
        var customRunner = frameProvider.LoadAllRunners().FirstOrDefault(runner => runner.Id == runnerId);
        return customRunner ?? Runner.Default;
    }

    public void ApplyRunner(Runner runner)
    {
        IReadOnlyList<Bitmap> frames;
        try
        {
            frames = frameProvider.LoadFrames(runner);
        }
        catch (FileNotFoundException) when (runner.Id != Runner.Default.Id)
        {
            ApplyRunner(Runner.Default);
            return;
        }
        currentRunner = runner;
        Settings.RunnerId = runner.Id;
        settingsStore.Save();
        RebuildIcons(frames);
        foreach (var frame in frames.Distinct())
        {
            frame.Dispose();
        }
    }

    private void RebuildIcons(IReadOnlyList<Bitmap> frames)
    {
        var oldIcons = icons;
        var iconSize = SystemInformation.SmallIconSize;
        icons = frames
            .Select(frame => TrayIconRenderer.RenderIcon(
                frame,
                iconSize,
                Theme.TrayTintColor(),
                currentRunner.IsTemplate,
                Settings.IsFlippedHorizontally
            ))
            .ToArray();
        frameIndex = 0;
        AdvanceFrame();
        foreach (var icon in oldIcons)
        {
            icon.Dispose();
        }
    }

    public void RefreshIcons()
    {
        ApplyRunner(currentRunner);
    }

    private void AdvanceFrame()
    {
        if (icons.Count == 0)
        {
            return;
        }
        notifyIcon.Icon = icons[frameIndex % icons.Count];
        frameIndex = (frameIndex + 1) % icons.Count;
    }

    private void UpdateMetrics()
    {
        latestBundle = systemInfoObserver.Sample();
        cpuRingBuffer.Append(latestBundle.CpuPercentage);
        if (latestBundle.MemoryInfo is { } memoryInfo)
        {
            memoryRingBuffer.Append(memoryInfo.Percentage);
        }
        var speed = RunnerSpeed.Calculate(latestBundle.CpuPercentage, Settings.SpeedDecreasesUnderLoad);
        animationTimer.Interval = Math.Max(15, (int)RunnerSpeed.FrameInterval(speed).TotalMilliseconds);
        if (!animationTimer.Enabled)
        {
            animationTimer.Start();
        }
        notifyIcon.Text = $"RunCat Neo — CPU {latestBundle.CpuPercentage:F1} %";
        dashboardForm?.RefreshMetrics();
    }

    public void ApplyUpdateInterval()
    {
        metricsTimer.Interval = Settings.UpdateInterval.Seconds() * 1000;
        settingsStore.Save();
    }

    public void ApplySystemMetricsConfiguration()
    {
        var configuration = Settings.SystemMetricsConfiguration;
        systemInfoObserver.MonitorsMemory = configuration.MonitorsMemory;
        systemInfoObserver.MonitorsStorage = configuration.MonitorsStorage;
        systemInfoObserver.MonitorsBattery = configuration.MonitorsBattery;
        systemInfoObserver.MonitorsNetwork = configuration.MonitorsNetwork;
    }

    public void SaveSettings()
    {
        settingsStore.Save();
    }

    public void AddCustomMetricsSource(string filePath)
    {
        var source = new CustomMetricsSource
        {
            DisplayName = Path.GetFileNameWithoutExtension(filePath),
            FilePath = filePath,
            CreatedAt = DateTimeOffset.Now,
        };
        var configuration = Settings.CustomMetricsConfiguration;
        Settings.CustomMetricsConfiguration = configuration with
        {
            Sources = [.. configuration.Sources, source],
        };
        settingsStore.Save();
        CustomMetricsService.Configure(Settings.CustomMetricsConfiguration);
    }

    public void RemoveCustomMetricsSource(Guid sourceId)
    {
        var configuration = Settings.CustomMetricsConfiguration;
        Settings.CustomMetricsConfiguration = configuration with
        {
            Sources = configuration.Sources.Where(source => source.Id != sourceId).ToArray(),
        };
        settingsStore.Save();
        CustomMetricsService.Configure(Settings.CustomMetricsConfiguration);
    }

    private void ShowDashboard()
    {
        if (dashboardForm is null || dashboardForm.IsDisposed)
        {
            dashboardForm = new DashboardForm(this);
        }
        dashboardForm.Show();
        dashboardForm.Activate();
    }

    private void ShowSettings()
    {
        if (settingsForm is null || settingsForm.IsDisposed)
        {
            settingsForm = new SettingsForm(this);
        }
        settingsForm.Show();
        settingsForm.Activate();
    }

    private static void OpenTaskManager()
    {
        try
        {
            Process.Start(new ProcessStartInfo("taskmgr.exe") { UseShellExecute = true });
        }
        catch (Exception exception) when (exception is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            // Task Manager can be disabled by policy; there is nothing sensible to do here.
        }
    }

    private static void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    private static void ShowAbout()
    {
        MessageBox.Show(Strings.Get("aboutBody"), Strings.Get("about"), MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category == UserPreferenceCategory.General || e.Category == UserPreferenceCategory.Color)
        {
            RefreshIcons();
        }
    }

    private void Cleanup()
    {
        SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
        CustomMetricsService.Dispose();
        animationTimer.Stop();
        metricsTimer.Stop();
        notifyIcon.Visible = false;
        notifyIcon.Dispose();
        foreach (var icon in icons)
        {
            icon.Dispose();
        }
    }
}
