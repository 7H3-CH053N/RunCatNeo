/*
 CustomMetrics.cs
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

public sealed record CustomMetric
{
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("formattedValue")]
    public string? FormattedValue { get; init; }

    [JsonPropertyName("normalizedValue")]
    public double? NormalizedValue { get; init; }
}

// The JSON snapshot a producer script writes, as documented in docs/CustomMetricsSchema.md.
public sealed record CustomMetricsSnapshot
{
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("symbol")]
    public string? Symbol { get; init; }

    [JsonPropertyName("metricsBarValue")]
    public string? MetricsBarValue { get; init; }

    [JsonPropertyName("metrics")]
    public IReadOnlyList<CustomMetric>? Metrics { get; init; }

    [JsonPropertyName("lastUpdatedDate")]
    public DateTimeOffset? LastUpdatedDate { get; init; }

    // Parses a snapshot, returning null when the JSON is invalid or a
    // required field (title, metrics, lastUpdatedDate) is missing —
    // mirroring the strict Codable decoding on macOS.
    public static CustomMetricsSnapshot? FromJson(string json)
    {
        CustomMetricsSnapshot? snapshot;
        try
        {
            snapshot = JsonSerializer.Deserialize<CustomMetricsSnapshot>(json);
        }
        catch (JsonException)
        {
            return null;
        }
        if (snapshot?.Title is null || snapshot.Metrics is null || snapshot.LastUpdatedDate is null)
        {
            return null;
        }
        if (snapshot.Metrics.Any(metric => metric.Title is null || metric.FormattedValue is null))
        {
            return null;
        }
        return snapshot;
    }
}

public sealed record CustomMetricsBundle(Guid Id, CustomMetricsSnapshot Snapshot, bool IsFailed);

public sealed record CustomMetricsSource
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string DisplayName { get; init; } = "";
    public string FilePath { get; init; } = "";
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record CustomMetricsConfiguration
{
    public IReadOnlyList<CustomMetricsSource> Sources { get; init; } = [];

    public static CustomMetricsConfiguration Empty { get; } = new();
}
