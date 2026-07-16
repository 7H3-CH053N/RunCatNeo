/*
 CustomRunnerRepository.cs
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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace RunCatNeo.Core;

// Persists custom runners with the same JSON shape the macOS app uses
// ({id, name, isTemplate, frameOrder: [Int]}), so runner definitions are interchangeable.
public sealed class CustomRunnerRepository
{
    private sealed record CustomRunnerDto
    {
        [JsonPropertyName("id")]
        public string Id { get; init; } = "";

        [JsonPropertyName("name")]
        public string Name { get; init; } = "";

        [JsonPropertyName("isTemplate")]
        public bool IsTemplate { get; init; }

        [JsonPropertyName("frameOrder")]
        public int[] FrameOrder { get; init; } = [];
    }

    private readonly string rootDirectory;

    private string RunnersFilePath => Path.Combine(rootDirectory, "custom-runners.json");

    public CustomRunnerRepository(string rootDirectory)
    {
        this.rootDirectory = rootDirectory;
    }

    public IReadOnlyList<Runner> LoadCustomRunners()
    {
        if (!File.Exists(RunnersFilePath))
        {
            return [];
        }
        try
        {
            var dtos = JsonSerializer.Deserialize<List<CustomRunnerDto>>(File.ReadAllText(RunnersFilePath)) ?? [];
            return dtos
                .Select(dto => new Runner(dto.Id, dto.Name, dto.IsTemplate, FrameOrder.Custom(dto.FrameOrder)))
                .ToArray();
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public void SaveCustomRunners(IEnumerable<Runner> runners)
    {
        Directory.CreateDirectory(rootDirectory);
        var dtos = runners.Select(runner => new CustomRunnerDto
        {
            Id = runner.Id,
            Name = runner.Name,
            IsTemplate = runner.IsTemplate,
            FrameOrder = runner.FrameOrder.Order.ToArray(),
        });
        File.WriteAllText(
            RunnersFilePath,
            JsonSerializer.Serialize(dtos, new JsonSerializerOptions { WriteIndented = true })
        );
    }

    public string FramePath(Runner runner, string resourceName) =>
        Path.Combine(rootDirectory, runner.Id, $"{resourceName}.png");
}
