/*
 CustomMetricsCardPanel.cs
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

using System.Drawing.Drawing2D;
using RunCatNeo.Core;

namespace RunCatNeo.Windows.Forms;

// Draws one custom metrics source as a card, the Windows counterpart of
// CustomMetricsCardView: title header, "title: formattedValue" rows with an
// optional progress bar, and a relative "last updated" footer (red on failure).
public sealed class CustomMetricsCardPanel : Panel
{
    private const int PaddingSize = 10;
    private const int HeaderHeight = 22;
    private const int RowTextHeight = 18;
    private const int BarHeight = 4;
    private const int FooterHeight = 18;

    private CustomMetricsBundle bundle;

    public CustomMetricsCardPanel(CustomMetricsBundle bundle, int width)
    {
        this.bundle = bundle;
        DoubleBuffered = true;
        BackColor = Theme.PanelBackground;
        ForeColor = Theme.Text;
        BorderStyle = BorderStyle.FixedSingle;
        Width = width;
        Height = PreferredCardHeight(bundle);
    }

    public void Update(CustomMetricsBundle newBundle)
    {
        bundle = newBundle;
        Height = PreferredCardHeight(newBundle);
        Invalidate();
    }

    public static int PreferredCardHeight(CustomMetricsBundle bundle)
    {
        var rowsHeight = bundle.Snapshot.Metrics!
            .Sum(metric => RowTextHeight + (metric.NormalizedValue is null ? 0 : BarHeight + 2));
        return 2 * PaddingSize + HeaderHeight + rowsHeight + FooterHeight;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var snapshot = bundle.Snapshot;
        var x = PaddingSize;
        var y = PaddingSize;
        var contentWidth = ClientSize.Width - 2 * PaddingSize;

        using var headerFont = new Font(Font, FontStyle.Bold);
        TextRenderer.DrawText(
            graphics, snapshot.Title, headerFont,
            new Rectangle(x, y, contentWidth, HeaderHeight),
            ForeColor, TextFormatFlags.Left
        );
        y += HeaderHeight;

        foreach (var metric in snapshot.Metrics!)
        {
            TextRenderer.DrawText(
                graphics, $"{metric.Title}: {metric.FormattedValue}", Font,
                new Rectangle(x, y, contentWidth, RowTextHeight),
                ForeColor, TextFormatFlags.Left
            );
            y += RowTextHeight;
            if (metric.NormalizedValue is { } normalizedValue)
            {
                var clamped = (float)Math.Clamp(normalizedValue, 0.0, 1.0);
                using var trackBrush = new SolidBrush(Theme.GraphTrack);
                graphics.FillRectangle(trackBrush, x, y, contentWidth, BarHeight);
                using var barBrush = new SolidBrush(Color.SteelBlue);
                graphics.FillRectangle(barBrush, x, y, contentWidth * clamped, BarHeight);
                y += BarHeight + 2;
            }
        }

        var footerText = bundle.IsFailed
            ? $"{Strings.Get("lastUpdated")}: {Strings.Get("failed")}"
            : $"{Strings.Get("lastUpdated")}: {RelativeTime.Format(snapshot.LastUpdatedDate!.Value)}";
        TextRenderer.DrawText(
            graphics, footerText, Font,
            new Rectangle(x, y, contentWidth, FooterHeight),
            bundle.IsFailed ? Theme.ErrorText : Theme.SubtleText, TextFormatFlags.Left
        );
    }
}
