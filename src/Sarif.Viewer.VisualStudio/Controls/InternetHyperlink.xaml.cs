// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Microsoft.Sarif.Viewer.Controls
{
    /// <summary>
    /// Interaction logic for InternetHyperlink.xaml
    /// </summary>
    public partial class InternetHyperlink : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(InternetHyperlink));
        public static readonly DependencyProperty NavigateUriProperty = DependencyProperty.Register("NavigateUri", typeof(string), typeof(InternetHyperlink));

        public InternetHyperlink()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The text to display for the hyperlink.
        /// </summary>
        public string Text
        {
            get
            {
                return (string)this.GetValue(TextProperty);
            }
            set
            {
                this.SetValue(TextProperty, value);
            }
        }

        /// <summary>
        /// The URI to navigate to when the hyperlink is clicked.
        /// </summary>
        public string NavigateUri
        {
            get
            {
                return (string)this.GetValue(NavigateUriProperty);
            }
            set
            {
                this.SetValue(NavigateUriProperty, value);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if (this.NavigateUri != null)
            {
                System.Diagnostics.Process.Start(this.NavigateUri);
            }
        }
    }
}
