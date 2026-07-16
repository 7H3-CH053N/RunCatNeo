/*
 RingBufferTests.cs
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

public sealed class RingBufferTests
{
    [Fact]
    public void InitialValues_AreFilledWithTwo()
    {
        var buffer = new RingBuffer(length: 5);
        Assert.Equal([2.0, 2.0, 2.0, 2.0, 2.0], buffer.Values);
    }

    [Fact]
    public void Append_KeepsLengthConstant()
    {
        var buffer = new RingBuffer(length: 3);
        buffer.Append(50.0);
        Assert.Equal([2.0, 2.0, 50.0], buffer.Values);
        buffer.Append(75.0);
        Assert.Equal([2.0, 50.0, 75.0], buffer.Values);
    }

    [Fact]
    public void AppendMany_KeepsOnlyTheNewestValues()
    {
        var buffer = new RingBuffer(length: 3);
        buffer.Append([1.0, 2.0, 3.0, 4.0]);
        Assert.Equal([2.0, 3.0, 4.0], buffer.Values);
    }
}
