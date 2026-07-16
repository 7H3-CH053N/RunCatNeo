/*
 DashboardPopupForm.cs
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

// Borderless, non-activating dashboard popup shown while the mouse hovers
// over the tray icon — the Windows counterpart of the macOS menu-bar popover.
// It never steals focus from the foreground application.
public sealed class DashboardPopupForm : Form
{
    private const int WsExNoActivate = 0x08000000;
    private const int WsExToolWindow = 0x00000080;

    private readonly DashboardContentPanel content;

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            var parameters = base.CreateParams;
            parameters.ExStyle |= WsExNoActivate | WsExToolWindow;
            return parameters;
        }
    }

    public DashboardPopupForm(TrayAppContext context)
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        ShowInTaskbar = false;
        ClientSize = new Size(360, 580);
        BackColor = Theme.WindowBackground;
        Padding = new Padding(1);

        // Footer with quick actions, so settings are reachable without
        // fighting the popup for the tray icon's context menu.
        var footer = new Panel { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(8) };
        var settingsButton = new Button
        {
            Text = Strings.Get("settings"),
            Dock = DockStyle.Left,
            Width = 130,
        };
        settingsButton.Click += (_, _) =>
        {
            Hide();
            context.ShowSettings();
        };
        var dashboardButton = new Button
        {
            Text = Strings.Get("dashboard"),
            Dock = DockStyle.Right,
            Width = 130,
        };
        dashboardButton.Click += (_, _) =>
        {
            Hide();
            context.ShowDashboard();
        };
        footer.Controls.Add(settingsButton);
        footer.Controls.Add(dashboardButton);

        content = new DashboardContentPanel(context) { Dock = DockStyle.Fill };
        Controls.Add(content);
        Controls.Add(footer);
        Theme.Apply(this);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var border = ClientRectangle;
        border.Width -= 1;
        border.Height -= 1;
        using var pen = new Pen(Theme.Border);
        e.Graphics.DrawRectangle(pen, border);
    }

    // Shows the popup anchored to the tray area: near the cursor horizontally,
    // sitting just above (or beside) the taskbar, clamped to the working area.
    public void ShowAt(Point anchor)
    {
        var workingArea = Screen.FromPoint(anchor).WorkingArea;
        var x = Math.Clamp(anchor.X - Width / 2, workingArea.Left + 8, workingArea.Right - Width - 8);
        var y = anchor.Y < workingArea.Top + workingArea.Height / 2
            ? workingArea.Top + 8
            : workingArea.Bottom - Height - 8;
        Location = new Point(x, y);
        if (!Visible)
        {
            Show();
        }
        RefreshMetrics();
        RefreshCustomMetrics();
    }

    public void RefreshMetrics() => content.RefreshMetrics();

    public void RefreshCustomMetrics() => content.RefreshCustomMetrics();
}
