/*
 RunnerTests.cs
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

public sealed class RunnerTests
{
    [Fact]
    public void BuiltInRunner_ResourceNamesFollowFrameOrder()
    {
        var runner = new Runner(RunnerKind.Cat);
        Assert.Equal(
            ["cat-frame-0", "cat-frame-1", "cat-frame-2", "cat-frame-3", "cat-frame-4"],
            runner.ResourceNames()
        );
    }

    [Fact]
    public void MochiRunner_ResourceNamesFollowSwingOrder()
    {
        var runner = new Runner(RunnerKind.Mochi);
        Assert.Equal(
            [
                "mochi-frame-0", "mochi-frame-1", "mochi-frame-2", "mochi-frame-3",
                "mochi-frame-4", "mochi-frame-3", "mochi-frame-2", "mochi-frame-1",
            ],
            runner.ResourceNames()
        );
    }

    [Fact]
    public void CustomRunner_ResourceNamesUsePlainFramePrefix()
    {
        var runner = new Runner("some-id", "My Runner", isTemplate: false, FrameOrder.Custom([0, 2]));
        Assert.True(runner.IsCustom);
        Assert.Equal(["frame-0", "frame-2"], runner.ResourceNames());
    }

    [Fact]
    public void AllRunnerKinds_HaveUniqueIds()
    {
        var ids = RunnerKindExtensions.AllCases.Select(kind => kind.Id()).ToArray();
        Assert.Equal(ids.Length, ids.Distinct().Count());
    }

    [Fact]
    public void FromId_RoundTripsAllKinds()
    {
        foreach (var kind in RunnerKindExtensions.AllCases)
        {
            Assert.Equal(kind, RunnerKindExtensions.FromId(kind.Id()));
        }
        Assert.Null(RunnerKindExtensions.FromId("does-not-exist"));
    }
}
