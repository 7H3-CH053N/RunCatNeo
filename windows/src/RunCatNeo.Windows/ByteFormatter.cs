/*
 ByteFormatter.cs
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

namespace RunCatNeo.Windows;

public static class ByteFormatter
{
    private static readonly string[] Units = ["B", "KB", "MB", "GB", "TB"];

    public static string Format(double bytes)
    {
        var value = Math.Max(0.0, bytes);
        var unitIndex = 0;
        while (value >= 1000.0 && unitIndex < Units.Length - 1)
        {
            value /= 1000.0;
            unitIndex += 1;
        }
        return $"{value:F1} {Units[unitIndex]}";
    }

    public static string FormatPerSecond(double bytesPerSecond) => $"{Format(bytesPerSecond)}/s";
}
