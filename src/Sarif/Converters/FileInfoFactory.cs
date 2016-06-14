﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    internal class FileInfoFactory
    {
        private readonly Dictionary<string, FileData> _fileInfoDictionary;
        private readonly Func<string, string> _mimeTypeClassifier;

        internal FileInfoFactory(Func<string, string> mimeTypeClassifier)
        {
            _mimeTypeClassifier = mimeTypeClassifier ?? MimeType.DetermineFromFileExtension;
            _fileInfoDictionary = new Dictionary<string, FileData>();
        }

        internal Dictionary<string, FileData> Create(IEnumerable<Result> results)
        {
            foreach (Result result in results)
            {
                if (result.Locations != null)
                {
                    foreach (Location location in result.Locations)
                    {
                        if (location.AnalysisTarget != null)
                        {
                            AddFile(location.AnalysisTarget);
                        }

                        if (location.ResultFile != null)
                        {
                            AddFile(location.ResultFile);
                        }
                    }
                }

                if (result.Stacks != null)
                {
                    foreach (IList<AnnotatedCodeLocation> stack in result.Stacks)
                    {
                        foreach (AnnotatedCodeLocation stackFrame in stack)
                        {
                            AddFile(stackFrame.PhysicalLocation);
                        }
                    }

                }

                if (result.CodeFlows != null)
                {
                    foreach (CodeFlow codeFlow in result.CodeFlows)
                    {
                        foreach (AnnotatedCodeLocation codeLocation in codeFlow.Locations)
                        {
                            AddFile(codeLocation.PhysicalLocation);
                        }
                    }
                }

                if (result.RelatedLocations != null)
                {
                    foreach (AnnotatedCodeLocation relatedLocation in result.RelatedLocations)
                    {
                        AddFile(relatedLocation.PhysicalLocation);
                    }
                }
            }

            return _fileInfoDictionary;
        }

        private void AddFile(PhysicalLocation physicalLocation)
        {
            if (physicalLocation == null)
            {
                return;
            }

            Uri uri = physicalLocation.Uri;
            string key = uri.ToString();
            string filePath = key;

            if (uri.IsAbsoluteUri && uri.IsFile)
            {
                filePath = uri.LocalPath;
            }


            if (!_fileInfoDictionary.ContainsKey(key))
            {
                _fileInfoDictionary.Add(
                    key,
                    new FileData
                    {
                        MimeType = _mimeTypeClassifier(filePath)
                    });
            }
        }
    }
}
