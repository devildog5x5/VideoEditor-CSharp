using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VideoEditor.Models;
using VideoEditor.ViewModels;

namespace VideoEditor
{
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }

        public MainWindow(MainViewModel viewModel)
        {
            try
            {
                InitializeComponent();
                
                if (viewModel == null)
                {
                    throw new ArgumentNullException(nameof(viewModel));
                }
                
                ViewModel = viewModel;
                DataContext = ViewModel;
                
                // Connect timeline selection to properties
                if (ViewModel.TimelineViewModel != null)
                {
                    ViewModel.TimelineViewModel.PropertyChanged += (s, e) =>
                    {
                        try
                        {
                            if (e.PropertyName == nameof(ViewModels.TimelineViewModel.SelectedClip) &&
                                ViewModel.PropertiesViewModel != null)
                            {
                                ViewModel.PropertiesViewModel.SetClip(ViewModel.TimelineViewModel.SelectedClip);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in timeline property changed handler: {ex.Message}");
                        }
                    };
                }

                // Enable drag and drop on window
                this.Drop += MainWindow_Drop;
                this.DragOver += MainWindow_DragOver;

                // Update timeline hint visibility when clips are added
                if (ViewModel.TimelineViewModel != null)
                {
                    ViewModel.TimelineViewModel.PropertyChanged += (s, e) =>
                    {
                        // Timeline hint will be hidden when clips are added (handled by binding if we add it)
                    };
                }

                // Load demo video after window is loaded
                this.Loaded += MainWindow_Loaded;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in MainWindow constructor: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show(
                    $"Failed to initialize main window: {ex.Message}",
                    "Initialization Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                throw;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel == null)
                {
                    System.Diagnostics.Debug.WriteLine("ViewModel is null in MainWindow_Loaded");
                    return;
                }

                // Ensure demo video loads after UI is ready
                var demoPath = @"C:\Users\rober\Documents\GitHub\VideoEditor-CSharp\Samples\sample_video.mp4";
                if (System.IO.File.Exists(demoPath))
                {
                    // Load the demo video directly on UI thread
                    try
                    {
                        // Create media file directly
                        var mediaFile = new VideoEditor.Models.MediaFile
                        {
                            FilePath = demoPath,
                            FileName = System.IO.Path.GetFileName(demoPath)
                        };
                        
                        // Add to collection on UI thread
                        if (ViewModel.MediaFiles != null)
                        {
                            ViewModel.MediaFiles.Add(mediaFile);
                            ViewModel.SelectedMediaFile = mediaFile;
                            ViewModel.StatusMessage = $"Demo video loaded: {mediaFile.FileName}";
                            
                            // Trigger preview loading
                            Task.Run(() =>
                            {
                                try
                                {
                                    System.Threading.Thread.Sleep(500); // Give UI time to update
                                    if (ViewModel != null)
                                    {
                                        ViewModel.LoadPreviewFrame(demoPath);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error loading preview in background: {ex.Message}");
                                }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading demo video: {ex.Message}");
                        if (ViewModel != null)
                        {
                            ViewModel.StatusMessage = $"Error loading demo video: {ex.Message}";
                        }
                    }
                }
                else
                {
                    if (ViewModel != null)
                    {
                        ViewModel.StatusMessage = "Demo video not found. Use 'Import Video' to add videos.";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in MainWindow_Loaded: {ex.Message}\n{ex.StackTrace}");
                // Don't show message box here as it might cause issues during startup
            }
        }

        private void MainWindow_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent(typeof(MediaFile)))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            // Handle file drop from external sources
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var videoFiles = files.Where(f => 
                {
                    var ext = Path.GetExtension(f)?.ToLowerInvariant();
                    return ext != null && (ext == ".mp4" || ext == ".avi" || ext == ".mov" || 
                           ext == ".mkv" || ext == ".flv" || ext == ".wmv" || ext == ".webm");
                }).ToArray();

                if (videoFiles.Length > 0)
                {
                    ViewModel.ImportVideoFiles(videoFiles);
                }
                e.Handled = true;
            }
        }

        private void MediaLibraryListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is ListBox listBox)
            {
                var hitTest = System.Windows.Media.VisualTreeHelper.HitTest(listBox, e.GetPosition(listBox));
                if (hitTest != null)
                {
                    var listBoxItem = FindParent<ListBoxItem>(hitTest.VisualHit);
                    if (listBoxItem != null)
                    {
                        var mediaFile = listBoxItem.DataContext as MediaFile;
                        if (mediaFile != null)
                        {
                            DragDrop.DoDragDrop(listBox, mediaFile, DragDropEffects.Copy);
                        }
                    }
                }
            }
        }

        private static T? FindParent<T>(System.Windows.DependencyObject child) where T : System.Windows.DependencyObject
        {
            var parentObject = System.Windows.Media.VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            if (parentObject is T parent) return parent;
            return FindParent<T>(parentObject);
        }

        private void MediaLibraryListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is MediaFile mediaFile)
            {
                // Double-click adds video to timeline
                ViewModel.AddMediaFileToTimeline(mediaFile);
            }
        }

        private void MediaLibraryListBox_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(MediaFile)) || e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void MediaLibraryListBox_Drop(object sender, DragEventArgs e)
        {
            // Handle drop in media library (for external files)
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var videoFiles = files.Where(f => 
                {
                    var ext = Path.GetExtension(f)?.ToLowerInvariant();
                    return ext != null && (ext == ".mp4" || ext == ".avi" || ext == ".mov" || 
                           ext == ".mkv" || ext == ".flv" || ext == ".wmv" || ext == ".webm");
                }).ToArray();

                if (videoFiles.Length > 0)
                {
                    ViewModel.ImportVideoFiles(videoFiles);
                }
                e.Handled = true;
            }
        }

        private void TimelineArea_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(MediaFile)) || e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void TimelineArea_Drop(object sender, DragEventArgs e)
        {
            // Handle drop on timeline
            if (e.Data.GetDataPresent(typeof(MediaFile)))
            {
                var mediaFile = (MediaFile)e.Data.GetData(typeof(MediaFile));
                if (mediaFile != null)
                {
                    ViewModel.AddMediaFileToTimeline(mediaFile);
                }
                e.Handled = true;
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Handle external file drop on timeline
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var videoFiles = files.Where(f => 
                {
                    var ext = Path.GetExtension(f)?.ToLowerInvariant();
                    return ext != null && (ext == ".mp4" || ext == ".avi" || ext == ".mov" || 
                           ext == ".mkv" || ext == ".flv" || ext == ".wmv" || ext == ".webm");
                }).ToArray();

                if (videoFiles.Length > 0)
                {
                    try
                    {
                        // Import files first, then add to timeline
                        ViewModel.ImportVideoFiles(videoFiles);
                        // Add the first file to timeline
                        MediaFile? firstFile = null;
                        if (ViewModel.MediaFiles != null && videoFiles != null && videoFiles.Length > 0)
                        {
                            foreach (var m in ViewModel.MediaFiles)
                            {
                                if (m?.FilePath == videoFiles[0])
                                {
                                    firstFile = m;
                                    break;
                                }
                            }
                        }
                        if (firstFile != null)
                        {
                            ViewModel.AddMediaFileToTimeline(firstFile);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error handling timeline drop: {ex.Message}");
                        ViewModel.StatusMessage = $"Error adding video to timeline: {ex.Message}";
                    }
                }
                e.Handled = true;
            }
        }
    }
}

