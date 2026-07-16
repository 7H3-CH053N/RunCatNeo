/*
 AppSettings.cs
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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace RunCatNeo.Core;

public sealed class AppSettings
{
    public string RunnerId { get; set; } = Runner.Default.Id;
    public bool SpeedDecreasesUnderLoad { get; set; }
    public bool IsFlippedHorizontally { get; set; }
    public int UpdateIntervalSeconds { get; set; } = UpdateIntervalExtensions.Default.Seconds();
    public SystemMetricsConfiguration SystemMetricsConfiguration { get; set; } = SystemMetricsConfiguration.Default;

    [JsonIgnore]
    public UpdateInterval UpdateInterval
    {
        get => UpdateIntervalExtensions.FromSeconds(UpdateIntervalSeconds);
        set => UpdateIntervalSeconds = value.Seconds();
    }

    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public string ToJson() => JsonSerializer.Serialize(this, jsonOptions);

    public static AppSettings FromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<AppSettings>(json, jsonOptions) ?? new AppSettings();
        }
        catch (JsonException)
        {
            return new AppSettings();
        }
    }
}
