// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    internal class FileInfoFactory
    {
        private readonly Dictionary<Uri, IList<FileReference>> _fileInfoDictionary;
        private readonly Func<Uri, string> _mimeTypeClassifier;

        internal FileInfoFactory(Func<Uri, string> mimeTypeClassifier)
        {
            _mimeTypeClassifier = mimeTypeClassifier;
            _fileInfoDictionary = new Dictionary<Uri, IList<FileReference>>();
        }

        internal Dictionary<Uri, IList<FileReference>> Create(IEnumerable<Result> results)
        {
            foreach (Result result in results)
            {
                if (result.Locations != null)
                {
                    foreach (Location location in result.Locations)
                    {
                        if (location.AnalysisTarget != null)
                        {
                            AddFileReference(location.AnalysisTarget);
                        }

                        if (location.ResultFile != null)
                        {
                            AddFileReference(location.ResultFile);
                        }
                    }
                }

                if (result.Stacks != null)
                {
                    foreach (IList<AnnotatedCodeLocation> stack in result.Stacks)
                    {
                        foreach (AnnotatedCodeLocation stackFrame in stack)
                        {
                            AddFileReference(stackFrame.PhysicalLocation);
                        }
                    }

                }

                if (result.CodeFlows != null)
                {
                    foreach (IList<AnnotatedCodeLocation> codeFlow in result.CodeFlows)
                    {
                        foreach (AnnotatedCodeLocation codeLocation in codeFlow)
                        {
                            AddFileReference(codeLocation.PhysicalLocation);
                        }
                    }
                }

                if (result.RelatedLocations != null)
                {
                    foreach (AnnotatedCodeLocation relatedLocation in result.RelatedLocations)
                    {
                        AddFileReference(relatedLocation.PhysicalLocation);
                    }
                }
            }

            return _fileInfoDictionary;
        }

        private void AddFileReference(PhysicalLocation physicalLocation)
        {
            Uri key = physicalLocation.Uri;

            if (!_fileInfoDictionary.ContainsKey(key))
            {
                _fileInfoDictionary.Add(
                    key,
                    new List<FileReference>
                    {
                        new FileReference
                        {
                            Uri = physicalLocation.Uri,
                            MimeType = _mimeTypeClassifier(key)
                        }
                    });
            }
        }
    }
}
