using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VideoEditor.Models;
using VideoEditor.Services;

namespace VideoEditor.ViewModels
{
    public partial class TimelineViewModel : ObservableObject
    {
        private readonly TimelineService _timelineService;

        [ObservableProperty]
        private ObservableCollection<VideoClip> clips = new();

        [ObservableProperty]
        private VideoClip? selectedClip;

        [ObservableProperty]
        private double playheadPosition;

        [ObservableProperty]
        private bool isPlaying;

        public TimelineViewModel(TimelineService timelineService)
        {
            _timelineService = timelineService;
        }

        public void AddClip(VideoClip clip)
        {
            Clips.Add(clip);
        }

        public void Clear()
        {
            Clips.Clear();
            SelectedClip = null;
            PlayheadPosition = 0;
        }

        [RelayCommand]
        public void Play()
        {
            IsPlaying = true;
            // Start playback timer
        }

        [RelayCommand]
        public void Pause()
        {
            IsPlaying = false;
        }

        [RelayCommand]
        public void Stop()
        {
            IsPlaying = false;
            PlayheadPosition = 0;
        }

        public void CutAtPlayhead()
        {
            if (SelectedClip != null)
            {
                var cutTime = PlayheadPosition - SelectedClip.StartTime;
                if (cutTime > 0 && cutTime < SelectedClip.Duration)
                {
                    var newClip = new VideoClip
                    {
                        FilePath = SelectedClip.FilePath,
                        Name = SelectedClip.Name,
                        StartTime = PlayheadPosition,
                        Duration = SelectedClip.Duration - cutTime,
                        TrimStart = SelectedClip.TrimStart + cutTime,
                        TrimEnd = SelectedClip.TrimEnd
                    };
                    SelectedClip.Duration = cutTime;
                    SelectedClip.TrimEnd = SelectedClip.TrimStart + cutTime;
                    Clips.Add(newClip);
                }
            }
        }

        public void DeleteSelected()
        {
            if (SelectedClip != null)
            {
                Clips.Remove(SelectedClip);
                SelectedClip = null;
            }
        }
    }
}

