using CommunityToolkit.Mvvm.ComponentModel;
using VideoEditor.Models;

namespace VideoEditor.ViewModels
{
    public partial class PropertiesViewModel : ObservableObject
    {
        [ObservableProperty]
        private double brightness;

        [ObservableProperty]
        private double contrast = 100;

        [ObservableProperty]
        private double saturation = 100;

        [ObservableProperty]
        private double volume = 100;

        [ObservableProperty]
        private double speed = 100;

        private VideoClip? _currentClip;

        public void SetClip(VideoClip? clip)
        {
            _currentClip = clip;
            if (clip != null)
            {
                Brightness = clip.Brightness;
                Contrast = clip.Contrast;
                Saturation = clip.Saturation;
                Volume = clip.Volume;
                Speed = clip.Speed;
            }
        }

        partial void OnBrightnessChanged(double value)
        {
            if (_currentClip != null) _currentClip.Brightness = value;
        }

        partial void OnContrastChanged(double value)
        {
            if (_currentClip != null) _currentClip.Contrast = value;
        }

        partial void OnSaturationChanged(double value)
        {
            if (_currentClip != null) _currentClip.Saturation = value;
        }

        partial void OnVolumeChanged(double value)
        {
            if (_currentClip != null) _currentClip.Volume = value;
        }

        partial void OnSpeedChanged(double value)
        {
            if (_currentClip != null) _currentClip.Speed = value;
        }
    }
}

