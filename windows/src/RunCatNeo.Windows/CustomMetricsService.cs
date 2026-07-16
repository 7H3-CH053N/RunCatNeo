/*
 CustomMetricsService.cs
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

// Watches the JSON files of the configured custom metrics sources and keeps
// one CustomMetricsBundle per source, mirroring the macOS CustomMetricsService:
// the card updates on file-system events, a failed read keeps the previous
// snapshot flagged as failed, and unreachable files are retried every 5 seconds.
public sealed class CustomMetricsService : IDisposable
{
    // Invisible control whose handle marshals FileSystemWatcher events onto the UI thread.
    private sealed class MarshalControl : Control
    {
        public MarshalControl()
        {
            _ = Handle;
        }
    }

    private readonly MarshalControl marshalControl = new();
    private readonly System.Windows.Forms.Timer retryTimer = new() { Interval = 5000 };
    private readonly Dictionary<Guid, FileSystemWatcher> watchers = [];
    private readonly Dictionary<Guid, CustomMetricsBundle> bundles = [];
    private IReadOnlyList<CustomMetricsSource> sources = [];

    public event Action? BundlesChanged;

    public IReadOnlyList<CustomMetricsBundle> Bundles =>
        sources
            .Select(source => bundles.GetValueOrDefault(source.Id))
            .Where(bundle => bundle is not null)
            .Select(bundle => bundle!)
            .ToArray();

    public CustomMetricsService()
    {
        retryTimer.Tick += (_, _) => RetryUnhealthySources();
        retryTimer.Start();
    }

    public void Configure(CustomMetricsConfiguration configuration)
    {
        sources = configuration.Sources;
        var sourceIds = sources.Select(source => source.Id).ToHashSet();
        foreach (var staleId in watchers.Keys.Where(id => !sourceIds.Contains(id)).ToArray())
        {
            watchers[staleId].Dispose();
            watchers.Remove(staleId);
        }
        foreach (var staleId in bundles.Keys.Where(id => !sourceIds.Contains(id)).ToArray())
        {
            bundles.Remove(staleId);
        }
        foreach (var source in sources)
        {
            EnsureWatcher(source);
            Reload(source);
        }
        BundlesChanged?.Invoke();
    }

    private void EnsureWatcher(CustomMetricsSource source)
    {
        if (watchers.TryGetValue(source.Id, out var existing))
        {
            if (existing.EnableRaisingEvents)
            {
                return;
            }
            existing.Dispose();
            watchers.Remove(source.Id);
        }
        var directory = Path.GetDirectoryName(source.FilePath);
        var fileName = Path.GetFileName(source.FilePath);
        if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName) || !Directory.Exists(directory))
        {
            return;
        }
        var watcher = new FileSystemWatcher(directory, fileName)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
            SynchronizingObject = marshalControl,
        };
        void OnChanged(object _, FileSystemEventArgs __) => ReloadAndNotify(source);
        watcher.Changed += OnChanged;
        watcher.Created += OnChanged;
        watcher.Deleted += OnChanged;
        watcher.Renamed += (_, _) => ReloadAndNotify(source);
        watcher.EnableRaisingEvents = true;
        watchers[source.Id] = watcher;
    }

    private void ReloadAndNotify(CustomMetricsSource source)
    {
        Reload(source);
        BundlesChanged?.Invoke();
    }

    private void Reload(CustomMetricsSource source)
    {
        CustomMetricsSnapshot? snapshot = null;
        try
        {
            if (File.Exists(source.FilePath))
            {
                snapshot = CustomMetricsSnapshot.FromJson(File.ReadAllText(source.FilePath));
            }
        }
        catch (IOException)
        {
            // A concurrent atomic replace can race the read; treat it as a failed read.
        }
        catch (UnauthorizedAccessException)
        {
        }
        if (snapshot is not null)
        {
            bundles[source.Id] = new CustomMetricsBundle(source.Id, snapshot, IsFailed: false);
        }
        else if (bundles.TryGetValue(source.Id, out var previous))
        {
            bundles[source.Id] = previous with { IsFailed = true };
        }
    }

    private void RetryUnhealthySources()
    {
        var changed = false;
        foreach (var source in sources)
        {
            var bundle = bundles.GetValueOrDefault(source.Id);
            if (bundle is { IsFailed: false } && watchers.ContainsKey(source.Id))
            {
                continue;
            }
            EnsureWatcher(source);
            Reload(source);
            changed |= bundles.GetValueOrDefault(source.Id) != bundle;
        }
        if (changed)
        {
            BundlesChanged?.Invoke();
        }
    }

    public bool HasError(Guid sourceId) =>
        bundles.GetValueOrDefault(sourceId) is null or { IsFailed: true };

    public void Dispose()
    {
        retryTimer.Dispose();
        foreach (var watcher in watchers.Values)
        {
            watcher.Dispose();
        }
        watchers.Clear();
        marshalControl.Dispose();
    }
}
