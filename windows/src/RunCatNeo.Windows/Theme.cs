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

using Microsoft.Win32;

namespace RunCatNeo.Windows;

public static class Theme
{
    // The taskbar follows the system (not the apps) theme setting.
    public static bool IsTaskbarLight()
    {
        using var key = Registry.CurrentUser.OpenSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"
        );
        return key?.GetValue("SystemUsesLightTheme") is int value && value != 0;
    }

    public static Color TrayTintColor() => IsTaskbarLight() ? Color.Black : Color.White;
}
