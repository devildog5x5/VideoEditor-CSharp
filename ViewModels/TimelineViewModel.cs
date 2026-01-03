using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VideoEditor.Models;
using VideoEditor.Services;

namespace VideoEditor.ViewModels
{
    public partial class TimelineViewModel : ObservableObject
    {
        private readonly TimelineService _timelineService;
        private readonly Commands.UndoRedoManager _commandManager;
        private DispatcherTimer? _playbackTimer;
        private double _lastPlayheadUpdate;
        
        // Cut operation state
        private double? _cutStartPosition;
        private VideoClip? _cutStartClip;

        // Selection state
        private HashSet<VideoClip> _selectedClips = new();
        private VideoClip? _lastSelectedClip;
        private bool _isMultiSelectMode;

        // Drag/Resize state
        private bool _isDragging;
        private bool _isResizing;
        private VideoClip? _dragClip;
        private double _dragStartX;
        private double _dragStartTime;
        private bool _resizeFromStart;
        private double _resizeStartDuration;

        [ObservableProperty]
        private ObservableCollection<VideoClip> clips = new();

        [ObservableProperty]
        private VideoClip? selectedClip;

        [ObservableProperty]
        private ObservableCollection<VideoClip> selectedClips = new();

        [ObservableProperty]
        private double playheadPosition;

        [ObservableProperty]
        private bool isPlaying;
        
        [ObservableProperty]
        private bool hasPendingCut;

        [ObservableProperty]
        private double snapDistance = 0.1; // 100ms snap grid

        [ObservableProperty]
        private bool snapToGrid = true;

        [ObservableProperty]
        private bool snapToPlayhead = true;

        [ObservableProperty]
        private bool snapToClips = true;

        public Commands.UndoRedoManager CommandManager => _commandManager;

        public TimelineViewModel(TimelineService timelineService, Commands.UndoRedoManager? commandManager = null)
        {
            _timelineService = timelineService;
            _commandManager = commandManager ?? new Commands.UndoRedoManager();
            InitializePlaybackTimer();
        }

