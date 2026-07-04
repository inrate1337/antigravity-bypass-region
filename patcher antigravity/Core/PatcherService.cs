using System;
using System.IO;
using System.Threading.Tasks;
namespace patcher_antigravity.Core
{
    public class PatcherService
    {
        public event Action<string> OnProgress;
        public event Action<string> OnError;
        public event Action OnSuccess;
        public async Task RunPatchAsync()
        {
            await Task.Run(async () =>
            {
                try
                {
                    OnProgress?.Invoke("> Initializing core...");
                    await Task.Delay(600);
                    OnProgress?.Invoke("> Searching for Antigravity IDE in the system...");
                    await Task.Delay(400);
                    string ideRoot = Discovery.FindIdeInstallRoot();
                    if (string.IsNullOrEmpty(ideRoot))
                    {
                        OnError?.Invoke("Antigravity IDE not found in standard paths.");
                        return;
                    }
                    OnProgress?.Invoke("> Searching for mount point (main.js)...");
                    await Task.Delay(400);
                    string mainJs = Discovery.FindMainJs(ideRoot);
                    if (string.IsNullOrEmpty(mainJs))
                    {
                        OnError?.Invoke("File main.js not found.");
                        return;
                    }
                    OnProgress?.Invoke("> Injecting patch into main.js...");
                    await Task.Delay(500);
                    bool isAsar = mainJs.Contains("app.asar");
                    string asarPath = null;
                    string tempDir = null;
                    string targetJs = mainJs;
                    if (isAsar)
                    {
                        asarPath = mainJs.Substring(0, mainJs.IndexOf("app.asar") + 8);
                        tempDir = Path.Combine(Path.GetTempPath(), "ag_asar_" + Guid.NewGuid().ToString());
                        OnProgress?.Invoke("> Extracting app.asar archive...");
                        await Task.Delay(600);
                        if (!AsarArchive.Extract(asarPath, tempDir))
                        {
                            OnError?.Invoke("Error unpacking app.asar.");
                            return;
                        }
                        targetJs = Path.Combine(tempDir, mainJs.Substring(asarPath.Length + 1));
                    }
                    string content = File.ReadAllText(targetJs);
                    if (IdePatcher.IsAlreadyPatched(content))
                    {
                        OnProgress?.Invoke("> IDE is already patched. Checking CLI...");
                        await Task.Delay(600);
                    }
                    else
                    {
                        var (newContent, appliedCount) = IdePatcher.ApplyPatchesMinimal(content);
                        if (appliedCount == 0)
                        {
                            OnError?.Invoke("Failed to apply patches to main.js.");
                            if (isAsar) Directory.Delete(tempDir, true);
                            return;
                        }
                        File.WriteAllText(targetJs, newContent);
                        if (isAsar)
                        {
                            OnProgress?.Invoke("> Building and packing app.asar...");
                            await Task.Delay(600);
                            if (!AsarArchive.Pack(tempDir, asarPath))
                            {
                                OnError?.Invoke("> [ERROR] Failed to pack app.asar");
                                Directory.Delete(tempDir, true);
                                return;
                            }
                            Directory.Delete(tempDir, true);
                        }
                    }
                    OnProgress?.Invoke("> Searching for Antigravity CLI binary...");
                    await Task.Delay(400);
                    string agyBinary = Discovery.FindAgyBinary();
                    if (!string.IsNullOrEmpty(agyBinary))
                    {
                        OnProgress?.Invoke("> Applying gate patch to agy.exe...");
                        await Task.Delay(500);
                        var (success, err) = CliPatcher.PatchAgyBinary(agyBinary);
                        if (!success)
                        {
                            OnProgress?.Invoke($"> [WARNING] CLI patch: {err}");
                            await Task.Delay(800);
                        }
                        else
                        {
                            OnProgress?.Invoke("> CLI successfully patched.");
                            await Task.Delay(500);
                        }
                    }
                    else
                    {
                        OnProgress?.Invoke("> CLI module not found, skipping...");
                        await Task.Delay(500);
                    }
                    OnProgress?.Invoke("> Clearing cache and finishing...");
                    await Task.Delay(600);
                    OnSuccess?.Invoke();
                }
                catch (Exception ex)
                {
                    OnError?.Invoke("An error occurred: " + ex.Message);
                }
            });
        }
    }
}
