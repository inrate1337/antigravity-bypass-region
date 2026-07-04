using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
namespace patcher_antigravity.Core
{
    public static class Discovery
    {
        public static string FindIdeInstallRoot()
        {
            var candidates = new List<string>();
            var localAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");
            if (!string.IsNullOrEmpty(localAppData))
            {
                candidates.Add(Path.Combine(localAppData, "Programs", "Antigravity IDE"));
            }
            var programFiles = Environment.GetEnvironmentVariable("PROGRAMFILES");
            if (!string.IsNullOrEmpty(programFiles))
            {
                candidates.Add(Path.Combine(programFiles, "Antigravity IDE"));
            }
            var programFilesX86 = Environment.GetEnvironmentVariable("PROGRAMFILES(X86)");
            if (!string.IsNullOrEmpty(programFilesX86))
            {
                candidates.Add(Path.Combine(programFilesX86, "Antigravity IDE"));
            }
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(Constants.AgRegistrySubkey))
                {
                    if (key != null)
                    {
                        var installLoc = key.GetValue("InstallLocation") as string;
                        if (!string.IsNullOrWhiteSpace(installLoc))
                        {
                            candidates.Add(installLoc.Trim());
                        }
                    }
                }
            }
            catch { }
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(Constants.AgRegistrySubkey))
                {
                    if (key != null)
                    {
                        var installLoc = key.GetValue("InstallLocation") as string;
                        if (!string.IsNullOrWhiteSpace(installLoc))
                        {
                            candidates.Add(installLoc.Trim());
                        }
                    }
                }
            }
            catch { }
            foreach (var path in candidates)
            {
                var mainJsOptions = new[]
                {
                    Path.Combine(path, "resources", "app", "out", "main.js"),
                    Path.Combine(path, "resources", "app", "main.js"),
                    Path.Combine(path, "out", "main.js"),
                    Path.Combine(path, "main.js")
                };
                foreach (var opt in mainJsOptions)
                {
                    if (File.Exists(opt))
                    {
                        return path;
                    }
                }
            }
            return null;
        }
        public static string FindMainJs(string root)
        {
            if (string.IsNullOrEmpty(root)) return null;
            var mainJsOptions = new[]
            {
                Path.Combine(root, "resources", "app", "out", "main.js"),
                Path.Combine(root, "resources", "app", "main.js"),
                Path.Combine(root, "out", "main.js"),
                Path.Combine(root, "main.js")
            };
            foreach (var opt in mainJsOptions)
            {
                if (File.Exists(opt))
                {
                    return opt;
                }
            }
            return null;
        }
        public static string FindAgyBinary()
        {
            var candidates = new List<string>();
            var envVars = new[] { "LOCALAPPDATA", "PROGRAMFILES", "PROGRAMFILES(X86)", "ProgramData", "APPDATA" };
            foreach (var envVar in envVars)
            {
                var val = Environment.GetEnvironmentVariable(envVar);
                if (!string.IsNullOrEmpty(val))
                {
                    candidates.Add(val);
                    var programs = Path.Combine(val, "Programs");
                    if (Directory.Exists(programs)) candidates.Add(programs);
                }
            }
            var userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
            if (!string.IsNullOrEmpty(userProfile))
            {
                candidates.Add(Path.Combine(userProfile, "scoop", "apps"));
            }
            var scoop = Environment.GetEnvironmentVariable("SCOOP");
            if (!string.IsNullOrEmpty(scoop))
            {
                candidates.Add(Path.Combine(scoop, "apps"));
            }
            var hitCandidates = new List<string>();
            foreach (var root in candidates)
            {
                if (!Directory.Exists(root)) continue;
                var p1 = Path.Combine(root, "agy", "bin", "agy.exe");
                if (File.Exists(p1)) hitCandidates.Add(p1);
                try
                {
                    var agyDirs = Directory.GetDirectories(root, "agy*");
                    foreach (var dir in agyDirs)
                    {
                        var exe1 = Path.Combine(dir, "agy.exe");
                        if (File.Exists(exe1)) hitCandidates.Add(exe1);
                        var binExe = Path.Combine(dir, "bin", "agy.exe");
                        if (File.Exists(binExe)) hitCandidates.Add(binExe);
                    }
                }
                catch { }
            }
            string best = null;
            DateTime bestTime = DateTime.MinValue;
            foreach (var hit in hitCandidates)
            {
                var fi = new FileInfo(hit);
                if (fi.LastWriteTime > bestTime)
                {
                    bestTime = fi.LastWriteTime;
                    best = hit;
                }
            }
            return best;
        }
    }
}
