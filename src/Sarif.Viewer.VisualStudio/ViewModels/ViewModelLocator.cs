using Microsoft.Sarif.Viewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Microsoft.Sarif.Viewer.ViewModels
{
    public static class ViewModelLocator
    {
        static object _syncroot = new object();
        static ResultViewModel _designTime = null;

        public static ResultViewModel DesignTime
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

        private static ResultViewModel GetDesignTimeViewModel1()
        {
            ResultViewModel viewModel = new ResultViewModel();
            viewModel.RuleId = "CA1823";
            viewModel.RuleName = "Avoid unused private fields";
            viewModel.Message = "Potential mismatch between sizeof and countof quantities. Use sizeof() to scale byte sizes.";

            AnnotatedCodeLocationCollection location1 = new AnnotatedCodeLocationCollection(String.Empty);
            location1.Add(new AnnotatedCodeLocation()
            {
                FilePath = @"D:\GitHub\Jinu-NuGet.Services.Metadata\src\Ng\Catalog2Dnx.cs",
                Region = new CodeAnalysis.Sarif.Region(11, 1, 11, 2, 0, 0),
            });
            viewModel.Locations.Add(location1);

            AnnotatedCodeLocationCollection location2 = new AnnotatedCodeLocationCollection(String.Empty);
            location2.Add(new AnnotatedCodeLocation()
            {
                FilePath = @"D:\GitHub\Jinu-NuGet.Services.Metadata\src\Ng\Catalog2Dnx.cs",
                Region = new CodeAnalysis.Sarif.Region(12, 1, 12, 2, 0, 0),
            });
            viewModel.Locations.Add(location2);

            AnnotatedCodeLocationCollection relatedLocation1 = new AnnotatedCodeLocationCollection("");
            relatedLocation1.Add(new AnnotatedCodeLocation()
            {
                FilePath = @"D:\GitHub\Jinu-NuGet.Services.Metadata\src\Ng\Catalog2Dnx.cs",
                Region = new CodeAnalysis.Sarif.Region(21, 1, 21, 2, 0, 0),
            });
            viewModel.RelatedLocations.Add(relatedLocation1);

            AnnotatedCodeLocationCollection relatedLocation2 = new AnnotatedCodeLocationCollection("");
            relatedLocation2.Add(new AnnotatedCodeLocation()
            {
                FilePath = @"D:\GitHub\Jinu-NuGet.Services.Metadata\src\Ng\Catalog2Dnx.cs",
                Region = new CodeAnalysis.Sarif.Region(22, 1, 22, 2, 0, 0),
            });
            viewModel.RelatedLocations.Add(relatedLocation2);

            AnnotatedCodeLocationCollection relatedLocation3 = new AnnotatedCodeLocationCollection("");
            relatedLocation3.Add(new AnnotatedCodeLocation()
            {
                FilePath = @"D:\GitHub\Jinu-NuGet.Services.Metadata\src\Ng\Catalog2Dnx.cs",
                Region = new CodeAnalysis.Sarif.Region(23, 1, 23, 2, 0, 0),
            });
            viewModel.RelatedLocations.Add(relatedLocation3);

            AnnotatedCodeLocationCollection codeFlows1 = new AnnotatedCodeLocationCollection("Code Flows A");
            codeFlows1.Add(new AnnotatedCodeLocation()
            {
                Message = "Message A1",
                FilePath = @"D:\GitHub\Jinu-NuGet.Services.Metadata\src\Ng\Catalog2Dnx.cs",
                Region = new CodeAnalysis.Sarif.Region(11, 1, 11, 2, 0, 0),
            });
            codeFlows1.Add(new AnnotatedCodeLocation()
            {
                Message = "Message A2",
                FilePath = @"D:\GitHub\Jinu-NuGet.Services.Metadata\src\Ng\Catalog2Dnx.cs",
                Region = new CodeAnalysis.Sarif.Region(12, 1, 12, 2, 0, 0),
            });
            codeFlows1.Add(new AnnotatedCodeLocation()
            {
                Message = "Message A3",
                FilePath = @"D:\GitHub\Jinu-NuGet.Services.Metadata\src\Ng\Catalog2Dnx.cs",
                Region = new CodeAnalysis.Sarif.Region(13, 1, 13, 2, 0, 0),
            });
            viewModel.CodeFlows.Add(codeFlows1);

            AnnotatedCodeLocationCollection codeFlows2 = new AnnotatedCodeLocationCollection("Code Flows B");
            codeFlows2.Add(new AnnotatedCodeLocation()
            {
                Message = "Message B1",
                FilePath = @"D:\GitHub\Jinu-NuGet.Services.Metadata\src\Ng\Catalog2Dnx.cs",
                Region = new CodeAnalysis.Sarif.Region(11, 1, 11, 2, 0, 0),
            });
            codeFlows2.Add(new AnnotatedCodeLocation()
            {
                Message = "Message B2",
                FilePath = @"D:\GitHub\Jinu-NuGet.Services.Metadata\src\Ng\Catalog2Dnx.cs",
                Region = new CodeAnalysis.Sarif.Region(12, 1, 12, 2, 0, 0),
            });
            codeFlows2.Add(new AnnotatedCodeLocation()
            {
                Message = "Message B3",
                FilePath = @"D:\GitHub\Jinu-NuGet.Services.Metadata\src\Ng\Catalog2Dnx.cs",
                Region = new CodeAnalysis.Sarif.Region(13, 1, 13, 2, 0, 0),
            });
            codeFlows2.Add(new AnnotatedCodeLocation()
            {
                Message = "Message B4",
                FilePath = @"D:\GitHub\Jinu-NuGet.Services.Metadata\src\Ng\Catalog2Dnx.cs",
                Region = new CodeAnalysis.Sarif.Region(13, 1, 13, 2, 0, 0),
            });
            viewModel.CodeFlows.Add(codeFlows2);

            StackCollection stack1 = new StackCollection("Stack A1");
            stack1.Add(new StackFrame()
            {
                Message = "Message A1.1",
                FilePath = @"D:\GitHub\Jinu-NuGet.Services.Metadata\src\Ng\Catalog2Dnx.cs",
                Line = 11,
                Column = 1,
                FullyQualifiedLogicalName ="My.Assembly.Main(string[] args)",
                Module = "My.Module.dll",
            });
            stack1.Add(new StackFrame()
            {
                Message = "Message A1.2",
                FilePath = @"D:\GitHub\Jinu-NuGet.Services.Metadata\src\Ng\Catalog2Dnx.cs",
                Line = 12,
                Column = 1,
                FullyQualifiedLogicalName = "Our.Shared.Library.Method(int param)",
                Module = "My.Module.dll",
            });
            stack1.Add(new StackFrame()
            {
                Message = "Message A1.3",
                FilePath = @"D:\GitHub\Jinu-NuGet.Services.Metadata\src\Ng\Catalog2Dnx.cs",
                Line = 1,
                Column = 1,
                FullyQualifiedLogicalName = "Your.PIA.External()",
            });
            viewModel.Stacks.Add(stack1);

            return viewModel;
        }
    }
}
