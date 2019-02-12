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
        private readonly HashSet<FileData> _files;

        internal FileInfoFactory(Func<string, string> mimeTypeClassifier, OptionallyEmittedData dataToInsert)
        {
            _mimeTypeClassifier = mimeTypeClassifier ?? MimeType.DetermineFromFileExtension;
            _files = new HashSet<FileData>(FileData.ValueComparer);
            _dataToInsert = dataToInsert;
        }

        internal HashSet<FileData> Create(IEnumerable<Result> results)
        {
            foreach (Result result in results)
            {
                if (result.AnalysisTarget != null)
                {
                    AddFile(new PhysicalLocation
                    {
                        FileLocation = result.AnalysisTarget.DeepClone()
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
                    foreach (Stack stack in result.Stacks)
                    {
                        foreach (StackFrame stackFrame in stack.Frames)
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

            return _files;
        }

        private void AddFile(PhysicalLocation physicalLocation)
        {
            if (physicalLocation?.FileLocation == null)
            {
                return;
            }

            Uri uri = physicalLocation.FileLocation.Uri;
            string filePath = UriHelper.MakeValidUri(uri.OriginalString);

            if (uri.IsAbsoluteUri && uri.IsFile)
            {
                filePath = uri.LocalPath;
            }

            FileData fileData = FileData.Create(
                uri,
                _dataToInsert,
                _mimeTypeClassifier(filePath));

            _files.Add(fileData);
        }
    }
}
