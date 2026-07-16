/*
 RunnerPreviewPanel.cs
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
using System.Drawing.Imaging;

namespace RunCatNeo.Windows.Forms;

// Animates a runner's frames at the base speed, mirroring RunnerPreviewView.
public sealed class RunnerPreviewPanel : Panel
{
    // Recolors every pixel to white while preserving alpha, so template frames
    // (black shapes) stay visible on the dark theme's background.
    private static readonly ImageAttributes WhiteTint = CreateWhiteTint();

    private static ImageAttributes CreateWhiteTint()
    {
        var attributes = new ImageAttributes();
        attributes.SetColorMatrix(new ColorMatrix([
            [0f, 0f, 0f, 0f, 0f],
            [0f, 0f, 0f, 0f, 0f],
            [0f, 0f, 0f, 0f, 0f],
            [0f, 0f, 0f, 1f, 0f],
            [1f, 1f, 1f, 0f, 1f],
        ]));
        return attributes;
    }

    private readonly System.Windows.Forms.Timer timer = new();
    private IReadOnlyList<Bitmap> frames = [];
    private int frameIndex;
    private bool isTemplate = true;

    public bool IsFlipped { get; set; }

    public RunnerPreviewPanel()
    {
        DoubleBuffered = true;
        BackColor = Theme.PanelBackground;
        BorderStyle = BorderStyle.FixedSingle;
        timer.Interval = (int)(RunCatNeo.Core.RunnerSpeed.BaseFrameDuration * 1000);
        timer.Tick += (_, _) =>
        {
            if (frames.Count > 0)
            {
                frameIndex = (frameIndex + 1) % frames.Count;
                Invalidate();
            }
        };
        timer.Start();
    }

    public void SetFrames(IReadOnlyList<Bitmap> newFrames, bool isTemplate = true)
    {
        var oldFrames = frames.Distinct().ToArray();
        frames = newFrames;
        this.isTemplate = isTemplate;
        frameIndex = 0;
        Invalidate();
        foreach (var frame in oldFrames)
        {
            frame.Dispose();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (frames.Count == 0)
        {
            return;
        }
        var frame = frames[frameIndex % frames.Count];
        e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
        var scale = Math.Min(
            (float)ClientSize.Width / frame.Width,
            (float)ClientSize.Height / frame.Height
        ) * 0.6f;
        var width = frame.Width * scale;
        var height = frame.Height * scale;
        var x = (ClientSize.Width - width) / 2f;
        var y = (ClientSize.Height - height) / 2f;
        if (IsFlipped)
        {
            e.Graphics.TranslateTransform(ClientSize.Width, 0f);
            e.Graphics.ScaleTransform(-1f, 1f);
        }
        if (isTemplate && !Theme.IsAppsLight())
        {
            var destination = new RectangleF(x, y, width, height);
            e.Graphics.DrawImage(
                frame,
                [destination.Location,
                 new PointF(destination.Right, destination.Top),
                 new PointF(destination.Left, destination.Bottom)],
                new RectangleF(0f, 0f, frame.Width, frame.Height),
                GraphicsUnit.Pixel,
                WhiteTint
            );
        }
        else
        {
            e.Graphics.DrawImage(frame, x, y, width, height);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            timer.Dispose();
        }
        base.Dispose(disposing);
    }
}
