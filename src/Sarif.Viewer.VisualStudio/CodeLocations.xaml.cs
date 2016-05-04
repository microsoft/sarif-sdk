using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Microsoft.Sarif.Viewer
{
    /// <summary>
    /// Interaction logic for CodeLocations.xaml
    /// </summary>
    public partial class CodeLocations : UserControl
    {
        public CodeLocations()
        {
            InitializeComponent();
            this.detailsGrid.DataContext = this;
        }

        public ObservableCollection<AnnotatedCodeLocationModel> Items { get; internal set; }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
        }

        SarifError _sarifError;
        public SarifError CurrentSarifError
        {
            get
            {
                return _sarifError;
            }

            set
            {
                _sarifError = value;
                if (_sarifError != null)
                {
                    SetItems(_sarifError.Annotations);
                }
            }
        }

        private void SetItems(ObservableCollection<AnnotatedCodeLocationModel> items)
        {
            // Set up data source 
            CollectionViewSource _csv = new CollectionViewSource();
            _csv.Source = items;

            HashSet<int> stacks = new HashSet<int>();
            HashSet<int> executionFlows = new HashSet<int>();

            foreach(AnnotatedCodeLocationModel annotatedCodeLocationModel in items)
            {
                if (annotatedCodeLocationModel.Kind == AnnotatedCodeLocationKind.Stack)
                {
                    stacks.Add(annotatedCodeLocationModel.Index);
                }
                else
                {
                    executionFlows.Add(annotatedCodeLocationModel.Index);
                }

                if (stacks.Count > 1 && executionFlows.Count > 1) { break; }
            }

            if (stacks.Count > 1 && executionFlows.Count > 1)
            {
                _csv.GroupDescriptions.Add(new PropertyGroupDescription("Kind"));
            }

            if (stacks.Count > 1 || executionFlows.Count > 1)
            {
                _csv.GroupDescriptions.Add(new PropertyGroupDescription("Index"));
            }

            this.codeLocationsListView.DataContext = _csv;
        }

        private void CodeLocationsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HandleLineInformationListSelectionChanged(sender, e);
        }

        private void HelpHyperlink_Click(object sender, RoutedEventArgs e)
        {
            // HandleHelpHyperlinkClick(sender);
        }

        private void FileAndCategoryGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // HandleFileAndCategoryGridSizeChanged(sender);
        }

        private static void HandleLineInformationListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Deselecting previously-selected result
            if (e.RemovedItems.Count > 0)
            {
                for (int i = 0; i < e.RemovedItems.Count; i++)
                {
                    var item = e.RemovedItems[i] as AnnotatedCodeLocationModel;

                    if (item != null)
                    {
                        item.IsSelected = false;
                        item.OnDeselectKeyEvent();
                    }
                }
            }

            // Selecting new result
            if (e.AddedItems.Count > 0)
            {
                for (int i = 0; i < e.AddedItems.Count; i++)
                {
                    var item = e.AddedItems[i] as AnnotatedCodeLocationModel;
                    if (item != null)
                    {
                        item.IsSelected = true;
                        item.OnSelectKeyEvent();
                    }
                }
            }

            e.Handled = true;
        }

        private static AnnotatedCodeLocationModel GetCodeAnalysisItemFromSender(object eventSender)
        {
            var fce = eventSender as FrameworkContentElement;
            if (fce != null)
            {
                return fce.DataContext as AnnotatedCodeLocationModel;
            }
            else
            {
                // Let's also try FrameworkElement depending on who sent the event
                var fe = eventSender as FrameworkElement;
                if (fe != null)
                {
                    return fe.DataContext as AnnotatedCodeLocationModel;
                }
            }

            return null;
        }
    }
}
