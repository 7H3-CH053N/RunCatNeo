/*
 RunnerSpeedTests.cs
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

public sealed class RunnerSpeedTests
{
    [Theory]
    [InlineData(0.0, 1.0)]
    [InlineData(5.0, 1.0)]
    [InlineData(25.0, 5.0)]
    [InlineData(50.0, 10.0)]
    [InlineData(100.0, 20.0)]
    [InlineData(150.0, 20.0)]
    public void Calculate_ScalesCpuToSpeedRange(double cpu, double expected)
    {
        Assert.Equal(expected, RunnerSpeed.Calculate(cpu, speedDecreasesUnderLoad: false), precision: 5);
    }

    [Theory]
    [InlineData(0.0, 10.0)]
    [InlineData(50.0, 5.5)]
    [InlineData(100.0, 0.5)]
    public void Calculate_InvertsWhenSpeedDecreasesUnderLoad(double cpu, double expected)
    {
        Assert.Equal(expected, RunnerSpeed.Calculate(cpu, speedDecreasesUnderLoad: true), precision: 5);
    }

    [Fact]
    public void FrameInterval_HalvesBaseDurationBySpeed()
    {
        Assert.Equal(500.0, RunnerSpeed.FrameInterval(1.0).TotalMilliseconds, precision: 5);
        Assert.Equal(25.0, RunnerSpeed.FrameInterval(20.0).TotalMilliseconds, precision: 5);
    }
}
