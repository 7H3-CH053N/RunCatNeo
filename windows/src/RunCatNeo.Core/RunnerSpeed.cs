/*
 RunnerSpeed.cs
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

public static class RunnerSpeed
{
    // Base animation rate: two frames per second at speed 1, scaled by the speed factor.
    public const double BaseFrameDuration = 0.5;

    public static double Calculate(double cpuPercentage, bool speedDecreasesUnderLoad)
    {
        var cpuValue = Math.Max(1.0, Math.Min(20.0, cpuPercentage / 5.0));
        return speedDecreasesUnderLoad ? 0.5 * (21.0 - cpuValue) : cpuValue;
    }

    public static TimeSpan FrameInterval(double speed) =>
        TimeSpan.FromSeconds(BaseFrameDuration / Math.Max(0.1, speed));
}
