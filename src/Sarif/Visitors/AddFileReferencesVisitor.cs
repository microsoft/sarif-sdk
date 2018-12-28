// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.using System;
using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class AddFileReferencesVisitor : SarifRewritingVisitor
    {
        private Run _currentRun;
        private IDictionary<FileLocation, int> _fileToIndexMap;

        public override Run VisitRun(Run run)
        {
            _fileToIndexMap = new Dictionary<FileLocation, int>();

            run.Files = run.Files ?? new List<FileData>();

            // First, we'll initialize our file object to index map
            // with any files that already exist in the table
            for (int i = 0; i < run.Files.Count; i++)
            {
                FileData fileData = run.Files[i];

                var fileLocation = new FileLocation
                {
                    Uri = fileData.FileLocation.Uri,
                    UriBaseId = fileData.FileLocation.UriBaseId
                };

                _fileToIndexMap[fileLocation] = i;

                // For good measure, we'll explicitly populate the file index property
                run.Files[i].FileLocation.FileIndex = i;
            }

            _currentRun = run;

            // Next, visit all run file locations. This will add any
            // previously unknown file objects to the files table.
            base.VisitRun(run);

            return _currentRun;
        }

        public override FileLocation VisitFileLocation(FileLocation node)
        {
            node.FileIndex = _currentRun.GetFileIndex(node, addToFilesTableIfNotPresent: true);            

            return base.VisitFileLocation(node);



            
        }
    }
}
