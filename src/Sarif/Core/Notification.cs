// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System;
#endif

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Notification
    {
        public bool ShouldSerializeLocations() { return this.Locations.HasAtLeastOneNonDefaultValue(Location.ValueComparer); }

#if DEBUG
        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();

            if (this.TimeUtc != default) { sb.Append($"{this.TimeUtc}"); }

            Uri uri = this.Locations?[0].PhysicalLocation?.ArtifactLocation?.Uri;
            if (uri != null) { sb.Append(uri); }

            if (this.Descriptor != null) { sb.Append(" : ").Append(this.Descriptor?.Id); }
            if (this.AssociatedRule != null) { sb.Append(" : ").Append(this.AssociatedRule?.Id); }

            sb.Append($" : {this.Level}");

            if (!string.IsNullOrEmpty(this.Message?.Text))
            {
                sb.Append($" : {this.Message.Text}");
            }
            else if (this.Message?.Arguments != null)
            {
                sb.Append(" : {");
                foreach (string argument in this.Message.Arguments)
                {
                    sb.Append(argument).Append(',');
                }
                sb.Length = sb.Length - 1;
                sb.Append('}');
            }

            if (this.Exception != null)
            {
                sb.Append($" : {this.Exception.ToString().Replace(Environment.NewLine, string.Empty)}");
            }

            return sb.ToString();
        }
#endif

    }
}
