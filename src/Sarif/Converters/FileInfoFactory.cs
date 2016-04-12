// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    internal class FileInfoFactory
    {
        private readonly Dictionary<Uri, IList<FileData>> _fileInfoDictionary;
        private readonly Func<Uri, string> _mimeTypeClassifier;

        internal FileInfoFactory(Func<Uri, string> mimeTypeClassifier)
        {
            _mimeTypeClassifier = mimeTypeClassifier;
            _fileInfoDictionary = new Dictionary<Uri, IList<FileData>>();
        }

        internal Dictionary<Uri, IList<FileData>> Create(IEnumerable<Result> results)
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
                    foreach (IList<AnnotatedCodeLocation> codeFlow in result.CodeFlows)
                    {
                        foreach (AnnotatedCodeLocation codeLocation in codeFlow)
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
            Uri key = physicalLocation.Uri;

            if (!_fileInfoDictionary.ContainsKey(key))
            {
                _fileInfoDictionary.Add(
                    key,
                    new List<FileData>
                    {
                        new FileData
                        {
                            MimeType = _mimeTypeClassifier(key)
                        }
                    });
            }
        }
    }
}
