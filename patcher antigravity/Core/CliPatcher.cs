using System;
using System.IO;
namespace patcher_antigravity.Core
{
    public static class CliPatcher
    {
        public static (bool success, string error) PatchAgyBinary(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return (false, "Target is not a valid file.");
            }
            try
            {
                byte[] data = File.ReadAllBytes(path);
                var status = CheckStatus(data);
                if (status == "patched")
                {
                    return (true, "Already patched.");
                }
                else if (status == "unknown")
                {
                    return (false, "Gate signature not found (unsupported version) or not unique.");
                }
                int offset = FindSignatureOffset(data, Constants.CliGateSigPattern);
                if (offset == -1) return (false, "Could not find offset during patch.");
                string bakPath = path + Constants.BakExt;
                if (!File.Exists(bakPath))
                {
                    File.Copy(path, bakPath);
                }
                int patchIndex = offset + Constants.CliGateOffset;
                for (int i = 0; i < Constants.CliGateFix.Length; i++)
                {
                    data[patchIndex + i] = Constants.CliGateFix[i];
                }
                File.WriteAllBytes(path, data);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public static string CheckStatus(byte[] data)
        {
            if (FindSignatureOffset(data, Constants.CliGatePatchedPattern) != -1)
            {
                return "patched";
            }
            int occurrences = CountSignatureOccurrences(data, Constants.CliGateSigPattern);
            if (occurrences == 1)
            {
                return "unpatched";
            }
            return "unknown"; 
        }
        private static int FindSignatureOffset(byte[] data, byte?[] pattern)
        {
            for (int i = 0; i <= data.Length - pattern.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (pattern[j].HasValue && data[i + j] != pattern[j].Value)
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    return i;
                }
            }
            return -1;
        }
        private static int CountSignatureOccurrences(byte[] data, byte?[] pattern)
        {
            int count = 0;
            for (int i = 0; i <= data.Length - pattern.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (pattern[j].HasValue && data[i + j] != pattern[j].Value)
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
