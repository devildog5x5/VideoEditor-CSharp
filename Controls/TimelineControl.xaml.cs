using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VideoEditor.Models;
using VideoEditor.ViewModels;

namespace VideoEditor
{
    public partial class TimelineControl : UserControl
    {
        private const double PixelsPerSecond = 50.0;

        public TimelineControl()
        {
            InitializeComponent();
            this.Loaded += TimelineControl_Loaded;
            this.DataContextChanged += TimelineControl_DataContextChanged;
        }

        private void TimelineControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is TimelineViewModel oldVm)
            {
                oldVm.PropertyChanged -= TimelineViewModel_PropertyChanged;
            }
            if (e.NewValue is TimelineViewModel newVm)
            {
                newVm.PropertyChanged += TimelineViewModel_PropertyChanged;
                UpdatePlayhead();
            }
        }

        private void TimelineViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TimelineViewModel.PlayheadPosition))
            {
                UpdatePlayhead();
            }
        }

        private void TimelineControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateTimeRuler();
            UpdatePlayhead();
        }

        private void UpdatePlayhead()
        {
            if (DataContext is TimelineViewModel viewModel)
            {
                double x = viewModel.PlayheadPosition * PixelsPerSecond;
                
                // Update playhead line
                PlayheadLine.X1 = x;
                PlayheadLine.X2 = x;
                
                // Update playhead handle position
                Canvas.SetLeft(PlayheadHandle, x - 6); // Center the handle
                Canvas.SetLeft(PlayheadContainer, 0);
                
                // Update time label position
                Canvas.SetLeft(PlayheadTimeLabel, x - 20); // Center the label
                PlayheadTimeText.Text = $"{viewModel.PlayheadPosition:F2}s";
                
                // Highlight clip at playhead position
                HighlightClipAtPlayhead(viewModel);
            }
        }

        private void HighlightClipAtPlayhead(TimelineViewModel viewModel)
        {
            try
            {
                // Find clip at playhead position
                VideoClip? clipAtPlayhead = null;
                foreach (var clip in viewModel.Clips)
                {
                    if (viewModel.PlayheadPosition >= clip.StartTime && 
                        viewModel.PlayheadPosition < clip.EndTime)
                    {
                        clipAtPlayhead = clip;
                        break;
                    }
                }
                
                // Update all clips' visual state - highlight if selected OR at playhead
                foreach (var clip in viewModel.Clips)
                {
                    bool shouldHighlight = (clip == viewModel.SelectedClip) || (clip == clipAtPlayhead);
                    if (clip.IsSelected != shouldHighlight)
                    {
                        clip.IsSelected = shouldHighlight;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error highlighting clip: {ex.Message}");
            }
        }

        private void UpdateTimeRuler()
        {
            TimeRulerCanvas.Children.Clear();
            
            // Draw time markers every 5 seconds
            for (int i = 0; i <= 40; i += 5)
            {
                double x = i * PixelsPerSecond;
                var line = new System.Windows.Shapes.Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = 30,
                    Stroke = (Brush)FindResource("BorderColor"),
                    StrokeThickness = 1
                };
                TimeRulerCanvas.Children.Add(line);

                var label = new TextBlock
                {
                    Text = $"{i}s",
                    Foreground = (Brush)FindResource("TextColor"),
                    FontSize = 10,
                    Margin = new Thickness(x + 2, 2, 0, 0)
                };
                TimeRulerCanvas.Children.Add(label);
            }
        }

        private void TimelineCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(TimelineCanvas);
            var timeInSeconds = position.X / PixelsPerSecond;
            
            if (DataContext is TimelineViewModel viewModel)
            {
                viewModel.PlayheadPosition = Math.Max(0, timeInSeconds);
                // Preview will update automatically via PropertyChanged event
            }
        }

        private void Clip_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (sender is Border border && border.DataContext is VideoClip clip)
            {
                if (DataContext is TimelineViewModel viewModel)
                {
                    // Deselect all clips first
                    foreach (var c in viewModel.Clips)
                    {
                        c.IsSelected = false;
                    }
                    // Select this clip
                    clip.IsSelected = true;
                    viewModel.SelectedClip = clip;
                }
            }
        }

        private void Clip_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Opacity = 0.9;
            }
        }

        private void Clip_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Opacity = 1.0;
            }
        }
    }
}

