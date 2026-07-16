/*
 AppSettingsTests.cs
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

public sealed class AppSettingsTests
{
    [Fact]
    public void Defaults_MatchMacOsDefaults()
    {
        var settings = new AppSettings();
        Assert.Equal("cat", settings.RunnerId);
        Assert.False(settings.SpeedDecreasesUnderLoad);
        Assert.False(settings.IsFlippedHorizontally);
        Assert.Equal(UpdateInterval.FiveSeconds, settings.UpdateInterval);
        Assert.Equal(SystemMetricsConfiguration.Default, settings.SystemMetricsConfiguration);
    }

    [Fact]
    public void Json_RoundTripsAllProperties()
    {
        var settings = new AppSettings
        {
            RunnerId = "mochi",
            SpeedDecreasesUnderLoad = true,
            IsFlippedHorizontally = true,
            UpdateInterval = UpdateInterval.TenSeconds,
            SystemMetricsConfiguration = new SystemMetricsConfiguration { MonitorsBattery = false },
        };
        var restored = AppSettings.FromJson(settings.ToJson());
        Assert.Equal("mochi", restored.RunnerId);
        Assert.True(restored.SpeedDecreasesUnderLoad);
        Assert.True(restored.IsFlippedHorizontally);
        Assert.Equal(UpdateInterval.TenSeconds, restored.UpdateInterval);
        Assert.False(restored.SystemMetricsConfiguration.MonitorsBattery);
        Assert.True(restored.SystemMetricsConfiguration.MonitorsMemory);
    }

    [Fact]
    public void FromJson_FallsBackToDefaultsOnInvalidJson()
    {
        var settings = AppSettings.FromJson("not json at all");
        Assert.Equal("cat", settings.RunnerId);
    }

    [Fact]
    public void UnknownUpdateInterval_FallsBackToDefault()
    {
        var settings = new AppSettings { UpdateIntervalSeconds = 42 };
        Assert.Equal(UpdateInterval.FiveSeconds, settings.UpdateInterval);
    }
}
