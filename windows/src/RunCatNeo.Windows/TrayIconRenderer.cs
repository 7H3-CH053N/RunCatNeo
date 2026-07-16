/*
 TrayIconRenderer.cs
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
using System.Runtime.InteropServices;

namespace RunCatNeo.Windows;

public static class TrayIconRenderer
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr handle);

    // Renders one animation frame into a tray-sized icon. Template frames are
    // recolored to the tint color (the Windows analogue of macOS template images).
    public static Icon RenderIcon(Bitmap frame, Size iconSize, Color tintColor, bool isTemplate, bool isFlipped)
    {
        using var canvas = new Bitmap(iconSize.Width, iconSize.Height, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(canvas))
        {
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            var scale = Math.Min(
                (float)iconSize.Width / frame.Width,
                (float)iconSize.Height / frame.Height
            );
            var width = frame.Width * scale;
            var height = frame.Height * scale;
            var x = (iconSize.Width - width) / 2f;
            var y = (iconSize.Height - height) / 2f;
            if (isFlipped)
            {
                graphics.TranslateTransform(iconSize.Width, 0f);
                graphics.ScaleTransform(-1f, 1f);
            }
            graphics.DrawImage(frame, x, y, width, height);
        }
        if (isTemplate)
        {
            Tint(canvas, tintColor);
        }
        var iconHandle = canvas.GetHicon();
        try
        {
            using var unmanagedIcon = Icon.FromHandle(iconHandle);
            return (Icon)unmanagedIcon.Clone();
        }
        finally
        {
            DestroyIcon(iconHandle);
        }
    }

    private static void Tint(Bitmap bitmap, Color tintColor)
    {
        var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        try
        {
            var length = data.Stride * data.Height;
            var pixels = new byte[length];
            Marshal.Copy(data.Scan0, pixels, 0, length);
            for (var i = 0; i < length; i += 4)
            {
                if (pixels[i + 3] == 0)
                {
                    continue;
                }
                pixels[i] = tintColor.B;
                pixels[i + 1] = tintColor.G;
                pixels[i + 2] = tintColor.R;
            }
            Marshal.Copy(pixels, 0, data.Scan0, length);
        }
        finally
        {
            bitmap.UnlockBits(data);
        }
    }
}
