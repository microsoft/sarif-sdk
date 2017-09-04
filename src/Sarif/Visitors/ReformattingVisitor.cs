using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class ReformattingVisitor : SarifRewritingVisitor
    {
        LoggingOptions _loggingOptions;

        public ReformattingVisitor(LoggingOptions loggingOptions)
        {
            _loggingOptions = loggingOptions;
        }

        public override Run VisitRun(Run node)
        {
            if (node != null)
            {
                if (node.Files != null)
                {
                    var keys = node.Files.Keys.ToArray();
                    foreach (var key in keys)
                    {
                        var value = node.Files[key];
                        if (value != null)
                        {
                            node.Files[key] = VisitDictionaryValueNullChecked(key, value);
                        }
                    }
                }
            }

            return node;
        }

        public FileData VisitDictionaryValueNullChecked(string key, FileData node)
        {
            bool workToDo = false;

            workToDo |= node.Hashes == null && _loggingOptions.Includes(LoggingOptions.ComputeFileHashes);
            workToDo |= node.Contents == null && _loggingOptions.Includes(LoggingOptions.PersistFileContents);

            if (workToDo)
            {
                node = FileData.Create(new Uri(key), _loggingOptions, node.MimeType);
            }

            return base.VisitFileData(node);
        }
    }
}
