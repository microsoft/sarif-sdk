// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    internal class FileInfoFactory
    {
        private readonly LoggingOptions _loggingOptions;
        private readonly Func<string, string> _mimeTypeClassifier;
        private readonly Dictionary<string, FileData> _fileInfoDictionary;

        internal FileInfoFactory(Func<string, string> mimeTypeClassifier, LoggingOptions loggingOptions)
        {
            _mimeTypeClassifier = mimeTypeClassifier ?? MimeType.DetermineFromFileExtension;
            _fileInfoDictionary = new Dictionary<string, FileData>();
            _loggingOptions = loggingOptions;
        }

        internal Dictionary<string, FileData> Create(IEnumerable<Result> results)
        {
            foreach (Result result in results)
            {
                if (result.AnalysisTarget != null)
                {
                    AddFile(new PhysicalLocation
                    {
                        FileLocation = new FileLocation
                        {
                            Uri = result.AnalysisTarget.Uri
                        }
                    });
                }

                if (result.Locations != null)
                {
                    foreach (Location location in result.Locations)
                    {
                        if (location.PhysicalLocation != null)
                        {
                            AddFile(location.PhysicalLocation);
                        }
                    }
                }

                if (result.Stacks != null)
                {
                    foreach (IList<CodeFlowLocation> stack in result.Stacks)
                    {
                        foreach (CodeFlowLocation stackFrame in stack)
                        {
                            AddFile(stackFrame.Location.PhysicalLocation);
                        }
                    }

                }

                if (result.CodeFlows != null)
                {
                    foreach (CodeFlow codeFlow in result.CodeFlows)
                    {
                        foreach (CodeFlowLocation codeLocation in codeFlow.Locations)
                        {
                            if (codeLocation.Location?.PhysicalLocation != null)
                            {
                                AddFile(codeLocation.Location.PhysicalLocation);
                            }
                        }
                    }
                }

                if (result.RelatedLocations != null)
                {
                    foreach (Location relatedLocation in result.RelatedLocations)
                    {
                        AddFile(relatedLocation.PhysicalLocation);
                    }
                }
            }

            return _fileInfoDictionary;
        }

        private void AddFile(PhysicalLocation physicalLocation)
        {
            if (physicalLocation?.FileLocation == null)
            {
                return;
            }

            Uri uri = physicalLocation.FileLocation.Uri;
            string key = UriHelper.MakeValidUri(uri.OriginalString);
            string filePath = key;

            if (uri.IsAbsoluteUri && uri.IsFile)
            {
                filePath = uri.LocalPath;
            }

            FileData fileData = FileData.Create(
                uri,
                _loggingOptions,
                _mimeTypeClassifier(filePath));


            if (!_fileInfoDictionary.ContainsKey(key))
            {
                _fileInfoDictionary.Add(
                    key,
                    fileData);
            }
        }
    }
}
