/*
 Strings.cs
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

using System.Globalization;

namespace RunCatNeo.Windows;

public static class Strings
{
    private static readonly Dictionary<string, string> English = new()
    {
        ["dashboard"] = "Dashboard",
        ["settings"] = "Settings",
        ["openTaskManager"] = "Open Task Manager",
        ["about"] = "About RunCat Neo",
        ["reportIssue"] = "Report Issue",
        ["quit"] = "Quit RunCat Neo",
        ["cpu"] = "CPU",
        ["memory"] = "Memory",
        ["storage"] = "Storage",
        ["network"] = "Network",
        ["battery"] = "Battery",
        ["charging"] = "Charging",
        ["used"] = "Used",
        ["free"] = "Free",
        ["upload"] = "Upload",
        ["download"] = "Download",
        ["general"] = "General",
        ["runner"] = "Runner",
        ["launchAtLogin"] = "Launch at login",
        ["speedDecreasesUnderLoad"] = "Speed decreases under load",
        ["flipHorizontally"] = "Flip runner horizontally",
        ["updateInterval"] = "Update interval",
        ["seconds"] = "seconds",
        ["monitorsMemory"] = "Monitor memory",
        ["monitorsStorage"] = "Monitor storage",
        ["monitorsBattery"] = "Monitor battery",
        ["monitorsNetwork"] = "Monitor network",
        ["customMetrics"] = "Custom Metrics",
        ["addJsonSource"] = "Add JSON Source…",
        ["removeSource"] = "Remove",
        ["errorDetected"] = "⚠ Error Detected",
        ["lastUpdated"] = "Last updated",
        ["failed"] = "Failed",
        ["justNow"] = "just now",
        ["minutesAgo"] = "{0} min ago",
        ["hoursAgo"] = "{0} h ago",
        ["daysAgo"] = "{0} d ago",
        ["aboutBody"] = "RunCat Neo for Windows\nA cute running cat animation in your system tray.\n\nThis app is open-source software.\nhttps://github.com/runcat-dev/RunCatNeo",
    };

    private static readonly Dictionary<string, string> German = new()
    {
        ["dashboard"] = "Dashboard",
        ["settings"] = "Einstellungen",
        ["openTaskManager"] = "Task-Manager öffnen",
        ["about"] = "Über RunCat Neo",
        ["reportIssue"] = "Problem melden",
        ["quit"] = "RunCat Neo beenden",
        ["cpu"] = "CPU",
        ["memory"] = "Speicher",
        ["storage"] = "Festplatte",
        ["network"] = "Netzwerk",
        ["battery"] = "Batterie",
        ["charging"] = "Lädt",
        ["used"] = "Belegt",
        ["free"] = "Frei",
        ["upload"] = "Upload",
        ["download"] = "Download",
        ["general"] = "Allgemein",
        ["runner"] = "Runner",
        ["launchAtLogin"] = "Bei Anmeldung starten",
        ["speedDecreasesUnderLoad"] = "Geschwindigkeit sinkt unter Last",
        ["flipHorizontally"] = "Runner horizontal spiegeln",
        ["updateInterval"] = "Aktualisierungsintervall",
        ["seconds"] = "Sekunden",
        ["monitorsMemory"] = "Speicher überwachen",
        ["monitorsStorage"] = "Festplatte überwachen",
        ["monitorsBattery"] = "Batterie überwachen",
        ["monitorsNetwork"] = "Netzwerk überwachen",
        ["customMetrics"] = "Custom Metrics",
        ["addJsonSource"] = "JSON-Quelle hinzufügen…",
        ["removeSource"] = "Entfernen",
        ["errorDetected"] = "⚠ Fehler erkannt",
        ["lastUpdated"] = "Zuletzt aktualisiert",
        ["failed"] = "Fehlgeschlagen",
        ["justNow"] = "gerade eben",
        ["minutesAgo"] = "vor {0} Min.",
        ["hoursAgo"] = "vor {0} Std.",
        ["daysAgo"] = "vor {0} Tagen",
        ["aboutBody"] = "RunCat Neo für Windows\nEine niedliche laufende Katze in deiner Taskleiste.\n\nDiese App ist Open-Source-Software.\nhttps://github.com/runcat-dev/RunCatNeo",
    };

    public static string Get(string key)
    {
        var table = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "de" ? German : English;
        return table.TryGetValue(key, out var value) ? value : English.GetValueOrDefault(key, key);
    }
}
