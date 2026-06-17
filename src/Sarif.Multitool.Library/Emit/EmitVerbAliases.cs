// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Centralized mapping of deprecated emit-verb names to their canonical replacements.
    /// </summary>
    /// <remarks>
    /// <para>The emit chain is six verbs that each emit one or more events to the staged
    /// <c>.wip.jsonl</c> log (or, for <c>emit-finalize</c>, the final SARIF artifact). The
    /// canonical names share the <c>emit-</c> prefix so the family is discoverable as a unit and
    /// the verb name reads as the event kind it emits.</para>
    /// <para>Earlier releases (through v5.0.x) used an <c>add-</c> prefix on the four append verbs.
    /// Those names remain accepted with a one-line deprecation warning on stderr; they will be
    /// removed in v6. The two long descriptor verb names also drop the redundant <c>-reporting-</c>
    /// segment.</para>
    /// <para><see cref="Program.Main"/> applies <see cref="Normalize"/> on <c>args[0]</c> before
    /// argument parsing so the deprecated names route to the canonical options classes without a
    /// per-class alias attribute. <see cref="GetSchemaCommand"/> applies the same normalization to
    /// its verb argument so <c>get-schema add-results</c> still resolves.</para>
    /// </remarks>
    public static class EmitVerbAliases
    {
        /// <summary>Deprecated verb name -> canonical verb name.</summary>
        public static readonly IReadOnlyDictionary<string, string> DeprecatedToCanonical =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["add-results"] = "emit-results",
                ["add-invocations"] = "emit-invocations",
                ["add-rule-reporting-descriptors"] = "emit-rule-descriptors",
                ["add-notification-reporting-descriptors"] = "emit-notification-descriptors",
            };

        /// <summary>
        /// If <paramref name="verb"/> is a deprecated emit-verb name, returns its canonical
        /// replacement and (when <paramref name="warn"/>) writes a one-line deprecation message to
        /// stderr; otherwise returns <paramref name="verb"/> unchanged.
        /// </summary>
        public static string Normalize(string verb, bool warn = true)
        {
            if (verb != null && DeprecatedToCanonical.TryGetValue(verb, out string canonical))
            {
                if (warn)
                {
                    Console.Error.WriteLine(
                        $"warning: '{verb}' is deprecated and will be removed in v6; use '{canonical}'.");
                }

                return canonical;
            }

            return verb;
        }
    }
}
