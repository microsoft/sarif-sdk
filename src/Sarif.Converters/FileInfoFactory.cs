// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    internal class FileInfoFactory
    {
        private readonly OptionallyEmittedData _dataToInsert;
        private readonly Func<string, string> _mimeTypeClassifier;
        private readonly Dictionary<string, FileData> _fileInfoDictionary;

        internal FileInfoFactory(Func<string, string> mimeTypeClassifier, OptionallyEmittedData dataToInsert)
        {
            _mimeTypeClassifier = mimeTypeClassifier ?? MimeType.DetermineFromFileExtension;
            _fileInfoDictionary = new Dictionary<string, FileData>();
            _dataToInsert = dataToInsert;
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
                    foreach (IList<ThreadFlowLocation> stack in result.Stacks)
                    {
                        foreach (ThreadFlowLocation stackFrame in stack)
                        {
                            AddFile(stackFrame.Location.PhysicalLocation);
                        }
                    }

                }

                if (result.CodeFlows != null)
                {
                    foreach (CodeFlow codeFlow in result.CodeFlows)
                    {
                        foreach (ThreadFlow threadFlow in codeFlow.ThreadFlows)
                        {
                            foreach (ThreadFlowLocation codeLocation in threadFlow.Locations)
                            {
                                if (codeLocation.Location?.PhysicalLocation != null)
                                {
                                    AddFile(codeLocation.Location.PhysicalLocation);
                                }
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

            Uri uri = SarifUtilities.CreateUri(physicalLocation.FileLocation.Uri);
            string key = UriHelper.MakeValidUri(uri.OriginalString);
            string filePath = key;

            if (uri.IsAbsoluteUri && uri.IsFile)
            {
                filePath = uri.LocalPath;
            }

            FileData fileData = FileData.Create(
                uri,
                _dataToInsert,
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
