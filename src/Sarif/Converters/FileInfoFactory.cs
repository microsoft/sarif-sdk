// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    internal class FileInfoFactory
    {
        private readonly Dictionary<string, IList<FileReference>> _fileInfoDictionary;
        private readonly Func<string, string> _mimeTypeDeterminer;

        internal FileInfoFactory(Func<string, string> mimeTypeDeterminer)
        {
            _mimeTypeDeterminer = mimeTypeDeterminer;
            _fileInfoDictionary = new Dictionary<string, IList<FileReference>>();
        }

        internal Dictionary<string, IList<FileReference>> Create(IEnumerable<Result> results)
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

                if (result.ExecutionFlows != null)
                {
                    foreach (IList<AnnotatedCodeLocation> codeFlow in result.ExecutionFlows)
                    {
                        foreach (AnnotatedCodeLocation codeLocation in codeFlow)
                        {
                            AddFileReference(codeLocation.PhysicalLocation);
                        }
                    }
                }

            }

            return _fileInfoDictionary;
        }

        private void AddFileReference(PhysicalLocation physicalLocation)
        {
            string key = physicalLocation.Uri.OriginalString;

            if (!_fileInfoDictionary.ContainsKey(key))
            {
                _fileInfoDictionary.Add(
                    key,
                    new List<FileReference>
                    {
                        new FileReference
                        {
                            Uri = physicalLocation.Uri,
                            MimeType = _mimeTypeDeterminer(key)
                        }
                    });
            }
        }
    }
}
