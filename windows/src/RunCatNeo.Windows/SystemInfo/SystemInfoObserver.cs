/*
 SystemInfoObserver.cs
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

using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace RunCatNeo.Windows.SystemInfo;

// Windows counterpart of SystemInfoKit's observer: samples CPU, memory,
// storage, network, and battery on demand.
public sealed class SystemInfoObserver
{
    private long previousIdleTime;
    private long previousKernelTime;
    private long previousUserTime;
    private long previousNetworkSentBytes;
    private long previousNetworkReceivedBytes;
    private DateTime previousNetworkSampleTime = DateTime.UtcNow;

    public bool MonitorsMemory { get; set; } = true;
    public bool MonitorsStorage { get; set; } = true;
    public bool MonitorsBattery { get; set; } = true;
    public bool MonitorsNetwork { get; set; } = true;

    public SystemInfoObserver()
    {
        _ = SampleCpuPercentage();
        _ = SampleNetworkInfo();
    }

    public SystemInfoBundle Sample() => new()
    {
        CpuPercentage = SampleCpuPercentage(),
        MemoryInfo = MonitorsMemory ? SampleMemoryInfo() : null,
        StorageInfo = MonitorsStorage ? SampleStorageInfo() : null,
        NetworkInfo = MonitorsNetwork ? SampleNetworkInfo() : null,
        BatteryInfo = MonitorsBattery ? SampleBatteryInfo() : null,
    };

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetSystemTimes(out long idleTime, out long kernelTime, out long userTime);

    private double SampleCpuPercentage()
    {
        if (!GetSystemTimes(out var idleTime, out var kernelTime, out var userTime))
        {
            return 0.0;
        }
        var idleDelta = idleTime - previousIdleTime;
        var kernelDelta = kernelTime - previousKernelTime;
        var userDelta = userTime - previousUserTime;
        previousIdleTime = idleTime;
        previousKernelTime = kernelTime;
        previousUserTime = userTime;
        // Kernel time includes idle time, so busy time is (kernel - idle) + user.
        var totalDelta = kernelDelta + userDelta;
        if (totalDelta <= 0)
        {
            return 0.0;
        }
        var busyDelta = totalDelta - idleDelta;
        return Math.Clamp(100.0 * busyDelta / totalDelta, 0.0, 100.0);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatusEx
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhys;
        public ulong AvailPhys;
        public ulong TotalPageFile;
        public ulong AvailPageFile;
        public ulong TotalVirtual;
        public ulong AvailVirtual;
        public ulong AvailExtendedVirtual;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx buffer);

    private static MemoryInfo? SampleMemoryInfo()
    {
        var status = new MemoryStatusEx { Length = (uint)Marshal.SizeOf<MemoryStatusEx>() };
        if (!GlobalMemoryStatusEx(ref status))
        {
            return null;
        }
        var usedBytes = status.TotalPhys - status.AvailPhys;
        var percentage = status.TotalPhys > 0 ? 100.0 * usedBytes / status.TotalPhys : 0.0;
        return new MemoryInfo(percentage, status.TotalPhys, usedBytes);
    }

    private static StorageInfo? SampleStorageInfo()
    {
        var systemRoot = Path.GetPathRoot(Environment.SystemDirectory);
        if (systemRoot is null)
        {
            return null;
        }
        try
        {
            var drive = new DriveInfo(systemRoot);
            var total = (ulong)drive.TotalSize;
            var available = (ulong)drive.AvailableFreeSpace;
            var percentage = total > 0 ? 100.0 * (total - available) / total : 0.0;
            return new StorageInfo(percentage, total, available);
        }
        catch (IOException)
        {
            return null;
        }
    }

    private NetworkInfo? SampleNetworkInfo()
    {
        long sentBytes = 0;
        long receivedBytes = 0;
        try
        {
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                    networkInterface.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }
                var statistics = networkInterface.GetIPStatistics();
                sentBytes += statistics.BytesSent;
                receivedBytes += statistics.BytesReceived;
            }
        }
        catch (NetworkInformationException)
        {
            return null;
        }
        var now = DateTime.UtcNow;
        var elapsedSeconds = (now - previousNetworkSampleTime).TotalSeconds;
        var sentDelta = sentBytes - previousNetworkSentBytes;
        var receivedDelta = receivedBytes - previousNetworkReceivedBytes;
        previousNetworkSentBytes = sentBytes;
        previousNetworkReceivedBytes = receivedBytes;
        previousNetworkSampleTime = now;
        if (elapsedSeconds <= 0)
        {
            return new NetworkInfo(0.0, 0.0);
        }
        return new NetworkInfo(
            Math.Max(0.0, sentDelta / elapsedSeconds),
            Math.Max(0.0, receivedDelta / elapsedSeconds)
        );
    }

    private static BatteryInfo? SampleBatteryInfo()
    {
        var powerStatus = SystemInformation.PowerStatus;
        if (powerStatus.BatteryChargeStatus.HasFlag(BatteryChargeStatus.NoSystemBattery))
        {
            return null;
        }
        return new BatteryInfo(
            Math.Clamp(100.0 * powerStatus.BatteryLifePercent, 0.0, 100.0),
            powerStatus.PowerLineStatus == PowerLineStatus.Online
        );
    }
}
