/*
 RelativeTime.cs
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

namespace RunCatNeo.Windows;

public static class RelativeTime
{
    public static string Format(DateTimeOffset date)
    {
        var elapsed = DateTimeOffset.Now - date;
        if (elapsed < TimeSpan.FromMinutes(1))
        {
            return Strings.Get("justNow");
        }
        if (elapsed < TimeSpan.FromHours(1))
        {
            return string.Format(Strings.Get("minutesAgo"), (int)elapsed.TotalMinutes);
        }
        if (elapsed < TimeSpan.FromDays(1))
        {
            return string.Format(Strings.Get("hoursAgo"), (int)elapsed.TotalHours);
        }
        return string.Format(Strings.Get("daysAgo"), (int)elapsed.TotalDays);
    }
}
