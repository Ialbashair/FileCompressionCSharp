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
            ResetAlgorithmSelection();
        }

        // Date Created: 10/13/2025 5:05 PM
        // Last Modified: N/A
        // Description: Creates output path based on input path and archive type
        public void CreateOutputPath(string inputPath, ArchiveType type)
        {
            if (algorithim == ArchiveType.None) 
            {
                MessageBox.Show("Please select a valid compression algorithm.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
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


        public void ResetAlgorithmSelection()
        {
            algorithim = ArchiveType.None;

            btnBoth.IsEnabled = false;
            btnBoth.IsChecked = false;
            btnBoth.Foreground = Brushes.DarkGray;
            btnBoth.Opacity = 0.5;

            btnHuffman.IsEnabled = false;
            btnHuffman.IsChecked = false;
            btnHuffman.Foreground = Brushes.DarkGray;
            btnHuffman.Opacity = 0.5;

            btnSlidingWindow.IsEnabled = false;
            btnSlidingWindow.IsChecked = false;
            btnSlidingWindow.Foreground = Brushes.DarkGray;
            btnSlidingWindow.Opacity = 0.5;
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
                ResetAlgorithmSelection(); // Reset algorithm selection
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
                btnBoth.IsChecked = false;
                btnHuffman.IsChecked = false;
                btnSlidingWindow.IsChecked = false;
                ResetAlgorithmSelection();
                algorithim = ArchiveType.None;
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
        // Last Modified: 10/13/20205 5:15 PM - Refactored to use ArchiveTypeChecker and handle Algorithm selection state
        // Description: Handles enabling/disabling buttons based on file selection and type
        private void FlipButtons(bool active)
        {
            // If inactive flag means "no file selected" -> disable both
            if (active)
            {
                // Disable both buttons and show tooltips
                btnCompress.IsEnabled = false;
                btnCompress.Foreground = Brushes.DarkGray;
                btnCompress.Opacity = 0.5;
                btnCompress.ToolTip = "A file must be selected";

                btnDecompress.IsEnabled = false;
                btnDecompress.Foreground = Brushes.DarkGray;
                btnDecompress.Opacity = 0.5;
                btnDecompress.ToolTip = "A file must be selected";

                // When no file selected, algorithm panel should be disabled
                ResetAlgorithmSelection();
                return;
            }

            // At this point, active == false means a file *has been* selected.
            // Guard: ensure we have a path
            if (string.IsNullOrEmpty(selectedPath) || !File.Exists(selectedPath))
            {
                // treat as no file selected
                btnCompress.IsEnabled = false;
                btnDecompress.IsEnabled = false;
                ResetAlgorithmSelection();
                return;
            }

            // Get archive type once
            ArchiveType fileType = _checker.GetArchiveType(selectedPath);

            // If the checker says the file *is an archive* we should enable Decompress only
            if (fileType != ArchiveType.None)
            {
                btnDecompress.IsEnabled = true;
                btnDecompress.Foreground = Brushes.White;
                btnDecompress.Opacity = 1.0;
                btnDecompress.ToolTip = null;

                btnCompress.IsEnabled = false;
                btnCompress.Foreground = Brushes.DarkGray;
                btnCompress.Opacity = 0.5;
                btnCompress.ToolTip = "File is already an archive";

                // When file is an archive, user shouldn't change algorithm for compression
                ResetAlgorithmSelection();

                // Reset algorithm selection (optional)
                // algorithim = ArchiveType.None;

                return;
            }

            // fileType == None -> file is NOT a recognized archive
            // If an algorithm is selected, enable Compress; else keep both disabled and allow algorithm selection
            btnSlidingWindow.IsEnabled = true;
            btnSlidingWindow.Foreground = Brushes.White;
            btnSlidingWindow.Opacity = 1.0;
            btnHuffman.IsEnabled = true;
            btnHuffman.Foreground = Brushes.White;
            btnHuffman.Opacity = 1.0;
            btnBoth.IsEnabled = true;
            btnBoth.Foreground = Brushes.White;
            btnBoth.Opacity = 1.0;


            if (algorithim != ArchiveType.None)
            {
                btnCompress.IsEnabled = true;
                btnCompress.Foreground = Brushes.White;
                btnCompress.Opacity = 1.0;
                btnCompress.ToolTip = null;
            }
            else
            {
                btnCompress.IsEnabled = false;
                btnCompress.Foreground = Brushes.DarkGray;
                btnCompress.Opacity = 0.5;
                btnCompress.ToolTip = "Select a compression algorithm";
            }

            // Ensure Decompress is disabled for non-archive files
            btnDecompress.IsEnabled = false;
            btnDecompress.Foreground = Brushes.DarkGray;
            btnDecompress.Opacity = 0.5;
            btnDecompress.ToolTip = "Not an archive file";
        }


        // Algorithm check - RadioButtonEvents
        private void btnHuffman_Checked(object sender, RoutedEventArgs e)
        {
            btnBoth.IsChecked = false;
            btnSlidingWindow.IsChecked = false;

            algorithim = ArchiveType.Huffman;            

            // Enable Compress button
            btnCompress.IsEnabled = true;
            btnCompress.Foreground = Brushes.White;
            btnCompress.Opacity = 1.0;
            btnCompress.ToolTip = null;
        }

        private void btnSlidingWindow_Checked(object sender, RoutedEventArgs e)
        {
            btnBoth.IsChecked = false;
            btnHuffman.IsChecked = false;

            // Enable Compress button
            btnCompress.IsEnabled = true;
            btnCompress.Foreground = Brushes.White;
            btnCompress.Opacity = 1.0;
            btnCompress.ToolTip = null;
        }

        private void btnBoth_Checked(object sender, RoutedEventArgs e)
        {
            btnHuffman.IsChecked = false;
            btnSlidingWindow.IsChecked = false;

            // Enable Compress button
            btnCompress.IsEnabled = true;
            btnCompress.Foreground = Brushes.White;
            btnCompress.Opacity = 1.0;
            btnCompress.ToolTip = null;
        }
    }
}
