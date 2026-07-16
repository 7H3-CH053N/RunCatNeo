/*
 UpdateInterval.cs
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

public enum UpdateInterval
{
    ThreeSeconds = 3,
    FiveSeconds = 5,
    TenSeconds = 10,
}

public static class UpdateIntervalExtensions
{
    public static readonly UpdateInterval[] AllCases = [
        UpdateInterval.ThreeSeconds,
        UpdateInterval.FiveSeconds,
        UpdateInterval.TenSeconds,
    ];

    public const UpdateInterval Default = UpdateInterval.FiveSeconds;

    public static int Seconds(this UpdateInterval interval) => (int)interval;

    public static UpdateInterval FromSeconds(int seconds) =>
        AllCases.Contains((UpdateInterval)seconds) ? (UpdateInterval)seconds : Default;
}
