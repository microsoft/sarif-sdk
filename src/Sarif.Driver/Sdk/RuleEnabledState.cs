// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    [Flags]
    public enum RuleEnabledState
    {
        /// <summary>
        /// Unknown state. Rule will raise all notifications according
        /// to helpers invoked during execution state.
        /// </summary>
        Default = 0,

        /// <summary>
        /// User has disabled a rule. This should always result in a tool
        /// warning, in order to preserve knowledge that some portion of
        /// analysis was disabled.
        /// </summary>
        Disabled = 0x1,

        /// <summary>
        /// User has reduced all signal from a rule to warning. Warnings
        /// should always be persisted to reports but may not block
        /// engineering processes.
        /// </summary>
        Warning = 0x2,

        /// <summary>
        /// User has elevated all failures from a check to errors.
        /// </summary>
        Error = 0x4
    }
}
