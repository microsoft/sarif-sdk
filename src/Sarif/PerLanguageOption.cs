﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// An option that can be specified once per language.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PerLanguageOption<T> : IOption
    {
        /// <summary>
        /// A description of this specific option.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Feature this option is associated with.
        /// </summary>
        public string Feature { get; }

        /// <summary>
        /// The name of the option.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type of the option value.
        /// </summary>
        public Type OptionType
        {
            get { return typeof(T); }
        }

        /// <summary>
        /// The default option value.
        /// </summary>
        public Func<T> DefaultValue { get; }

        public PerLanguageOption(string feature, string name, Func<T> defaultValue) :
            this(feature, name, defaultValue, description: null)
        {
        }

        public PerLanguageOption(string feature, string name, Func<T> defaultValue, string description)
        {
            if (string.IsNullOrWhiteSpace(feature))
            {
                throw new ArgumentNullException(nameof(feature));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
            Feature = feature;
            Description = description;
            DefaultValue = defaultValue;
        }

        Type IOption.Type
        {
            get { return typeof(T); }
        }

        object IOption.DefaultValue
        {
            get { return this.DefaultValue(); }
        }

        bool IOption.IsPerLanguage
        {
            get { return true; }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} - {1}", this.Feature, this.Name);
        }
    }
}
