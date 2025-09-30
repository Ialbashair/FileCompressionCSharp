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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileCompressionCSharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string selectedPath = string.Empty;
        private bool fileSelected = false;
        public MainWindow()
        {
            InitializeComponent();
        }


        // Date Created: 9/28/2025 11:06:00 PM
        // Last Modified: N/A
        // Description: Handles file/folder drag and drop functionality
        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    selectedPath = files[0];
                    SelectedPath.Text = selectedPath;
                }
            }
        }

        // Date Created: 9/28/2025 11:06:00 PM
        // Last Modified: N/A
        // Description: Handles drag over event to provide visual feedback
        private void Window_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        // Date Created: 9/28/2025 11:06:00 PM
        // Last Modified: N/A
        // Description: Handles the Browse button click to open a file dialog
        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                CheckFileExists = true,
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                selectedPath = dialog.FileName;
                SelectedPath.Text = selectedPath;
                fileSelected = true;
            }
            else 
            {
                fileSelected = false;
            }

            if (fileSelected)
            {
                ActivateButtons(false);
            }
        }

        // Date Created: 9/28/2025 11:06:00 PM
        // Last Modified: N/A
        // Description: Handles the Clear button click to reset the selected path
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedPath) || SelectedPath.Text == "No file/folder selected")
            {
                SelectedPath.Text = "No file/folder selected";

                // Create a *new* unfrozen brush each time
                var brush = new SolidColorBrush(Colors.Red);
                SelectedPath.Foreground = brush;

                var fadeToGray = new ColorAnimation
                {
                    To = Colors.LightGray,
                    Duration = TimeSpan.FromSeconds(1)
                };

                brush.BeginAnimation(SolidColorBrush.ColorProperty, fadeToGray);
            }
            else
            {
                selectedPath = string.Empty;
                SelectedPath.Text = "No file/folder selected";
                ActivateButtons(true);
            }

        }

        private void Compress_Click(object sender, RoutedEventArgs e) 
        {

        }
        private void Decompress_Click(object sender, RoutedEventArgs e)
        {

        }

        // Date Created: 9/29/2025 7:45 PM
        // Last Modified: N/A
        // Description: Handles window loaded events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region Buttons
            ActivateButtons(true);           
            #endregion
        }

        bool IsZip(string path) // NEEDS WORK
        {
           /* byte[] header = new byte[4];
            using (var stream = File.OpenRead(path))
            {
                stream.Read(header, 0, header.Length);
            }

            return header[0] == 0x50 && header[1] == 0x4B && header[2] == 0x03 && header[3] == 0x04;*/
        }

        private void ActivateButtons(bool Active) 
        {
            if (Active)
            {
                // Compress button
                btnCompress.IsEnabled = false;
                btnCompress.Foreground = Brushes.DarkGray;
                btnCompress.Opacity = 0.5;
                btnCompress.ToolTip = "A File must be selected";
                // Decompress button
                btnDecompress.IsEnabled = false;
                btnDecompress.Foreground = Brushes.DarkGray;
                btnDecompress.Opacity = 0.5;
                btnDecompress.ToolTip = "A File must be selected";
            }
            else
            {
                // Enable Compress button
                btnCompress.IsEnabled = true;
                btnCompress.Foreground = Brushes.White;
                btnCompress.Opacity = 1.0;
                btnCompress.ToolTip = null;
                // Enable Decompress button
                btnDecompress.IsEnabled = true;
                btnDecompress.Foreground = Brushes.White;
                btnDecompress.Opacity = 1.0;
                btnDecompress.ToolTip = null;
            }
        }
    }
}
