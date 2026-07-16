/*
 SettingsStore.cs
 RunCatNeo.Core

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

namespace RunCatNeo.Core;

public sealed class SettingsStore
{
    private readonly string filePath;

    public AppSettings Settings { get; private set; }

    public SettingsStore(string filePath)
    {
        this.filePath = filePath;
        Settings = Load();
    }

    private AppSettings Load()
    {
        if (!File.Exists(filePath))
        {
            return new AppSettings();
        }
        return AppSettings.FromJson(File.ReadAllText(filePath));
    }

    public void Save()
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllText(filePath, Settings.ToJson());
    }
}
