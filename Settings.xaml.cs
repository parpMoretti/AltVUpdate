using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Avalon.Windows.Dialogs;

namespace AltVUpdate
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            Task.Delay(300);
            InitializeComponent();

            BuildBox.Items.Add("Release");
            BuildBox.Items.Add("RC");
            BuildBox.Items.Add("Dev");

            BuildBox.SelectedItem = User.Default.Branch;
            DirectoryBox.MouseDoubleClick += DirectoryBrowseButton_OnClick;
            DirectoryBox.Text = User.Default.Directory;
            CSharpBox.IsChecked = User.Default.CSharp;
            NodeBox.IsChecked = User.Default.NodeJs;
        }

        private void DirectoryBrowseButton_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.Title = "Browse for a Folder";
            dialog.SelectedPath = DirectoryBox.Text;
            dialog.Title = DirectoryBox.Text;
            bool? dialogBool = dialog.ShowDialog();
            if (dialogBool == true)
            {
                DirectoryBox.Text = dialog.SelectedPath;
            }
        }

        private void SaveConfigButton_OnClick(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            User.Default.Branch = BuildBox.SelectedItem.ToString();
            User.Default.Directory = DirectoryBox.Text;

            bool isUsingCSharp = CSharpBox.IsChecked ?? false;
            bool isUsingNodeJs = NodeBox.IsChecked ?? false;

            User.Default.CSharp = isUsingCSharp;
            User.Default.NodeJs = isUsingNodeJs;

            User.Default.Save();
            User.Default.Reload();

            Mouse.OverrideCursor = null;

            this.Close();
        }
    }
}