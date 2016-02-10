using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Driver.Sdk;
using Microsoft.Sarif.Viewer;
using Microsoft.Win32;

namespace ErrorDetailsTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        List<Tuple<string, string>> _remappedPathPrefixes = new List<Tuple<string, string>>();

        private void LoadSarifButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = SarifFile.OpenFileTitle,
                Filter = SarifFile.OpenFileFilter
            };

            if (!ofd.ShowDialog().Value)
            {
                return;
            }

            PopulateControlFromSarifText(File.ReadAllText(ofd.FileName));
        }

        private void PopulateControlFromSarifText(string sarifText)
        {
            ResultLog log = SarifFile.CreateFromText(sarifText);

            // TODO currently we just grovel for the first result, if any,
            // that has either a stack or an execution flow

            bool foundData = false;

            ObservableCollection<AnnotatedCodeLocationModel> items = new ObservableCollection<AnnotatedCodeLocationModel>();

            foreach (RunLog runLog in log.RunLogs)
            {
                if (foundData) { break; }

                if (runLog.Results != null)
                {
                    foreach (Result result in runLog.Results)
                    {
                        if (result.Stacks != null)
                        {
                            PopulateControl(items, result.Stacks, AnnotatedCodeLocationKind.Stack);
                            foundData = true;
                        }

                        if (result.ExecutionFlows != null)
                        {
                            PopulateControl(items, result.ExecutionFlows, AnnotatedCodeLocationKind.ExecutionFlow);
                            foundData = true;
                        }

                        if (foundData) { break; }
                    }
                }
            }
            this.codeLocations.SetItems(items);
        }

        private string RebaselineFileName(string fileName, List<Tuple<string, string>> remappedPathPrefixes)
        {
            if (File.Exists(fileName)) { return fileName; }
            // First, we'll traverse our remappings and see if we can
            // make rebaseline from existing data
            foreach (Tuple<string, string> remapping in remappedPathPrefixes)
            {
                string remapped = fileName.Replace(remapping.Item1, remapping.Item2);
                if (File.Exists(remapped))
                {
                    return remapped;
                }
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();

            string fullPath = Path.GetFullPath(fileName);
            string shortName = Path.GetFileName(fullPath);

            openFileDialog.Title = "Locate missing file: " + fullPath;
            openFileDialog.Filter = shortName + "|" + shortName;
            openFileDialog.RestoreDirectory = true;

            bool? openFileResult = openFileDialog.ShowDialog();

            if (string.IsNullOrEmpty(openFileDialog.FileName))
            {
                return null;
            }

            if (!File.Exists(fileName))
            {
                return fileName;
            }

            string resolvedPath = openFileDialog.FileName;
            string resolvedFileName = Path.GetFileName(resolvedPath);

            // If remapping has somehow altered the file name itself,
            // we will bail on attempting to do any remapping
            if (Path.GetFileName(fullPath) != resolvedFileName)
            {
                return fileName;
            }

            int offset = resolvedFileName.Length;
            while ((resolvedPath.Length - offset) >= 0 &&
                   (fullPath.Length - offset) >= 0)
            {
                if (!resolvedPath[resolvedPath.Length - offset].ToString().Equals(fullPath[fullPath.Length - offset].ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                offset++;
            }

            offset--;

            // At this point, we've got our hands on the common suffix for both the 
            // original file path and the resolved location. we trim this off both
            // values and then add a remapping that converts one to the other
            string originalPrefix = fullPath.Substring(0, fullPath.Length - offset);
            string resolvedPrefix = resolvedPath.Substring(0, resolvedPath.Length - offset);

            remappedPathPrefixes.Add(new Tuple<string, string>(originalPrefix, resolvedPrefix));

            return resolvedPath;
        }



        private void PopulateControl(ObservableCollection<AnnotatedCodeLocationModel> items, IList<IList<AnnotatedCodeLocation>> annotatedCodeLocationSets, AnnotatedCodeLocationKind kind)
        {
            int index = 0;

            NewLineIndex newLineIndex = null;
            Dictionary<string, NewLineIndex> fileToNewLineIndexMap = new Dictionary<string, NewLineIndex>();

            foreach (IList<AnnotatedCodeLocation> annotatedCodeLocations in annotatedCodeLocationSets)
            {
                index++; // Labels are 1-indexed in UI
                foreach(AnnotatedCodeLocation codeLocation in annotatedCodeLocations)
                {
                    PhysicalLocationComponent plc = codeLocation.PhysicalLocation[0];
                    string fileName = plc.Uri.LocalPath;
                    Region region = plc.Region;

                    if (!File.Exists(fileName))
                    {
                        fileName = RebaselineFileName(fileName, _remappedPathPrefixes);

                        if (fileName == null)
                        {
                            // User cancelled
                            items.Clear();
                            return;
                        }
                    }

                    if (!fileToNewLineIndexMap.TryGetValue(fileName, out newLineIndex))
                    {
                        fileToNewLineIndexMap[fileName] = newLineIndex = new NewLineIndex(File.ReadAllText(fileName));
                    }
                    region.Populate(newLineIndex);

                    items.Add(new AnnotatedCodeLocationModel()
                    {
                        Index = index,
                        Kind = kind,
                        Message = codeLocation.Message,
                        Region = region,
                        FilePath = fileName,                         
                    });
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string sarifText = File.ReadAllText(@"D:\repros\CppSa\espx\test\Expected-003.sarif");
            PopulateControlFromSarifText(sarifText);
        }

        private void LoadPREfastButton_Click(object sender, RoutedEventArgs e)
        {
            string title = "Open PREfast XML log file";
            string filter = "PREfast log files (*.xml)|*.xml";

            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = title,
                Filter = filter
            };

            if (!ofd.ShowDialog().Value)
            {
                return;
            }

            string sarifText = ToolFormatConverter.ConvertPREfastToStandardFormat(ofd.FileName);

            // TODO DON'T CHECK THIS IN
            File.WriteAllText(@"d:\repros\converted_prefast.sarif", sarifText);

            PopulateControlFromSarifText(sarifText);
        }
    }
}
