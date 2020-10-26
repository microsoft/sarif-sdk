﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Notification
    {
#if DEBUG
        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();

            sb.Append(this.Locations?.FirstOrDefault()?.PhysicalLocation?.ArtifactLocation?.Uri);
            sb.Append(" : " + this.Descriptor.Id);
            sb.Append(" : " + this.AssociatedRule?.Id);
            sb.Append(" : " + this.Level);

            if (!string.IsNullOrEmpty(this.Message?.Text))
            {
                sb.Append(" : " + this.Message.Text);
            }
            else if (this.Message?.Arguments != null)
            {
                sb.Append(" : {");
                foreach (string argument in this.Message.Arguments)
                {
                    sb.Append(argument + ",");
                }
                sb.Length = sb.Length - 1;
                sb.Append("}");
            }
            return sb.ToString();
        }
#endif

    }
}
