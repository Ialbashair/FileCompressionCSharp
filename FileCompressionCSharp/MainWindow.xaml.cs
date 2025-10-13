using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using LogicLayerInterface;
using DataObjects;
using System.IO;

namespace FileCompressionCSharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IArchiveTypeChecker _checker;
        private string selectedPath = string.Empty;
        private bool fileSelected = false;        
        private ArchiveType algorithim = ArchiveType.None;
        public MainWindow(IArchiveTypeChecker checker)
        {
            InitializeComponent();
            _checker = checker;
        }

        // Date Created: 10/13/2025 5:05 PM
        // Last Modified: N/A
        // Description: Creates output path based on input path and archive type
        public void CreateOutputPath(string inputPath, ArchiveType type)
        {
            inputPath = selectedPath;
            string directory = Path.GetDirectoryName(inputPath);
            string baseName = Path.GetFileNameWithoutExtension(inputPath);
            string outputPath = Path.Combine(directory,baseName + ArchiveTypeExtensions.GetExtension(algorithim));
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
                FlipButtons(false);
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

                // Create a new unfrozen brush each time
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
                FlipButtons(true);
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
            FlipButtons(true);
        }
        

        // Date Created: 10/3/2025 10:22 PM
        // Last Modified: N/A
        // Description: Handles enabling/disabling buttons based on file selection and type
        private void FlipButtons(bool active)
        {
            if (active)
            {
                // Disable Compress button
                btnCompress.IsEnabled = false;
                btnCompress.Foreground = Brushes.DarkGray;
                btnCompress.Opacity = 0.5;
                btnCompress.ToolTip = "A File must be selected";
                
                // Disable Decompress button
                btnDecompress.IsEnabled = false;
                btnDecompress.Foreground = Brushes.DarkGray;
                btnDecompress.Opacity = 0.5;
                btnDecompress.ToolTip = "A File must be selected";

            }
            else if (!active && !_checker.GetArchiveType(selectedPath).Equals(ArchiveType.None))
            {
                // Enable Decompress button
                btnDecompress.IsEnabled = true;
                btnDecompress.Foreground = Brushes.White;
                btnDecompress.Opacity = 1.0;
                btnDecompress.ToolTip = null;

                // Disable Compress button
                btnCompress.IsEnabled = false;
                btnCompress.Foreground = Brushes.DarkGray;
                btnCompress.Opacity = 0.5;
                btnCompress.ToolTip = "A File must be selected";

            }
            else if (!active && _checker.GetArchiveType(selectedPath).Equals(ArchiveType.None))
            {
                // Enable Compress button
                btnCompress.IsEnabled = true;
                btnCompress.Foreground = Brushes.White;
                btnCompress.Opacity = 1.0;
                btnCompress.ToolTip = null;

                // Disable Decompress button
                btnDecompress.IsEnabled = false;
                btnDecompress.Foreground = Brushes.DarkGray;
                btnDecompress.Opacity = 0.5;
                btnDecompress.ToolTip = "A File must be selected";
            }
            else
            {
                // Enable Decompress button
                btnDecompress.IsEnabled = true;
                btnDecompress.Foreground = Brushes.White;
                btnDecompress.Opacity = 1.0;
                btnDecompress.ToolTip = null;

                // Enable Compress button
                btnCompress.IsEnabled = true;
                btnCompress.Foreground = Brushes.White;
                btnCompress.Opacity = 1.0;
                btnCompress.ToolTip = null;
            }
        }

        // Algorithm check - RadioButtonEvents
        private void btnHuffman_Checked(object sender, RoutedEventArgs e)
        {
            btnBoth.IsChecked = false;
            btnSlidingWindow.IsChecked = false;
            
            btnHuffman.Background = Brushes.DarkGray;
        }

        private void btnSlidingWindow_Checked(object sender, RoutedEventArgs e)
        {
            btnBoth.IsChecked = false;
            btnHuffman.IsChecked = false;
        }

        private void btnBoth_Checked(object sender, RoutedEventArgs e)
        {
            btnHuffman.IsChecked = false;
            btnSlidingWindow.IsChecked = false;
        }
    }
}
