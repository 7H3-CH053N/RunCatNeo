/*
 FrameOrderTests.cs
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

public sealed class FrameOrderTests
{
    [Fact]
    public void Ascending_ProducesSequentialOrder()
    {
        Assert.Equal([0, 1, 2, 3, 4], FrameOrder.Ascending(5).Order);
    }

    [Fact]
    public void Swing_MatchesMacOsDefinition()
    {
        Assert.Equal([0, 1, 2, 3, 4, 3, 2, 1], FrameOrder.Swing.Order);
    }

    [Fact]
    public void Pendulum_MatchesMacOsDefinition()
    {
        Assert.Equal([0, 1, 2, 1, 0, 3, 4, 3], FrameOrder.Pendulum.Order);
    }

    [Fact]
    public void PartyHorn_MatchesMacOsDefinition()
    {
        Assert.Equal([0, 1, 2, 3, 4, 4, 3, 2, 1], FrameOrder.PartyHorn.Order);
    }

    [Fact]
    public void Indexer_ReturnsZeroForOutOfRangeIndex()
    {
        var order = FrameOrder.Ascending(5);
        Assert.Equal(3, order[3]);
        Assert.Equal(0, order[99]);
        Assert.Equal(0, order[-1]);
    }
}
