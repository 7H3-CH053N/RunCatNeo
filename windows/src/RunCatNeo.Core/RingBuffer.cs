/*
 RingBuffer.cs
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

public sealed class RingBuffer
{
    private readonly int length;
    private readonly List<double> values;

    public IReadOnlyList<double> Values => values;

    public RingBuffer(int length = 61)
    {
        this.length = length;
        values = Enumerable.Repeat(2.0, length).ToList();
    }

    public void Append(double value)
    {
        values.Add(value);
        values.RemoveAt(0);
    }

    public void Append(IEnumerable<double> newValues)
    {
        values.AddRange(newValues);
        if (values.Count > length)
        {
            values.RemoveRange(0, values.Count - length);
        }
    }
}
