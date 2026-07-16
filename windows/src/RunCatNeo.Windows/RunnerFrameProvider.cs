/*
 RunnerFrameProvider.cs
 RunCatNeo.Windows

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

namespace RunCatNeo.Windows;

public sealed class RunnerFrameProvider
{
    private readonly string presetDirectory;
    private readonly CustomRunnerRepository customRunnerRepository;

    public RunnerFrameProvider(string presetDirectory, CustomRunnerRepository customRunnerRepository)
    {
        this.presetDirectory = presetDirectory;
        this.customRunnerRepository = customRunnerRepository;
    }

    // Returns the animation frames of a runner in frame order.
    // Frames are loaded per resource once and reused for repeated frame numbers.
    public IReadOnlyList<Bitmap> LoadFrames(Runner runner)
    {
        var cache = new Dictionary<string, Bitmap>();
        var frames = new List<Bitmap>();
        foreach (var resourceName in runner.ResourceNames())
        {
            if (!cache.TryGetValue(resourceName, out var bitmap))
            {
                var path = runner.IsCustom
                    ? customRunnerRepository.FramePath(runner, resourceName)
                    : Path.Combine(presetDirectory, runner.Id, $"{resourceName}.png");
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"Missing runner frame: {path}", path);
                }
                // Copy into a new bitmap so the file handle is released immediately.
                using var loaded = new Bitmap(path);
                bitmap = new Bitmap(loaded);
                cache[resourceName] = bitmap;
            }
            frames.Add(bitmap);
        }
        return frames;
    }

    public IReadOnlyList<Runner> LoadAllRunners()
    {
        var runners = RunnerKindExtensions.AllCases.Select(kind => new Runner(kind)).ToList();
        runners.AddRange(customRunnerRepository.LoadCustomRunners());
        return runners;
    }
}
