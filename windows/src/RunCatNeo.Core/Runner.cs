/*
 Runner.cs
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

public sealed class Runner : IEquatable<Runner>
{
    public string Id { get; }
    public string Name { get; }
    public bool IsTemplate { get; }
    public FrameOrder FrameOrder { get; }
    public RunnerKind? Kind { get; }

    public bool IsCustom => Kind is null;

    public Runner(string id, string name, bool isTemplate, FrameOrder frameOrder)
    {
        Id = id;
        Name = name;
        IsTemplate = isTemplate;
        FrameOrder = frameOrder;
        Kind = null;
    }

    public Runner(RunnerKind kind)
    {
        Id = kind.Id();
        Name = kind.Id();
        IsTemplate = true;
        FrameOrder = kind.FrameOrder();
        Kind = kind;
    }

    public IReadOnlyList<string> ResourceNames() =>
        FrameOrder.Order
            .Select(frameNumber => IsCustom ? $"frame-{frameNumber}" : $"{Id}-frame-{frameNumber}")
            .ToArray();

    public static Runner Default { get; } = new(RunnerKind.Cat);

    public bool Equals(Runner? other) => other is not null && Id == other.Id;

    public override bool Equals(object? obj) => Equals(obj as Runner);

    public override int GetHashCode() => Id.GetHashCode();
}
