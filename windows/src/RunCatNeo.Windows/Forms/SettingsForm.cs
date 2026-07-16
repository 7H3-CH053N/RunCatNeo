/*
 SettingsForm.cs
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

using RunCatNeo.Core;

namespace RunCatNeo.Windows.Forms;

public sealed class SettingsForm : Form
{
    private readonly TrayAppContext context;
    private readonly ListBox runnerListBox = new();
    private readonly RunnerPreviewPanel previewPanel = new();
    private readonly CheckBox launchAtLoginCheckBox = new();
    private readonly CheckBox speedDecreasesCheckBox = new();
    private readonly CheckBox flipCheckBox = new();
    private readonly ComboBox updateIntervalComboBox = new();
    private readonly CheckBox monitorsMemoryCheckBox = new();
    private readonly CheckBox monitorsStorageCheckBox = new();
    private readonly CheckBox monitorsBatteryCheckBox = new();
    private readonly CheckBox monitorsNetworkCheckBox = new();
    private readonly ListBox customMetricsListBox = new();
    private readonly Button addSourceButton = new();
    private readonly Button removeSourceButton = new();

    private IReadOnlyList<Runner> runners = [];
    private bool isLoading = true;

    public SettingsForm(TrayAppContext context)
    {
        this.context = context;
        Text = $"RunCat Neo — {Strings.Get("settings")}";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(460, 640);
        ShowInTaskbar = false;

        BuildLayout();
        Theme.Apply(this);
        LoadState();
        isLoading = false;
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        Theme.ApplyTitleBar(this);
    }

    private void BuildLayout()
    {
        var runnerGroup = new GroupBox
        {
            Text = Strings.Get("runner"),
            Bounds = new Rectangle(12, 12, 436, 190),
        };
        runnerListBox.Bounds = new Rectangle(12, 24, 220, 150);
        runnerListBox.SelectedIndexChanged += (_, _) => OnRunnerSelected();
        previewPanel.Bounds = new Rectangle(248, 24, 176, 150);
        runnerGroup.Controls.Add(runnerListBox);
        runnerGroup.Controls.Add(previewPanel);

        var generalGroup = new GroupBox
        {
            Text = Strings.Get("general"),
            Bounds = new Rectangle(12, 210, 436, 140),
        };
        launchAtLoginCheckBox.Text = Strings.Get("launchAtLogin");
        launchAtLoginCheckBox.Bounds = new Rectangle(12, 24, 400, 24);
        launchAtLoginCheckBox.CheckedChanged += (_, _) => Apply(() => LaunchAtLogin.IsEnabled = launchAtLoginCheckBox.Checked);
        speedDecreasesCheckBox.Text = Strings.Get("speedDecreasesUnderLoad");
        speedDecreasesCheckBox.Bounds = new Rectangle(12, 50, 400, 24);
        speedDecreasesCheckBox.CheckedChanged += (_, _) => Apply(() =>
        {
            context.Settings.SpeedDecreasesUnderLoad = speedDecreasesCheckBox.Checked;
            context.SaveSettings();
        });
        flipCheckBox.Text = Strings.Get("flipHorizontally");
        flipCheckBox.Bounds = new Rectangle(12, 76, 400, 24);
        flipCheckBox.CheckedChanged += (_, _) => Apply(() =>
        {
            context.Settings.IsFlippedHorizontally = flipCheckBox.Checked;
            context.SaveSettings();
            context.RefreshIcons();
            previewPanel.IsFlipped = flipCheckBox.Checked;
        });
        var updateIntervalLabel = new Label
        {
            Text = $"{Strings.Get("updateInterval")}:",
            Bounds = new Rectangle(12, 106, 160, 24),
            TextAlign = ContentAlignment.MiddleLeft,
        };
        updateIntervalComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        updateIntervalComboBox.Bounds = new Rectangle(180, 104, 120, 24);
        foreach (var interval in UpdateIntervalExtensions.AllCases)
        {
            updateIntervalComboBox.Items.Add($"{interval.Seconds()} {Strings.Get("seconds")}");
        }
        updateIntervalComboBox.SelectedIndexChanged += (_, _) => Apply(() =>
        {
            context.Settings.UpdateInterval = UpdateIntervalExtensions.AllCases[updateIntervalComboBox.SelectedIndex];
            context.ApplyUpdateInterval();
        });
        generalGroup.Controls.Add(launchAtLoginCheckBox);
        generalGroup.Controls.Add(speedDecreasesCheckBox);
        generalGroup.Controls.Add(flipCheckBox);
        generalGroup.Controls.Add(updateIntervalLabel);
        generalGroup.Controls.Add(updateIntervalComboBox);

        var metricsGroup = new GroupBox
        {
            Text = "Metrics",
            Bounds = new Rectangle(12, 358, 436, 100),
        };
        monitorsMemoryCheckBox.Text = Strings.Get("monitorsMemory");
        monitorsMemoryCheckBox.Bounds = new Rectangle(12, 24, 200, 24);
        monitorsStorageCheckBox.Text = Strings.Get("monitorsStorage");
        monitorsStorageCheckBox.Bounds = new Rectangle(224, 24, 200, 24);
        monitorsBatteryCheckBox.Text = Strings.Get("monitorsBattery");
        monitorsBatteryCheckBox.Bounds = new Rectangle(12, 56, 200, 24);
        monitorsNetworkCheckBox.Text = Strings.Get("monitorsNetwork");
        monitorsNetworkCheckBox.Bounds = new Rectangle(224, 56, 200, 24);
        foreach (var checkBox in new[] { monitorsMemoryCheckBox, monitorsStorageCheckBox, monitorsBatteryCheckBox, monitorsNetworkCheckBox })
        {
            checkBox.CheckedChanged += (_, _) => Apply(ApplyMetricsConfiguration);
            metricsGroup.Controls.Add(checkBox);
        }

        var customMetricsGroup = new GroupBox
        {
            Text = Strings.Get("customMetrics"),
            Bounds = new Rectangle(12, 466, 436, 160),
        };
        customMetricsListBox.Bounds = new Rectangle(12, 24, 412, 90);
        customMetricsListBox.HorizontalScrollbar = true;
        addSourceButton.Text = Strings.Get("addJsonSource");
        addSourceButton.Bounds = new Rectangle(12, 122, 180, 26);
        addSourceButton.Click += (_, _) => OnAddCustomMetricsSource();
        removeSourceButton.Text = Strings.Get("removeSource");
        removeSourceButton.Bounds = new Rectangle(200, 122, 120, 26);
        removeSourceButton.Click += (_, _) => OnRemoveCustomMetricsSource();
        customMetricsGroup.Controls.Add(customMetricsListBox);
        customMetricsGroup.Controls.Add(addSourceButton);
        customMetricsGroup.Controls.Add(removeSourceButton);

        Controls.Add(runnerGroup);
        Controls.Add(generalGroup);
        Controls.Add(metricsGroup);
        Controls.Add(customMetricsGroup);
    }

    private void RefreshCustomMetricsSources()
    {
        customMetricsListBox.Items.Clear();
        foreach (var source in context.Settings.CustomMetricsConfiguration.Sources)
        {
            var errorSuffix = context.CustomMetricsService.HasError(source.Id)
                ? $"  {Strings.Get("errorDetected")}"
                : "";
            customMetricsListBox.Items.Add($"{source.DisplayName} — {source.FilePath}{errorSuffix}");
        }
    }

    private void OnAddCustomMetricsSource()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "JSON (*.json)|*.json|All files (*.*)|*.*",
            CheckFileExists = true,
        };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            context.AddCustomMetricsSource(dialog.FileName);
            RefreshCustomMetricsSources();
        }
    }

    private void OnRemoveCustomMetricsSource()
    {
        var index = customMetricsListBox.SelectedIndex;
        var sources = context.Settings.CustomMetricsConfiguration.Sources;
        if (index < 0 || index >= sources.Count)
        {
            return;
        }
        context.RemoveCustomMetricsSource(sources[index].Id);
        RefreshCustomMetricsSources();
    }

    private void LoadState()
    {
        runners = context.FrameProvider.LoadAllRunners();
        runnerListBox.Items.Clear();
        foreach (var runner in runners)
        {
            runnerListBox.Items.Add(runner.Name);
        }
        var currentIndex = runners.ToList().FindIndex(runner => runner.Id == context.CurrentRunner.Id);
        runnerListBox.SelectedIndex = Math.Max(0, currentIndex);

        launchAtLoginCheckBox.Checked = LaunchAtLogin.IsEnabled;
        speedDecreasesCheckBox.Checked = context.Settings.SpeedDecreasesUnderLoad;
        flipCheckBox.Checked = context.Settings.IsFlippedHorizontally;
        updateIntervalComboBox.SelectedIndex =
            Array.IndexOf(UpdateIntervalExtensions.AllCases, context.Settings.UpdateInterval);

        var configuration = context.Settings.SystemMetricsConfiguration;
        monitorsMemoryCheckBox.Checked = configuration.MonitorsMemory;
        monitorsStorageCheckBox.Checked = configuration.MonitorsStorage;
        monitorsBatteryCheckBox.Checked = configuration.MonitorsBattery;
        monitorsNetworkCheckBox.Checked = configuration.MonitorsNetwork;
        previewPanel.IsFlipped = context.Settings.IsFlippedHorizontally;
        RefreshCustomMetricsSources();
    }

    private void Apply(Action action)
    {
        if (!isLoading)
        {
            action();
        }
    }

    private void OnRunnerSelected()
    {
        if (runnerListBox.SelectedIndex < 0 || runnerListBox.SelectedIndex >= runners.Count)
        {
            return;
        }
        var runner = runners[runnerListBox.SelectedIndex];
        try
        {
            previewPanel.SetFrames(context.FrameProvider.LoadFrames(runner), runner.IsTemplate);
        }
        catch (FileNotFoundException)
        {
            previewPanel.SetFrames([]);
        }
        if (!isLoading)
        {
            context.ApplyRunner(runner);
        }
    }

    private void ApplyMetricsConfiguration()
    {
        context.Settings.SystemMetricsConfiguration = new SystemMetricsConfiguration
        {
            MonitorsMemory = monitorsMemoryCheckBox.Checked,
            MonitorsStorage = monitorsStorageCheckBox.Checked,
            MonitorsBattery = monitorsBatteryCheckBox.Checked,
            MonitorsNetwork = monitorsNetworkCheckBox.Checked,
        };
        context.ApplySystemMetricsConfiguration();
        context.SaveSettings();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            previewPanel.SetFrames([]);
        }
        base.Dispose(disposing);
    }
}
