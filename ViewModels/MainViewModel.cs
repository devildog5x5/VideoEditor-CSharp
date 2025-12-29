using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
            if (value != null)
            {
                var clip = new VideoClip
                {
                    FilePath = value.FilePath,
                    Name = value.FileName,
                    StartTime = TimelineViewModel.PlayheadPosition,
                    Duration = value.Duration
                };
                TimelineViewModel.AddClip(clip);
            }
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

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedTheme) && SelectedTheme != null)
                {
                    _themeManager.ApplyTheme(SelectedTheme);
                }
            };
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
        private void ImportVideo()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Video Files (*.mp4;*.avi;*.mov;*.mkv;*.flv;*.wmv;*.webm)|*.mp4;*.avi;*.mov;*.mkv;*.flv;*.wmv;*.webm|All Files (*.*)|*.*",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                foreach (var filename in dialog.FileNames)
                {
                    var mediaFile = new MediaFile
                    {
                        FilePath = filename,
                        FileName = Path.GetFileName(filename)
                    };
                    MediaFiles.Add(mediaFile);
                }
                StatusMessage = $"Imported {dialog.FileNames.Length} video(s)";
            }
        }

        [RelayCommand]
        private void Play()
        {
            TimelineViewModel.Play();
            StatusMessage = "Playing...";
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
        }

        [RelayCommand]
        private void Cut()
        {
            TimelineViewModel.CutAtPlayhead();
            StatusMessage = "Clip cut";
        }

        [RelayCommand]
        private void Delete()
        {
            TimelineViewModel.DeleteSelected();
            StatusMessage = "Clip deleted";
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

