// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;

namespace Microsoft.Sarif.Viewer.ViewModels
{
    /// <summary>
    /// This type is only used by the VS designer. It provides the data which is 
    /// displayed in the designer. 
    /// </summary>
    public static class ViewModelLocator
    {
        static object _syncroot = new object();
        static SarifErrorListItem _designTime = null;

        // This is the view model displayed by the Visual Studio designer.
        public static SarifErrorListItem DesignTime
        {
            get
            {
                if (_designTime == null)
                {
                    lock (_syncroot)
                    {
                        if (_designTime == null)
                        {
                            _designTime = GetDesignTimeViewModel1();
                        }
                    }
                }

                return _designTime;
            }
        }

        private static SarifErrorListItem GetDesignTimeViewModel1()
        {
            SarifErrorListItem viewModel = new SarifErrorListItem();
            viewModel.Message = "Potential mismatch between sizeof and countof quantities. Use sizeof() to scale byte sizes.";

            viewModel.Tool = new ToolModel()
            {
                Name = "FxCop",
                Version = "1.0.0.0",
            };

            viewModel.Rule = new RuleModel()
            {
                Id = "CA1823",
                Name = "Avoid unused private fields",
                HelpUri = "http://aka.ms/analysis/ca1823",
                DefaultLevel = "Unknown"
            };

            viewModel.Invocation = new InvocationModel()
            {
                CommandLine = @"""C:\Temp\Foo.exe"" target.file /o out.sarif",
                FileName = @"C:\Temp\Foo.exe",
            };

            viewModel.Locations.Add(new Models.CodeFlowLocationModel()
            {
                FilePath = @"D:\GitHub\NuGet.Services.Metadata\src\Ng\Catalog2Dnx.cs",
                Region = new Region(11, 1, 11, 2, 0, 0, snippet: null),
            });

            viewModel.Locations.Add(new Models.CodeFlowLocationModel()
            {
                FilePath = @"D:\GitHub\NuGet.Services.Metadata\src\Ng\Catalog2Dnx.cs",
                Region = new Region(12, 1, 12, 2, 0, 0, snippet: null),
            });

            viewModel.RelatedLocations.Add(new Models.CodeFlowLocationModel()
            {
                FilePath = @"D:\GitHub\NuGet.Services.Metadata\src\Ng\Catalog2Dnx.cs",
                Region = new Region(21, 1, 21, 2, 0, 0, snippet: null),
            });

            viewModel.RelatedLocations.Add(new Models.CodeFlowLocationModel()
            {
                FilePath = @"D:\GitHub\NuGet.Services.Metadata\src\Ng\Catalog2Dnx.cs",
                Region = new Region(22, 1, 22, 2, 0, 0, snippet: null),
            });

            viewModel.RelatedLocations.Add(new Models.CodeFlowLocationModel()
            {
                FilePath = @"D:\GitHub\NuGet.Services.Metadata\src\Ng\Catalog2Dnx.cs",
                Region = new Region(23, 1, 23, 2, 0, 0, snippet: null),
            });

            viewModel.CallTrees.Add(new CallTree(
                new List<CallTreeNode>
                {
                    new CallTreeNode
                    {
                        Location = new CodeFlowLocation()
                    },

                    new CallTreeNode
                    {
                        Location = new CodeFlowLocation(),
                        Children = new List<CallTreeNode>
                        {
                            new CallTreeNode
                            {
                                Location = new CodeFlowLocation()
                            }
                        }
                    }
                }, SarifViewerPackage.SarifToolWindow));

            StackCollection stack1 = new StackCollection("Stack A1");
            stack1.Add(new StackFrameModel()
            {
                Message = "Message A1.1",
                FilePath = @"D:\GitHub\NuGet.Services.Metadata\src\Ng\Catalog2Dnx.cs",
                Line = 11,
                Column = 1,
                FullyQualifiedLogicalName ="My.Assembly.Main(string[] args)",
                Module = "My.Module.dll",
            });
            stack1.Add(new StackFrameModel()
            {
                Message = "Message A1.2",
                FilePath = @"D:\GitHub\NuGet.Services.Metadata\src\Ng\Catalog2Dnx.cs",
                Line = 12,
                Column = 1,
                FullyQualifiedLogicalName = "Our.Shared.Library.Method(int param)",
                Module = "My.Module.dll",
            });
            stack1.Add(new StackFrameModel()
            {
                Message = "Message A1.3",
                FilePath = @"D:\GitHub\NuGet.Services.Metadata\src\Ng\Catalog2Dnx.cs",
                Line = 1,
                Column = 1,
                FullyQualifiedLogicalName = "Your.PIA.External()",
            });
            viewModel.Stacks.Add(stack1);

            FixModel fix1 = new FixModel("Replace *.Close() with *.Dispose().", new FileSystem());
            FileChangeModel fileChange11 = new FileChangeModel();
            fileChange11.FilePath = @"D:\GitHub\NuGet.Services.Metadata\src\Ng\Catalog2Dnx.cs";
            fileChange11.Replacements.Add(new ReplacementModel()
            {
                Offset = 1234,
                DeletedLength = ".Close()".Length,
                InsertedString = ".Dispose()",
            });
            fix1.FileChanges.Add(fileChange11);
            viewModel.Fixes.Add(fix1);

            return viewModel;
        }
    }
}
