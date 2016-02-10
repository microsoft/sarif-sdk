using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        }

        public ObservableCollection<AnnotatedCodeLocationModel> Items { get; internal set; }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public void SetItems(ObservableCollection<AnnotatedCodeLocationModel> items)
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

        private void Expander_Click(object sender, RoutedEventArgs e)
        {
            Expander senderExp = (Expander)sender;
            object obj = senderExp.Tag;
            if (obj is ListViewItem)
            {
                ((ListViewItem)obj).IsSelected = true;
            }
        }
    }
}
