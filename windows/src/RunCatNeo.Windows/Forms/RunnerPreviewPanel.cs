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

namespace RunCatNeo.Windows.Forms;

// Animates a runner's frames at the base speed, mirroring RunnerPreviewView.
public sealed class RunnerPreviewPanel : Panel
{
    private readonly System.Windows.Forms.Timer timer = new();
    private IReadOnlyList<Bitmap> frames = [];
    private int frameIndex;

    public bool IsFlipped { get; set; }

    public RunnerPreviewPanel()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(245, 245, 245);
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

    public void SetFrames(IReadOnlyList<Bitmap> newFrames)
    {
        var oldFrames = frames.Distinct().ToArray();
        frames = newFrames;
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
        e.Graphics.DrawImage(frame, x, y, width, height);
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
