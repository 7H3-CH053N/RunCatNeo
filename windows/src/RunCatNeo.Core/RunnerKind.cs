/*
 RunnerKind.cs
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

public enum RunnerKind
{
    Cat,
    Dog,
    Slime,
    Drop,
    Coffee,
    NewtonCradle,
    Engine,
    Mochi,
}

public static class RunnerKindExtensions
{
    public static readonly RunnerKind[] AllCases = Enum.GetValues<RunnerKind>();

    public static string Id(this RunnerKind kind) => kind switch
    {
        RunnerKind.Cat => "cat",
        RunnerKind.Dog => "dog",
        RunnerKind.Slime => "slime",
        RunnerKind.Drop => "drop",
        RunnerKind.Coffee => "coffee",
        RunnerKind.NewtonCradle => "newton-cradle",
        RunnerKind.Engine => "engine",
        RunnerKind.Mochi => "mochi",
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    public static RunnerKind? FromId(string id) =>
        AllCases.Where(kind => kind.Id() == id).Select(kind => (RunnerKind?)kind).FirstOrDefault();

    public static int NumberOfResources(this RunnerKind kind) => kind switch
    {
        RunnerKind.Coffee or RunnerKind.Engine => 10,
        _ => 5,
    };

    public static FrameOrder FrameOrder(this RunnerKind kind) => kind switch
    {
        RunnerKind.Slime => Core.FrameOrder.PartyHorn,
        RunnerKind.NewtonCradle => Core.FrameOrder.Pendulum,
        RunnerKind.Mochi => Core.FrameOrder.Swing,
        _ => Core.FrameOrder.Ascending(kind.NumberOfResources()),
    };
}
