using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
namespace patcher_antigravity.Core
{
    public static class AsarArchive
    {
        public static bool Extract(string asarPath, string destDir)
        {
            if (!File.Exists(asarPath)) return false;
            try
            {
                using (var fs = new FileStream(asarPath, FileMode.Open, FileAccess.Read))
                using (var br = new BinaryReader(fs))
                {
                    uint pickleHeader = br.ReadUInt32();
                    uint headerSize = br.ReadUInt32();
                    uint jsonPayloadSize = br.ReadUInt32();
                    uint jsonSize = br.ReadUInt32();
                    byte[] jsonBytes = br.ReadBytes((int)jsonSize);
                    string jsonString = Encoding.UTF8.GetString(jsonBytes);
                    var root = JsonNode.Parse(jsonString);
                    long payloadOffset = 16 + headerSize;
                    ExtractNode(root, "", destDir, fs, payloadOffset, asarPath);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        private static void ExtractNode(JsonNode node, string currentPath, string destDir, FileStream fs, long payloadOffset, string asarPath)
        {
            if (node["files"] != null)
            {
                var dirPath = Path.Combine(destDir, currentPath);
                Directory.CreateDirectory(dirPath);
                var files = node["files"].AsObject();
                foreach (var kvp in files)
                {
                    ExtractNode(kvp.Value, Path.Combine(currentPath, kvp.Key), destDir, fs, payloadOffset, asarPath);
                }
            }
            else
            {
                var filePath = Path.Combine(destDir, currentPath);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                if (node["unpacked"] != null && node["unpacked"].GetValue<bool>())
                {
                    string srcFile = FindUnpackedFile(asarPath, currentPath);
                    if (srcFile != null)
                    {
                        File.Copy(srcFile, filePath, true);
                    }
                }
                else
                {
                    long offset = long.Parse(node["offset"].GetValue<string>());
                    long size = node["size"].GetValue<long>();
                    fs.Seek(payloadOffset + offset, SeekOrigin.Begin);
                    using (var outFs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        byte[] buffer = new byte[8192];
                        long bytesToRead = size;
                        while (bytesToRead > 0)
                        {
                            int read = fs.Read(buffer, 0, (int)Math.Min(buffer.Length, bytesToRead));
                            if (read == 0) break;
                            outFs.Write(buffer, 0, read);
                            bytesToRead -= read;
                        }
                    }
                }
            }
        }
        private static string FindUnpackedFile(string asarPath, string currentPath)
        {
            string resourceDir = Path.GetDirectoryName(asarPath);
            string[] candidates = {
                asarPath + ".unpacked",
                Path.Combine(resourceDir, "app.asar.unpacked"),
                Path.Combine(resourceDir, "app1.asar.unpacked")
            };
            foreach (var cand in candidates)
            {
                string candFile = Path.Combine(cand, currentPath);
                if (File.Exists(candFile)) return candFile;
            }
            return null;
        }
        public static HashSet<string> GetUnpackedPaths(string asarPath)
        {
            var unpackedPaths = new HashSet<string>();
            if (!File.Exists(asarPath)) return unpackedPaths;
            try
            {
                using (var fs = new FileStream(asarPath, FileMode.Open, FileAccess.Read))
                using (var br = new BinaryReader(fs))
                {
                    uint pickleHeader = br.ReadUInt32();
                    uint headerSize = br.ReadUInt32();
                    uint jsonPayloadSize = br.ReadUInt32();
                    uint jsonSize = br.ReadUInt32();
                    byte[] jsonBytes = br.ReadBytes((int)jsonSize);
                    var root = JsonNode.Parse(Encoding.UTF8.GetString(jsonBytes));
                    void CollectUnpacked(JsonNode node, string currentPath)
                    {
                        if (node["files"] != null)
                        {
                            foreach (var kvp in node["files"].AsObject())
                            {
                                CollectUnpacked(kvp.Value, string.IsNullOrEmpty(currentPath) ? kvp.Key : currentPath + "/" + kvp.Key);
                            }
                        }
                        else
                        {
                            if (node["unpacked"] != null && node["unpacked"].GetValue<bool>())
                            {
                                unpackedPaths.Add(currentPath.Replace('\\', '/'));
                            }
                        }
                    }
                    CollectUnpacked(root, "");
                }
            }
            catch { }
            return unpackedPaths;
        }
        private static JsonObject ComputeIntegrity(string filePath)
        {
            var obj = new JsonObject();
            obj["algorithm"] = "SHA256";
            obj["blockSize"] = Constants.IntegrityBlockSize;
            using (var sha = SHA256.Create())
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                obj["hash"] = BitConverter.ToString(sha.ComputeHash(fileBytes)).Replace("-", "").ToLower();
                var blocks = new JsonArray();
                for (int i = 0; i < fileBytes.Length; i += Constants.IntegrityBlockSize)
                {
                    int len = Math.Min(Constants.IntegrityBlockSize, fileBytes.Length - i);
                    byte[] block = new byte[len];
                    Array.Copy(fileBytes, i, block, 0, len);
                    blocks.Add(BitConverter.ToString(sha.ComputeHash(block)).Replace("-", "").ToLower());
                }
                obj["blocks"] = blocks;
            }
            return obj;
        }
        public static bool Pack(string sourceDir, string asarPath)
        {
            try
            {
                var unpackedPaths = GetUnpackedPaths(asarPath);
                string unpackedDir = asarPath + ".unpacked";
                if (Directory.Exists(unpackedDir))
                {
                    try { Directory.Delete(unpackedDir, true); } catch { }
                }
                Directory.CreateDirectory(unpackedDir);
                string tempPayloadPath = Path.GetTempFileName();
                var header = new JsonObject();
                header["files"] = new JsonObject();
                long currentOffset = 0;
                var allFiles = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories)
                                        .Select(f => new { FullPath = f, RelPath = Path.GetRelativePath(sourceDir, f).Replace('\\', '/') })
                                        .Where(f => !f.RelPath.EndsWith("downloaded_frontend_main.js") && !f.RelPath.EndsWith("frontend_patch_result.json") && !f.RelPath.EndsWith(".bak"))
                                        .OrderBy(f => f.RelPath)
                                        .ToList();
                using (var payloadFs = new FileStream(tempPayloadPath, FileMode.Create, FileAccess.Write))
                {
                    foreach (var file in allFiles)
                    {
                        long size = new FileInfo(file.FullPath).Length;
                        bool isUnpacked = unpackedPaths.Contains(file.RelPath);
                        var entry = new JsonObject();
                        entry["size"] = size;
                        entry["integrity"] = ComputeIntegrity(file.FullPath);
                        if (isUnpacked)
                        {
                            entry["unpacked"] = true;
                            string destUnpacked = Path.Combine(unpackedDir, file.RelPath.Replace('/', Path.DirectorySeparatorChar));
                            Directory.CreateDirectory(Path.GetDirectoryName(destUnpacked));
                            File.Copy(file.FullPath, destUnpacked, true);
                        }
                        else
                        {
                            byte[] data = File.ReadAllBytes(file.FullPath);
                            payloadFs.Write(data, 0, data.Length);
                            entry["offset"] = currentOffset.ToString();
                            currentOffset += size;
                        }
                        var parts = file.RelPath.Split('/');
                        var currentNode = header;
                        for (int i = 0; i < parts.Length - 1; i++)
                        {
                            if (currentNode["files"] == null) currentNode["files"] = new JsonObject();
                            var filesNode = currentNode["files"].AsObject();
                            if (!filesNode.ContainsKey(parts[i])) filesNode[parts[i]] = new JsonObject { ["files"] = new JsonObject() };
                            currentNode = filesNode[parts[i]].AsObject();
                        }
                        if (currentNode["files"] == null) currentNode["files"] = new JsonObject();
                        currentNode["files"].AsObject()[parts.Last()] = entry;
                    }
                }
                string jsonString = JsonSerializer.Serialize(header, new JsonSerializerOptions { WriteIndented = false });
                byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
                uint jsonSize = (uint)jsonBytes.Length;
                uint jsonPayloadSize = (uint)AlignTo(jsonSize + 4, 4);
                uint jsonPaddingSize = jsonPayloadSize - (jsonSize + 4);
                uint headerSize = jsonPayloadSize + 4;
                uint pickleHeader = 4;
                string tempAsarPath = Path.GetTempFileName();
                using (var outFs = new FileStream(tempAsarPath, FileMode.Create, FileAccess.Write))
                using (var bw = new BinaryWriter(outFs))
                {
                    bw.Write(pickleHeader);
                    bw.Write(headerSize);
                    bw.Write(jsonPayloadSize);
                    bw.Write(jsonSize);
                    bw.Write(jsonBytes);
                    if (jsonPaddingSize > 0)
                    {
                        bw.Write(new byte[jsonPaddingSize]);
                    }
                    using (var payFs = new FileStream(tempPayloadPath, FileMode.Open, FileAccess.Read))
                    {
                        payFs.CopyTo(outFs);
                    }
                }
                if (File.Exists(asarPath))
                {
                    File.Delete(asarPath);
                }
                File.Move(tempAsarPath, asarPath);
                File.Delete(tempPayloadPath);
                if (Directory.Exists(unpackedDir) && !Directory.EnumerateFileSystemEntries(unpackedDir).Any())
                {
                    Directory.Delete(unpackedDir);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        private static long AlignTo(long value, long alignment)
        {
            long remainder = value % alignment;
            return remainder == 0 ? value : value + (alignment - remainder);
        }
    }
}
