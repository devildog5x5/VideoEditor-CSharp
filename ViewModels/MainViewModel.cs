using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using VideoEditor.Models;
using VideoEditor.Services;

namespace VideoEditor.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ThemeManager _themeManager;
        private readonly ProjectManager _projectManager;
        private readonly VideoProcessingService _videoService;
        private readonly ExportService _exportService;
        private readonly TimelineService _timelineService;
        private double _lastPreviewTime = -1;

        [ObservableProperty]
        private ObservableCollection<MediaFile> mediaFiles = new();

        [ObservableProperty]
        private MediaFile? selectedMediaFile;

        [ObservableProperty]
        private string statusMessage = "Ready";

        [ObservableProperty]
        private string progressMessage = "";

        [ObservableProperty]
        private BitmapSource? previewFrame;

        [ObservableProperty]
        private bool showPreviewPlaceholder = true;

        [ObservableProperty]
        private ObservableCollection<Theme> themes;

        [ObservableProperty]
        private Theme? selectedTheme;

        public TimelineViewModel TimelineViewModel { get; }
        public PropertiesViewModel PropertiesViewModel { get; }

        partial void OnSelectedMediaFileChanged(MediaFile? value)
        {
            // Only load preview from media library if there are no clips in timeline
            // Otherwise, timeline should control the preview
            if (TimelineViewModel.Clips.Count == 0)
            {
                // Load preview frame when media file is selected
                if (value != null && !string.IsNullOrEmpty(value.FilePath) && File.Exists(value.FilePath))
                {
                    StatusMessage = $"Loading preview for: {value.FileName}...";
                    // Hide placeholder immediately when video is selected
                    ShowPreviewPlaceholder = false;
                    // Try to load preview frame
                    LoadPreviewFrame(value.FilePath);
                }
                else
                {
                    PreviewFrame = null;
                    ShowPreviewPlaceholder = true;
                }
            }
            // If timeline has clips, don't change preview when selecting from library
        }

        public void LoadPreviewFrame(string videoPath)
        {
            LoadPreviewFrameAtTime(videoPath, TimeSpan.FromSeconds(0));
        }

        public void LoadPreviewFrameAtTime(string videoPath, TimeSpan time)
        {
            // Run frame extraction on background thread to avoid blocking UI
            Task.Run(() =>
            {
                try
                {
                    if (string.IsNullOrEmpty(videoPath) || !File.Exists(videoPath))
                    {
                        Application.Current?.Dispatcher.Invoke(() =>
                        {
                            ShowPreviewPlaceholder = true;
                            PreviewFrame = null;
                        });
                        return;
                    }

                    // Check FFmpeg availability first
                    if (!_videoService.IsFFmpegAvailable)
                    {
                        Application.Current?.Dispatcher.Invoke(() =>
                        {
                            PreviewFrame = null;
                            ShowPreviewPlaceholder = true;
                            StatusMessage = "Preview unavailable: FFmpeg is not installed. Install FFmpeg to see preview frames.";
                        });
                        return;
                    }

                    // Extract frame at specified time
                    var tempFramePath = Path.Combine(Path.GetTempPath(), $"video_preview_{Guid.NewGuid()}.png");
                    
                    try
                    {
                        _videoService.ExtractFrame(videoPath, tempFramePath, time);
                    }
                    catch (InvalidOperationException ex)
                    {
                        // FFmpeg not available
                        Application.Current?.Dispatcher.Invoke(() =>
                        {
                            PreviewFrame = null;
                            ShowPreviewPlaceholder = true;
                            StatusMessage = $"Preview unavailable: {ex.Message}";
                        });
                        System.Diagnostics.Debug.WriteLine($"FFmpeg error: {ex.Message}");
                        return;
                    }
                    catch (Exception ex)
                    {
                        // Other FFmpeg errors
                        Application.Current?.Dispatcher.Invoke(() =>
                        {
                            PreviewFrame = null;
                            ShowPreviewPlaceholder = true;
                            StatusMessage = $"Preview error: {ex.Message}";
                        });
                        System.Diagnostics.Debug.WriteLine($"FFmpeg extraction error: {ex.Message}");
                        return;
                    }
                    
                    if (File.Exists(tempFramePath))
                    {
                        try
                        {
                            var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(tempFramePath, UriKind.Absolute);
                            bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            bitmap.Freeze();
                            
                            // Update UI on dispatcher thread
                            Application.Current?.Dispatcher.Invoke(() =>
                            {
                                PreviewFrame = bitmap;
                                ShowPreviewPlaceholder = false;
                                StatusMessage = $"Preview at {time.TotalSeconds:F1}s";
                            });
                            
                            // Clean up temp file after a delay
                            Task.Delay(5000).ContinueWith(_ => 
                            {
                                try { File.Delete(tempFramePath); } catch { }
                            });
                        }
                        catch (Exception ex)
                        {
                            Application.Current?.Dispatcher.Invoke(() =>
                            {
                                StatusMessage = $"Preview load error: {ex.Message}";
                                ShowPreviewPlaceholder = true;
                                PreviewFrame = null;
                            });
                        }
                    }
                    else
                    {
                        Application.Current?.Dispatcher.Invoke(() =>
                        {
                            ShowPreviewPlaceholder = true;
                            PreviewFrame = null;
                            StatusMessage = "Could not extract preview frame";
                        });
                    }
                }
                catch (Exception ex)
                {
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        StatusMessage = $"Preview error: {ex.Message}";
                        ShowPreviewPlaceholder = true;
                        PreviewFrame = null;
                    });
                }
            });
        }

        public MainViewModel(
            ThemeManager themeManager,
            ProjectManager projectManager,
            VideoProcessingService videoService,
            ExportService exportService,
            TimelineService timelineService,
            TimelineViewModel timelineViewModel,
            PropertiesViewModel propertiesViewModel)
        {
            _themeManager = themeManager;
            _projectManager = projectManager;
            _videoService = videoService;
            _exportService = exportService;
            _timelineService = timelineService;

            Themes = new ObservableCollection<Theme>(_themeManager.Themes);
            SelectedTheme = _themeManager.CurrentTheme;

            TimelineViewModel = timelineViewModel;
            PropertiesViewModel = propertiesViewModel;

            // Subscribe to timeline changes
            TimelineViewModel.PropertyChanged += TimelineViewModel_PropertyChanged;
            TimelineViewModel.Clips.CollectionChanged += (s, e) => UpdatePreviewFromTimeline();

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedTheme) && SelectedTheme != null)
                {
                    _themeManager.ApplyTheme(SelectedTheme);
                }
            };
        }

        private void TimelineViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TimelineViewModel.PlayheadPosition))
            {
                UpdatePreviewFromTimeline();
            }
            else if (e.PropertyName == nameof(TimelineViewModel.SelectedClip))
            {
                UpdatePreviewFromTimeline();
            }
        }

        private void UpdatePreviewFromTimeline()
        {
            try
            {
                // If we have clips in the timeline, show preview based on playhead or selected clip
                if (TimelineViewModel.Clips.Count > 0)
                {
                    // Always find the clip at the current playhead position (most important)
                    VideoClip? clipToShow = null;
                    foreach (var clip in TimelineViewModel.Clips)
                    {
                        if (TimelineViewModel.PlayheadPosition >= clip.StartTime && 
                            TimelineViewModel.PlayheadPosition < clip.EndTime)
                        {
                            clipToShow = clip;
                            break;
                        }
                    }
                    
                    // If no clip at playhead, use selected clip
                    if (clipToShow == null)
                    {
                        clipToShow = TimelineViewModel.SelectedClip;
                    }

                    if (clipToShow != null && File.Exists(clipToShow.FilePath))
                    {
                        // Calculate the time offset within the clip (time from start of clip on timeline)
                        double timeInClip = TimelineViewModel.PlayheadPosition - clipToShow.StartTime;
                        
                        // Clamp timeInClip to valid range
                        if (timeInClip < 0) timeInClip = 0;
                        if (timeInClip > clipToShow.Duration) timeInClip = clipToShow.Duration;
                        
                        // Calculate actual time in source video
                        // TrimStart is the offset in the source video where this clip starts
                        // timeInClip is how far into this clip we are on the timeline
                        double actualVideoTime = clipToShow.TrimStart + timeInClip;
                        
                        // Ensure we don't exceed TrimEnd
                        if (clipToShow.TrimEnd > 0 && actualVideoTime > clipToShow.TrimEnd)
                        {
                            actualVideoTime = clipToShow.TrimEnd;
                        }
                        
                        // Always update preview immediately when not playing (user interaction)
                        // During playback, throttle updates to every 0.3 seconds for smoother experience
                        bool shouldUpdate = !TimelineViewModel.IsPlaying || 
                                           Math.Abs(timeInClip - _lastPreviewTime) > 0.3 ||
                                           _lastPreviewTime < 0;
                        
                        if (shouldUpdate)
                        {
                            LoadPreviewFrameAtTime(clipToShow.FilePath, TimeSpan.FromSeconds(Math.Max(0, actualVideoTime)));
                            _lastPreviewTime = timeInClip;
                            StatusMessage = $"Preview: {clipToShow.Name} @ {actualVideoTime:F2}s (Timeline: {TimelineViewModel.PlayheadPosition:F2}s)";
                        }
                    }
                    else
                    {
                        // No clip at playhead position, clear preview
                        PreviewFrame = null;
                        ShowPreviewPlaceholder = true;
                        _lastPreviewTime = -1;
                        StatusMessage = $"Timeline: {TimelineViewModel.PlayheadPosition:F2}s (No clip at this position)";
                    }
                }
                else
                {
                    // No clips in timeline, show preview of selected media file if any
                    if (SelectedMediaFile != null && !string.IsNullOrEmpty(SelectedMediaFile.FilePath))
                    {
                        LoadPreviewFrame(SelectedMediaFile.FilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating preview from timeline: {ex.Message}");
                StatusMessage = $"Preview update error: {ex.Message}";
            }
        }

        // Pre-load demo video if it exists - will be called after window loads

        private void LoadDemoVideo()
        {
            try
            {
                // Try absolute path first (most reliable)
                var absolutePath = @"C:\Users\rober\Documents\GitHub\VideoEditor-CSharp\Samples\sample_video.mp4";
                string? demoVideoPath = null;

                if (File.Exists(absolutePath))
                {
                    demoVideoPath = absolutePath;
                }
                else
                {
                    // Fallback: Try relative paths
                    var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    var exeDirectory = Path.GetDirectoryName(assemblyLocation);
                    if (string.IsNullOrEmpty(exeDirectory))
                    {
                        exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    }

                    // Go up 4 levels from bin/Release/net8.0-windows to project root
                    var projectRoot = Path.GetFullPath(Path.Combine(exeDirectory, "..", "..", "..", ".."));
                    var relativePath = Path.Combine(projectRoot, "Samples", "sample_video.mp4");
                    
                    if (File.Exists(relativePath))
                    {
                        demoVideoPath = relativePath;
                    }
                }

                if (!string.IsNullOrEmpty(demoVideoPath) && File.Exists(demoVideoPath))
                {
                    var mediaFile = new MediaFile
                    {
                        FilePath = demoVideoPath,
                        FileName = Path.GetFileName(demoVideoPath)
                    };
                    // Load video duration
                    try
                    {
                        var duration = _videoService.GetVideoDuration(demoVideoPath);
                        mediaFile.Duration = duration.TotalSeconds;
                    }
                    catch
                    {
                        mediaFile.Duration = 0;
                    }
                    
                    // Ensure we're on UI thread
                    if (System.Windows.Application.Current?.Dispatcher.CheckAccess() == true)
                    {
                        MediaFiles.Add(mediaFile);
                        SelectedMediaFile = mediaFile;
                        StatusMessage = $"Demo video loaded: {Path.GetFileName(demoVideoPath)}";
                    }
                    else
                    {
                        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                        {
                            MediaFiles.Add(mediaFile);
                            SelectedMediaFile = mediaFile;
                            StatusMessage = $"Demo video loaded: {Path.GetFileName(demoVideoPath)}";
                        });
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"Demo video added to MediaFiles. Count: {MediaFiles.Count}");
                }
                else
                {
                    StatusMessage = "Demo video not found. Use 'Import Video' to add videos.";
                    System.Diagnostics.Debug.WriteLine("Demo video not found in any expected location");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Could not load demo video: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Could not load demo video: {ex.Message}");
            }
        }

        [RelayCommand]
        private void NewProject()
        {
            _projectManager.NewProject();
            MediaFiles.Clear();
            TimelineViewModel.Clear();
            StatusMessage = "New project created";
        }

        [RelayCommand]
        private void OpenProject()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Video Editor Projects (*.vep)|*.vep|All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _projectManager.LoadProject(dialog.FileName);
                    StatusMessage = $"Project loaded: {Path.GetFileName(dialog.FileName)}";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error loading project: {ex.Message}";
                }
            }
        }

        [RelayCommand]
        private void SaveProject()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Video Editor Projects (*.vep)|*.vep|All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _projectManager.SaveProject(dialog.FileName);
                    StatusMessage = $"Project saved: {Path.GetFileName(dialog.FileName)}";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error saving project: {ex.Message}";
                }
            }
        }

        [RelayCommand]
        private void AddSelectedToTimeline()
        {
            if (SelectedMediaFile != null)
            {
                AddMediaFileToTimeline(SelectedMediaFile);
            }
            else
            {
                StatusMessage = "Please select a video from the Media Library first";
            }
        }

        [RelayCommand]
        private void ImportVideo()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Video Files (*.mp4;*.avi;*.mov;*.mkv;*.flv;*.wmv;*.webm)|*.mp4;*.avi;*.mov;*.mkv;*.flv;*.wmv;*.webm|All Files (*.*)|*.*",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                ImportVideoFiles(dialog.FileNames);
            }
        }

        public void ImportVideoFiles(string[] filePaths)
        {
            if (filePaths == null || filePaths.Length == 0)
                return;

            int importedCount = 0;
            foreach (var filePath in filePaths)
            {
                if (File.Exists(filePath))
                {
                    var extension = Path.GetExtension(filePath)?.ToLowerInvariant();
                    if (extension != null && (extension == ".mp4" || extension == ".avi" || extension == ".mov" || 
                        extension == ".mkv" || extension == ".flv" || extension == ".wmv" || extension == ".webm"))
                    {
                        // Check if file is already imported
                        if (!MediaFiles.Any(m => m.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase)))
                {
                    var mediaFile = new MediaFile
                    {
                                FilePath = filePath,
                                FileName = Path.GetFileName(filePath)
                            };
                            // Load video duration (requires FFmpeg)
                            try
                            {
                                if (_videoService.IsFFmpegAvailable)
                                {
                                    var duration = _videoService.GetVideoDuration(filePath);
                                    mediaFile.Duration = duration.TotalSeconds;
                                }
                                else
                                {
                                    mediaFile.Duration = 0;
                                }
                            }
                            catch
                            {
                                mediaFile.Duration = 0;
                            }
                            
                            // Ensure we're on UI thread
                            if (System.Windows.Application.Current?.Dispatcher.CheckAccess() == true)
                            {
                                MediaFiles.Add(mediaFile);
                            }
                            else
                            {
                                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                                {
                                    MediaFiles.Add(mediaFile);
                                });
                            }
                            importedCount++;
                        }
                    }
                }
            }

            if (importedCount > 0)
            {
                StatusMessage = $"Imported {importedCount} video(s)";
                // Select the first imported file
                if (MediaFiles.Count > 0 && SelectedMediaFile == null)
                {
                    if (System.Windows.Application.Current?.Dispatcher.CheckAccess() == true)
                    {
                        SelectedMediaFile = MediaFiles[0];
                    }
                    else
                    {
                        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                        {
                            SelectedMediaFile = MediaFiles[0];
                        });
                    }
                }
            }
            else
            {
                StatusMessage = "No new video files to import";
            }
        }

        public void AddMediaFileToTimeline(MediaFile mediaFile)
        {
            if (mediaFile == null) return;

            // Ensure file is in media library
            if (!MediaFiles.Contains(mediaFile))
            {
                    MediaFiles.Add(mediaFile);
                }

            // Load duration if not already loaded (requires FFmpeg)
            if (mediaFile.Duration == 0)
            {
                try
                {
                    if (_videoService.IsFFmpegAvailable)
                    {
                        var duration = _videoService.GetVideoDuration(mediaFile.FilePath);
                        mediaFile.Duration = duration.TotalSeconds;
                    }
                    else
                    {
                        mediaFile.Duration = 30; // Default duration if FFmpeg not available
                    }
                }
                catch
                {
                    mediaFile.Duration = 30; // Default duration if can't read
                }
            }

            // Add to timeline
            double clipDuration = mediaFile.Duration > 0 ? mediaFile.Duration : 30.0; // Default 30 seconds if duration unknown
            var clip = new VideoClip
            {
                FilePath = mediaFile.FilePath,
                Name = mediaFile.FileName,
                StartTime = TimelineViewModel.PlayheadPosition,
                Duration = clipDuration,
                TrimStart = 0, // Start from beginning of source video
                TrimEnd = clipDuration // End at full duration (can be trimmed later)
            };
            TimelineViewModel.AddClip(clip);
            
            // Update preview to show frame at the playhead position
            TimelineViewModel.SelectedClip = clip;
            UpdatePreviewFromTimeline();
            
            // Select the media file to trigger preview
            SelectedMediaFile = mediaFile;
            StatusMessage = $"Added {mediaFile.FileName} to timeline";
        }

        [RelayCommand]
        private void Play()
        {
            if (TimelineViewModel.Clips.Count == 0)
            {
                StatusMessage = "Please add a video to the timeline first";
                return;
            }

            TimelineViewModel.Play();
            StatusMessage = "Playing...";
            // Preview will update automatically as playhead moves
        }

        [RelayCommand]
        private void Pause()
        {
            TimelineViewModel.Pause();
            StatusMessage = "Paused";
        }

        [RelayCommand]
        private void Stop()
        {
            TimelineViewModel.Stop();
            StatusMessage = "Stopped";
            // Preview will update to show frame at start
            _lastPreviewTime = -1;
            UpdatePreviewFromTimeline();
        }

        [RelayCommand]
        private void Cut()
        {
            if (TimelineViewModel.SelectedClip == null)
            {
                StatusMessage = "⚠️ Please select a clip first, then move the playhead to where you want to start the cut, then click Cut";
                return;
            }
            
            var clip = TimelineViewModel.SelectedClip;
            var cutTime = TimelineViewModel.PlayheadPosition - clip.StartTime;
            
            if (cutTime <= 0 || cutTime >= clip.Duration)
            {
                StatusMessage = $"⚠️ Playhead must be inside the selected clip. Clip range: {clip.StartTime:F2}s - {clip.EndTime:F2}s, Playhead: {TimelineViewModel.PlayheadPosition:F2}s";
                return;
            }
            
            // Check if this is the first or second cut
            bool wasPending = TimelineViewModel.HasPendingCut;
            TimelineViewModel.CutAtPlayhead();
            
            if (!wasPending)
            {
                // First cut point set
                StatusMessage = $"✓ Cut start marked at {TimelineViewModel.PlayheadPosition:F2}s. Move the playhead to where you want the cut to end, then click Cut again to remove the section.";
            }
            else
            {
                // Second cut point - section removed
                StatusMessage = $"✓ Section removed from {TimelineViewModel.PlayheadPosition:F2}s. The cut range has been deleted.";
            }
        }

        [RelayCommand]
        private void Delete()
        {
            if (TimelineViewModel.SelectedClip == null)
            {
                StatusMessage = "⚠️ Please select a clip to delete. Click on a clip in the timeline first.";
                return;
            }
            
            var clipName = TimelineViewModel.SelectedClip.Name;
            var clipDuration = TimelineViewModel.SelectedClip.Duration;
            TimelineViewModel.DeleteSelected();
            StatusMessage = $"✓ Deleted clip '{clipName}' ({clipDuration:F2}s) from timeline";
        }

        [RelayCommand]
        private void Export()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "MP4 Files (*.mp4)|*.mp4|AVI Files (*.avi)|*.avi|MOV Files (*.mov)|*.mov",
                FileName = "output.mp4"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    ProgressMessage = "Exporting...";
                    var clipsList = new List<VideoClip>(TimelineViewModel.Clips);
                    _exportService.Export(clipsList, dialog.FileName, progress =>
                    {
                        ProgressMessage = $"Exporting... {progress}%";
                    });
                    StatusMessage = $"Video exported: {Path.GetFileName(dialog.FileName)}";
                    ProgressMessage = "";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Export error: {ex.Message}";
                    ProgressMessage = "";
                }
            }
        }
    }
}

