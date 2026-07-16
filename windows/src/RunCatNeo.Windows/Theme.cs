/*
 Theme.cs
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

using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace RunCatNeo.Windows;

public static class Theme
{
    private static bool ReadsLightTheme(string valueName)
    {
        using var key = Registry.CurrentUser.OpenSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"
        );
        // The value is absent on default installations, which means light.
        return key?.GetValue(valueName) is not int value || value != 0;
    }

    // The taskbar follows the system (not the apps) theme setting.
    public static bool IsTaskbarLight() => ReadsLightTheme("SystemUsesLightTheme");

    // App windows follow the apps theme setting.
    public static bool IsAppsLight() => ReadsLightTheme("AppsUseLightTheme");

    public static Color TrayTintColor() => IsTaskbarLight() ? Color.Black : Color.White;

    public static Color WindowBackground => IsAppsLight() ? SystemColors.Window : Color.FromArgb(30, 30, 30);
    public static Color PanelBackground => IsAppsLight() ? Color.FromArgb(245, 245, 245) : Color.FromArgb(43, 43, 43);
    public static Color ControlBackground => IsAppsLight() ? SystemColors.Window : Color.FromArgb(45, 45, 45);
    public static Color Text => IsAppsLight() ? SystemColors.ControlText : Color.FromArgb(240, 240, 240);
    public static Color SubtleText => IsAppsLight() ? Color.Gray : Color.FromArgb(157, 157, 157);
    public static Color Border => IsAppsLight() ? SystemColors.ControlDark : Color.FromArgb(69, 69, 69);
    public static Color GraphTrack => IsAppsLight() ? Color.FromArgb(220, 220, 220) : Color.FromArgb(63, 63, 63);
    public static Color ErrorText => IsAppsLight() ? Color.Firebrick : Color.FromArgb(235, 110, 100);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    // Renders the window title bar dark to match the apps theme (Windows 10 1809+).
    public static void ApplyTitleBar(Form form)
    {
        const int UseImmersiveDarkMode = 20;
        var darkMode = IsAppsLight() ? 0 : 1;
        _ = DwmSetWindowAttribute(form.Handle, UseImmersiveDarkMode, ref darkMode, sizeof(int));
    }

    // Applies the theme palette to a control tree. Call after all children are added.
    public static void Apply(Control root)
    {
        root.BackColor = WindowBackground;
        root.ForeColor = Text;
        ApplyToChildren(root);
    }

    private static void ApplyToChildren(Control parent)
    {
        foreach (Control control in parent.Controls)
        {
            switch (control)
            {
                case ListBox or ComboBox or TextBox:
                    control.BackColor = ControlBackground;
                    control.ForeColor = Text;
                    break;
                case Button button:
                    button.BackColor = PanelBackground;
                    button.ForeColor = Text;
                    if (!IsAppsLight())
                    {
                        button.FlatStyle = FlatStyle.Flat;
                        button.FlatAppearance.BorderColor = Border;
                    }
                    break;
                default:
                    control.ForeColor = Text;
                    break;
            }
            ApplyToChildren(control);
        }
    }
}
