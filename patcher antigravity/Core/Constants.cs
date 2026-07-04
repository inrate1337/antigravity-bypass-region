using System;
using System.Text.RegularExpressions;
namespace patcher_antigravity.Core
{
    public static class Constants
    {
        public static readonly Version MinAgVersion = new Version(2, 1, 0);
        public static readonly Version AuthPatchSwitchVersion = new Version(1, 23);
        public static readonly Version RuntimeSettingsSwitchVersion = new Version(1, 23);
        public const string CloudCodeEndpoint = "https://cloudcode-pa.googleapis.com";
        public const string RuntimeExperimentsValue = "CASCADE_DEFAULT_MODEL_OVERRIDE,CASCADE_USE_EXPERIMENT_CHECKPOINTER,CASCADE_NEW_MODELS_NUX,CASCADE_NEW_WAVE_2_MODELS_NUX";
        public const string AgRegistrySubkey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{AA73B3E3-C6C8-45C8-B1DC-4AE56C751432}_is1";
        public static readonly Regex ReAuthIsGoogleInternal = new Regex(
            @"if\(\s*(?<prefix>(?:this\.[A-Za-z_$][\w$]*\.send\(\{type:[^}]+\}\)\s*,\s*)?" +
            @"this\.[A-Za-z_$][\w$]*\.resetIsTierGCPTos\(\)\s*,\s*)" +
            @"this\.[A-Za-z_$][\w$]*\.isGoogleInternal\s*\)", RegexOptions.Compiled);
        public static readonly Regex ReAuthIsGoogleInternalOld = new Regex(
            @"if\(\s*(?<prefix>this\.[A-Za-z_$][\w$]*\.resetIsTierGCPTos\(\)\s*,\s*)" +
            @"this\.[A-Za-z_$][\w$]*\.isGoogleInternal\s*\)", RegexOptions.Compiled);
        public static readonly Regex ReAuthIsGoogleInternalNew = new Regex(
            @"if\(\s*(?<prefix>this\.[A-Za-z_$][\w$]*\.send\(\{type:[^}]+\}\)\s*,\s*" +
            @"this\.[A-Za-z_$][\w$]*\.resetIsTierGCPTos\(\)\s*,\s*)" +
            @"this\.[A-Za-z_$][\w$]*\.isGoogleInternal\s*\)", RegexOptions.Compiled);
        public static readonly byte?[] CliGateSigPattern = new byte?[] { 
            0x48, 0x85, 0xc0, 0x0f, 0x84, null, null, null, null, 0x80, 0x78, 0x08, 0x00, 0x0f, 0x85, null, null, null, null 
        };
        public static readonly byte?[] CliGatePatchedPattern = new byte?[] { 
            0x48, 0x85, 0xc0, 0x0f, 0x84, null, null, null, null, 0x48, 0x85, 0xc0, 0x90, 0x0f, 0x85, null, null, null, null 
        };
        public static readonly byte[] CliGateFix = new byte[] { 0x48, 0x85, 0xc0, 0x90 };
        public const int CliGateOffset = 9;
        public const string BakExt = ".agybak";
        public const int IntegrityBlockSize = 4 * 1024 * 1024;
    }
}
