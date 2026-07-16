/*
 DashboardForm.cs
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

namespace RunCatNeo.Windows.Forms;

public sealed class DashboardForm : Form
{
    private readonly TrayAppContext context;
    private readonly Label cpuLabel = new();
    private readonly LineGraphPanel cpuGraph = new();
    private readonly Label memoryLabel = new();
    private readonly LineGraphPanel memoryGraph = new();
    private readonly Label storageLabel = new();
    private readonly Label networkLabel = new();
    private readonly Label batteryLabel = new();

    public DashboardForm(TrayAppContext context)
    {
        this.context = context;
        Text = $"RunCat Neo — {Strings.Get("dashboard")}";
        FormBorderStyle = FormBorderStyle.FixedToolWindow;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(380, 430);
        ShowInTaskbar = false;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            Padding = new Padding(12),
        };
        void AddRow(Control control, int height)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
            control.Dock = DockStyle.Fill;
            layout.Controls.Add(control);
        }

        cpuLabel.Font = new Font(Font, FontStyle.Bold);
        memoryLabel.Font = new Font(Font, FontStyle.Bold);
        cpuGraph.GraphColor = Color.SteelBlue;
        memoryGraph.GraphColor = Color.MediumSeaGreen;

        AddRow(cpuLabel, 24);
        AddRow(cpuGraph, 110);
        AddRow(memoryLabel, 24);
        AddRow(memoryGraph, 110);
        AddRow(storageLabel, 24);
        AddRow(networkLabel, 24);
        AddRow(batteryLabel, 24);
        Controls.Add(layout);

        RefreshMetrics();
    }

    public void RefreshMetrics()
    {
        var bundle = context.LatestBundle;
        cpuLabel.Text = $"{Strings.Get("cpu")}: {bundle.CpuPercentage:F1} %";
        cpuGraph.RingBuffer = context.CpuRingBuffer;
        cpuGraph.Invalidate();

        if (bundle.MemoryInfo is { } memoryInfo)
        {
            memoryLabel.Text = $"{Strings.Get("memory")}: {memoryInfo.Percentage:F1} % " +
                $"({ByteFormatter.Format(memoryInfo.UsedBytes)} / {ByteFormatter.Format(memoryInfo.TotalBytes)})";
        }
        else
        {
            memoryLabel.Text = $"{Strings.Get("memory")}: —";
        }
        memoryGraph.RingBuffer = context.MemoryRingBuffer;
        memoryGraph.Invalidate();

        storageLabel.Text = bundle.StorageInfo is { } storageInfo
            ? $"{Strings.Get("storage")}: {storageInfo.Percentage:F1} % {Strings.Get("used")} " +
                $"({ByteFormatter.Format(storageInfo.AvailableBytes)} {Strings.Get("free")})"
            : $"{Strings.Get("storage")}: —";

        networkLabel.Text = bundle.NetworkInfo is { } networkInfo
            ? $"{Strings.Get("network")}: ↑ {ByteFormatter.FormatPerSecond(networkInfo.UploadBytesPerSecond)}   " +
                $"↓ {ByteFormatter.FormatPerSecond(networkInfo.DownloadBytesPerSecond)}"
            : $"{Strings.Get("network")}: —";

        batteryLabel.Text = bundle.BatteryInfo is { } batteryInfo
            ? $"{Strings.Get("battery")}: {batteryInfo.Percentage:F0} %" +
                (batteryInfo.IsCharging ? $" ({Strings.Get("charging")})" : "")
            : $"{Strings.Get("battery")}: —";
    }
}
