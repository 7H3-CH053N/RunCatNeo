/*
 LineGraphPanel.cs
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

// Draws a ring buffer of percentages (0-100) as a filled line graph,
// the Windows counterpart of the dashboard's LineGraphView.
public sealed class LineGraphPanel : Panel
{
    public RingBuffer? RingBuffer { get; set; }
    public Color GraphColor { get; set; } = Color.SteelBlue;

    public LineGraphPanel()
    {
        DoubleBuffered = true;
        BackColor = Theme.PanelBackground;
        BorderStyle = BorderStyle.FixedSingle;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var values = RingBuffer?.Values;
        if (values is null || values.Count < 2)
        {
            return;
        }
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var width = (float)ClientSize.Width;
        var height = (float)ClientSize.Height;
        var stepX = width / (values.Count - 1);
        var points = new PointF[values.Count + 2];
        for (var i = 0; i < values.Count; i++)
        {
            var y = height * (1f - (float)Math.Clamp(values[i], 0.0, 100.0) / 100f);
            points[i] = new PointF(i * stepX, y);
        }
        points[values.Count] = new PointF(width, height);
        points[values.Count + 1] = new PointF(0f, height);
        using var fillBrush = new SolidBrush(Color.FromArgb(64, GraphColor));
        e.Graphics.FillPolygon(fillBrush, points);
        using var linePen = new Pen(GraphColor, 1.5f);
        e.Graphics.DrawLines(linePen, points.AsSpan(0, values.Count).ToArray());
    }
}
