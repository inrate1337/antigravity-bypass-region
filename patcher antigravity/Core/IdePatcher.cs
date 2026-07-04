using System;
using System.IO;
namespace patcher_antigravity.Core
{
    public static class IdePatcher
    {
        public static (string content, bool applied) PatchIdeName(string content)
        {
            string oldStr = "ideName:\"antigravity\"";
            string newStr = "ideName:\"antigravity-insiders\"";
            if (content.Contains(oldStr))
            {
                return (content.Replace(oldStr, newStr), true);
            }
            return (content, false);
        }
        public static (string content, bool applied) PatchIneligibleScreen(string content)
        {
            string oldStr = "...s?{}:{errorType:\"ineligible\",reason:a,verificationUrl:i}";
            string newStr = "...s?{}:{}";
            if (content.Contains(oldStr))
            {
                return (content.Replace(oldStr, newStr), true);
            }
            return (content, false);
        }
        public static (string content, bool applied) PatchIsGoogleInternal(string content)
        {
            var regex = new System.Text.RegularExpressions.Regex(@"if\(this\.([a-zA-Z_$]+)\.isGoogleInternal\)");
            bool applied = false;
            string newContent = regex.Replace(content, match =>
            {
                applied = true;
                return "if(true)";
            });
            return (newContent, applied);
        }
        public static (string content, bool applied) PatchIsGoogleInternalComma(string content, Version agVersion = null)
        {
            var authRegex = Constants.ReAuthIsGoogleInternal;
            if (agVersion != null && agVersion < Constants.AuthPatchSwitchVersion)
            {
                authRegex = Constants.ReAuthIsGoogleInternalOld;
            }
            else if (agVersion != null)
            {
                authRegex = Constants.ReAuthIsGoogleInternalNew;
            }
            bool applied = false;
            string newContent = authRegex.Replace(content, match =>
            {
                applied = true;
                var prefix = match.Groups["prefix"].Value;
                return $"if({prefix}true)";
            });
            return (newContent, applied);
        }
        public static (string content, int totalApplied) ApplyPatchesMinimal(string content, Version agVersion = null)
        {
            int appliedCount = 0;
            bool resultApplied;
            (content, resultApplied) = PatchIsGoogleInternal(content);
            if (resultApplied) appliedCount++;
            (content, resultApplied) = PatchIsGoogleInternalComma(content, agVersion);
            if (resultApplied) appliedCount++;
            (content, resultApplied) = PatchIdeName(content);
            if (resultApplied) appliedCount++;
            (content, resultApplied) = PatchIneligibleScreen(content);
            if (resultApplied) appliedCount++;
            return (content, appliedCount);
        }
        public static bool IsAlreadyPatched(string content)
        {
            bool hasUnpatchedSimple = new System.Text.RegularExpressions.Regex(@"if\(this\.[a-zA-Z_$]+\.isGoogleInternal\)").IsMatch(content);
            bool hasUnpatchedAuth = Constants.ReAuthIsGoogleInternal.IsMatch(content);
            bool hasIde = content.Contains("ideName:\"antigravity-insiders\"");
            return !hasUnpatchedSimple && !hasUnpatchedAuth && hasIde;
        }
    }
}
