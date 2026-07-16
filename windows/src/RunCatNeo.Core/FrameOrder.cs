/*
 FrameOrder.cs
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

public sealed class FrameOrder : IEquatable<FrameOrder>
{
    public IReadOnlyList<int> Order { get; }

    private FrameOrder(IReadOnlyList<int> order)
    {
        Order = order;
    }

    public int this[int index] => index >= 0 && index < Order.Count ? Order[index] : 0;

    public static FrameOrder Ascending(int count) => new(Enumerable.Range(0, count).ToArray());

    public static FrameOrder Swing { get; } = new([0, 1, 2, 3, 4, 3, 2, 1]);

    public static FrameOrder Pendulum { get; } = new([0, 1, 2, 1, 0, 3, 4, 3]);

    public static FrameOrder PartyHorn { get; } = new([0, 1, 2, 3, 4, 4, 3, 2, 1]);

    public static FrameOrder Custom(IEnumerable<int> order) => new(order.ToArray());

    public bool Equals(FrameOrder? other) => other is not null && Order.SequenceEqual(other.Order);

    public override bool Equals(object? obj) => Equals(obj as FrameOrder);

    public override int GetHashCode() => Order.Aggregate(17, (hash, value) => hash * 31 + value);
}
