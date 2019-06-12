using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;

namespace Microsoft.CodeAnalysis.Sarif.Baseline
{
    internal static class WhatComparer
    {
        private const string PropertySetBase = "Base";

        public static IEnumerable<WhatComponent> WhatProperties(this ExtractedResult result)
        {
            if (result?.Result == null) { yield break; }

            // Add Guid
            if (result.Result.Guid != null)
            {
                yield return new WhatComponent(result.RuleId, PropertySetBase, "Guid", result.Result.Guid);
            }

            // Add Message Text
            string messageText = result.Result.GetMessageText(result.Result.GetRule(result.OriginalRun));
            if (!string.IsNullOrEmpty(messageText))
            {
                yield return new WhatComponent(result.RuleId, PropertySetBase, "Message", messageText);
            }

            // Add each Fingerprint
            if (result.Result.Fingerprints != null)
            {
                foreach (var fingerprint in result.Result.Fingerprints)
                {
                    yield return new WhatComponent(result.RuleId, "Fingerprint", fingerprint.Key, fingerprint.Value);
                }
            }

            // Add each PartialFingerprint
            if (result.Result.PartialFingerprints != null)
            {
                foreach (var fingerprint in result.Result.PartialFingerprints)
                {
                    yield return new WhatComponent(result.RuleId, "PartialFingerprint", fingerprint.Key, fingerprint.Value);
                }
            }

            if (result.Result.Locations != null)
            {
                foreach (Location location in result.Result.Locations)
                {
                    string snippet = location?.PhysicalLocation?.Region?.Snippet?.Text;
                    if (!string.IsNullOrEmpty(snippet))
                    {
                        yield return new WhatComponent(result.RuleId, PropertySetBase, "Location.Snippet", snippet);
                    }
                }
            }

            // Add each Property
            if (result.Result.Properties != null)
            {
                foreach (var property in result.Result.Properties)
                {
                    yield return new WhatComponent(result.RuleId, "Property", property.Key, property.Value.SerializedValue);
                }
            }
        }

        /// <summary>
        ///  Match the 'What' properties of two ExtractedResults.
        /// </summary>
        /// <param name="other">ExtractedResult to match</param>
        /// <returns>True if *any* 'What' property matches, False otherwise</returns>
        public static bool MatchesWhat(this ExtractedResult me, ExtractedResult other)
        {
            if (me?.Result == null || other?.Result == null) { return false; }

            // Message Text and Snippets only match results when unique,
            // so they are never considered for matching in 'MatchesWhat'

            // Consider letting a MessageText non-match invalidate a match.
            //  This would be after the high confidence matching (Fingerprint, Guid) and before the lower confidence matching.
            // Should Properties match threshold be over 50%? 100%?

            // Match Guid
            if (me.Result.Guid != null)
            {
                if (string.Equals(me.Result.Guid, other.Result.Guid))
                {
                    return true;
                }
            }

            // Match Fingerprints (any one match is a match)
            if (me.Result.Fingerprints != null && other.Result.Fingerprints != null)
            {
                foreach (var fingerprint in me.Result.Fingerprints)
                {
                    if(other.Result.Fingerprints.TryGetValue(fingerprint.Key, out string otherFingerprint)
                        && string.Equals(fingerprint.Value, otherFingerprint))
                    {
                        return true;
                    }
                }
            }

            // Match PartialFingerprints (50% must match)
            if (me.Result.PartialFingerprints != null && other.Result.PartialFingerprints != null)
            {
                int matchCount = 0;

                foreach (var fingerprint in me.Result.PartialFingerprints)
                {
                    if (other.Result.PartialFingerprints.TryGetValue(fingerprint.Key, out string otherFingerprint)
                        && string.Equals(fingerprint.Value, otherFingerprint))
                    {
                        matchCount++;
                    }
                }

                if(matchCount > 0 && matchCount >= me.Result.PartialFingerprints.Count / 2)
                {
                    return true;
                }
            }

            // Match Properties (50% must match)
            if (me.Result.Properties != null && other.Result.Properties != null)
            {
                int matchCount = 0;

                foreach (var property in me.Result.Properties)
                {
                    if(other.Result.TryGetProperty(property.Key, out string otherPropertyValue)
                        && string.Equals(property.Value, otherPropertyValue))
                    {
                        matchCount++;
                    }
                }

                if (matchCount > 0 && matchCount >= me.Result.Properties.Count / 2)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