        private void InitializePlaybackTimer()
        {
            _playbackTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100) // Update 10 times per second
            };
            _playbackTimer.Tick += PlaybackTimer_Tick;
        }

        private void PlaybackTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (IsPlaying)
                {
                    double currentTime = DateTime.Now.TimeOfDay.TotalSeconds;
                    double deltaTime = currentTime - _lastPlayheadUpdate;
                    
                    if (_lastPlayheadUpdate == 0)
                    {
                        _lastPlayheadUpdate = currentTime;
                        return;
                    }

                    // Calculate total duration of all clips
                    double totalDuration = 0;
                    if (Clips.Count > 0)
                    {
                        foreach (var clip in Clips)
                        {
                            if (clip.EndTime > totalDuration)
                            {
                                totalDuration = clip.EndTime;
                            }
                        }
                    }

                    // Advance playhead
                    PlayheadPosition += deltaTime;

                    // Stop at end of timeline
                    if (PlayheadPosition >= totalDuration)
                    {
                        PlayheadPosition = totalDuration;
                        IsPlaying = false;
                        _playbackTimer?.Stop();
                    }

                    _lastPlayheadUpdate = currentTime;
                }
            }
            catch (Exception ex)
            {
                // Log error and stop playback to prevent crash
                System.Diagnostics.Debug.WriteLine($"Playback timer error: {ex.Message}");
                IsPlaying = false;
                _playbackTimer?.Stop();
            }
        }

        partial void OnIsPlayingChanged(bool value)
        {
            if (value)
            {
                // Starting playback
                _lastPlayheadUpdate = DateTime.Now.TimeOfDay.TotalSeconds;
                _playbackTimer?.Start();
            }
            else
            {
                // Stopping/pausing playback
                _playbackTimer?.Stop();
                _lastPlayheadUpdate = 0;
            }
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
            IsPlaying = false;
        }

        [RelayCommand]
        public void Play()
        {
            if (Clips.Count == 0)
            {
                return;
            }
            IsPlaying = true;
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
            if (SelectedClip == null)
            {
                // Clear any pending cut if no clip selected
                ClearPendingCut();
                return;
            }

            var clip = SelectedClip;
            var cutTime = PlayheadPosition - clip.StartTime;

            // Validate playhead is within the clip
            if (cutTime <= 0 || cutTime >= clip.Duration)
            {
                ClearPendingCut();
                return;
            }

            // If we have a pending cut (first cut point set)
            if (_cutStartPosition.HasValue && _cutStartClip != null)
            {
                // Second cut: remove section between start and end
                PerformRangeCut(_cutStartClip, _cutStartPosition.Value, clip, PlayheadPosition);
                ClearPendingCut();
            }
            else
            {
                // First cut: mark the start position
                _cutStartPosition = PlayheadPosition;
                _cutStartClip = clip;
                HasPendingCut = true;
            }
        }

        private void PerformRangeCut(VideoClip startClip, double startTime, VideoClip endClip, double endTime)
        {
            // If both cuts are on the same clip
            if (startClip == endClip)
            {
                var clip = startClip;
                var startOffset = startTime - clip.StartTime;
                var endOffset = endTime - clip.StartTime;

                // Validate range
                if (startOffset >= endOffset || startOffset < 0 || endOffset > clip.Duration)
                {
                    return;
                }

                // Case 1: Cut is at the beginning of the clip
                if (startOffset == 0)
                {
                    // Just trim the start of the clip
                    clip.Duration = clip.Duration - endOffset;
                    clip.StartTime = endTime;
                    clip.TrimStart = clip.TrimStart + endOffset;
                    clip.IsCutFromOriginal = true;
                }
                // Case 2: Cut is at the end of the clip
                else if (endOffset >= clip.Duration)
                {
                    // Just trim the end
                    clip.Duration = startOffset;
                    clip.TrimEnd = clip.TrimStart + startOffset;
                    clip.IsCutFromOriginal = true;
                }
                // Case 3: Cut is in the middle - split into two clips
                else
                {
                    // Create second part (after the cut)
                    var secondPart = new VideoClip
                    {
                        FilePath = clip.FilePath,
                        Name = clip.Name,
                        StartTime = endTime,
                        Duration = clip.Duration - endOffset,
                        TrimStart = clip.TrimStart + endOffset,
                        TrimEnd = clip.TrimEnd,
                        IsCutFromOriginal = true
                    };

                    // Update first part (before the cut)
                    clip.Duration = startOffset;
                    clip.TrimEnd = clip.TrimStart + startOffset;
                    clip.IsCutFromOriginal = true;

                    // Insert second part after the first
                    int insertIndex = Clips.IndexOf(clip) + 1;
                    Clips.Insert(insertIndex, secondPart);
                }
            }
            else
            {
                // Cuts span multiple clips - remove everything between start and end
                var clipsToRemove = new List<VideoClip>();
                var clipsToModify = new List<(VideoClip clip, bool trimStart, bool trimEnd, double trimAmount)>();

                foreach (var c in Clips)
                {
                    // Clip is completely within the cut range
                    if (c.StartTime >= startTime && c.EndTime <= endTime)
                    {
                        clipsToRemove.Add(c);
                    }
                    // Clip starts before but ends within range - trim end
                    else if (c.StartTime < startTime && c.EndTime > startTime && c.EndTime <= endTime)
                    {
                        var trimAmount = c.EndTime - startTime;
                        clipsToModify.Add((c, false, true, trimAmount));
                    }
                    // Clip starts within range but ends after - trim start
                    else if (c.StartTime >= startTime && c.StartTime < endTime && c.EndTime > endTime)
                    {
                        var trimAmount = endTime - c.StartTime;
                        clipsToModify.Add((c, true, false, trimAmount));
                    }
                    // Clip spans the entire range - split into two
                    else if (c.StartTime < startTime && c.EndTime > endTime)
                    {
                        // Create second part
                        var secondPart = new VideoClip
                        {
                            FilePath = c.FilePath,
                            Name = c.Name,
                            StartTime = endTime,
                            Duration = c.Duration - (endTime - c.StartTime),
                            TrimStart = c.TrimStart + (endTime - c.StartTime),
                            TrimEnd = c.TrimEnd,
                            IsCutFromOriginal = true
                        };

                        // Update first part
                        c.Duration = startTime - c.StartTime;
                        c.TrimEnd = c.TrimStart + (startTime - c.StartTime);
                        c.IsCutFromOriginal = true;

                        int insertIndex = Clips.IndexOf(c) + 1;
                        Clips.Insert(insertIndex, secondPart);
                    }
                }

                // Apply modifications
                foreach (var (clip, trimStart, trimEnd, trimAmount) in clipsToModify)
                {
                    if (trimStart)
                    {
                        clip.StartTime = endTime;
                        clip.Duration -= trimAmount;
                        clip.TrimStart += trimAmount;
                    }
                    if (trimEnd)
                    {
                        clip.Duration -= trimAmount;
                        clip.TrimEnd -= trimAmount;
                    }
                    clip.IsCutFromOriginal = true;
                }

                // Remove clips that are completely within the range
                foreach (var clipToRemove in clipsToRemove)
                {
                    Clips.Remove(clipToRemove);
                }
            }
        }

        private void ClearPendingCut()
        {
            _cutStartPosition = null;
            _cutStartClip = null;
            HasPendingCut = false;
        }
        
        partial void OnSelectedClipChanged(VideoClip? value)
        {
            // Clear pending cut if a different clip is selected (optional behavior)
            // Uncomment if you want to reset cut state when selecting a different clip
            // if (value != _cutStartClip)
            // {
            //     ClearPendingCut();
            // }
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


