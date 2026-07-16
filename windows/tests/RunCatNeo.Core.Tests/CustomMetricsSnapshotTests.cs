/*
 CustomMetricsSnapshotTests.cs
 RunCatNeo.Core.Tests

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

using RunCatNeo.Core;
using Xunit;

public sealed class CustomMetricsSnapshotTests
{
    // The example from docs/CustomMetricsSchema.md.
    private const string SchemaExample = """
        {
          "title": "Claude Code",
          "symbol": "staroflife",
          "metricsBarValue": "5.4%",
          "metrics": [
            { "title": "Model",   "formattedValue": "Opus 4.7" },
            { "title": "Context", "formattedValue": "5.4%",  "normalizedValue": 0.054 },
            { "title": "5h",      "formattedValue": "16.4%", "normalizedValue": 0.164 },
            { "title": "7d",      "formattedValue": "1.0%",  "normalizedValue": 0.01  }
          ],
          "lastUpdatedDate": "2026-06-05T04:50:40Z"
        }
        """;

    [Fact]
    public void FromJson_ParsesTheDocumentedSchemaExample()
    {
        var snapshot = CustomMetricsSnapshot.FromJson(SchemaExample);
        Assert.NotNull(snapshot);
        Assert.Equal("Claude Code", snapshot.Title);
        Assert.Equal("staroflife", snapshot.Symbol);
        Assert.Equal("5.4%", snapshot.MetricsBarValue);
        Assert.Equal(4, snapshot.Metrics!.Count);
        Assert.Equal("Model", snapshot.Metrics[0].Title);
        Assert.Equal("Opus 4.7", snapshot.Metrics[0].FormattedValue);
        Assert.Null(snapshot.Metrics[0].NormalizedValue);
        Assert.Equal(0.054, snapshot.Metrics[1].NormalizedValue!.Value, precision: 6);
        Assert.Equal(
            new DateTimeOffset(2026, 6, 5, 4, 50, 40, TimeSpan.Zero),
            snapshot.LastUpdatedDate!.Value
        );
    }

    [Fact]
    public void FromJson_AllowsEmptyMetricsArray()
    {
        var snapshot = CustomMetricsSnapshot.FromJson(
            """{"title": "T", "metrics": [], "lastUpdatedDate": "2026-06-05T04:50:40Z"}"""
        );
        Assert.NotNull(snapshot);
        Assert.Empty(snapshot.Metrics!);
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("{}")]
    [InlineData("""{"title": "T", "metrics": []}""")]
    [InlineData("""{"title": "T", "lastUpdatedDate": "2026-06-05T04:50:40Z"}""")]
    [InlineData("""{"metrics": [], "lastUpdatedDate": "2026-06-05T04:50:40Z"}""")]
    [InlineData("""{"title": "T", "metrics": [{"title": "x"}], "lastUpdatedDate": "2026-06-05T04:50:40Z"}""")]
    public void FromJson_ReturnsNullForInvalidOrIncompleteJson(string json)
    {
        Assert.Null(CustomMetricsSnapshot.FromJson(json));
    }

    [Fact]
    public void CustomMetricsConfiguration_RoundTripsThroughAppSettings()
    {
        var source = new CustomMetricsSource
        {
            DisplayName = "runcat-usage",
            FilePath = @"C:\Users\me\.claude\runcat-usage.json",
            CreatedAt = new DateTimeOffset(2026, 7, 16, 5, 0, 0, TimeSpan.Zero),
        };
        var settings = new AppSettings
        {
            CustomMetricsConfiguration = new CustomMetricsConfiguration { Sources = [source] },
        };
        var restored = AppSettings.FromJson(settings.ToJson());
        var restoredSource = Assert.Single(restored.CustomMetricsConfiguration.Sources);
        Assert.Equal(source.Id, restoredSource.Id);
        Assert.Equal(source.DisplayName, restoredSource.DisplayName);
        Assert.Equal(source.FilePath, restoredSource.FilePath);
    }
}
