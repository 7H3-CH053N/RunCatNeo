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
    private readonly DashboardContentPanel content;

    public DashboardForm(TrayAppContext context)
    {
        Text = $"RunCat Neo — {Strings.Get("dashboard")}";
        FormBorderStyle = FormBorderStyle.FixedToolWindow;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(380, 560);
        ShowInTaskbar = false;
        content = new DashboardContentPanel(context) { Dock = DockStyle.Fill };
        Controls.Add(content);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        RefreshMetrics();
        RefreshCustomMetrics();
    }

    public void RefreshMetrics() => content.RefreshMetrics();

    public void RefreshCustomMetrics() => content.RefreshCustomMetrics();
}
