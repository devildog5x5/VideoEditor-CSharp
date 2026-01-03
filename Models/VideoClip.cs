using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VideoEditor.Models
{
    public class VideoClip : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string FilePath { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double StartTime { get; set; }
        public double Duration { get; set; }
        public double EndTime => StartTime + Duration;
        public double TrimStart { get; set; }
        public double TrimEnd { get; set; }
        public double Brightness { get; set; }
        public double Contrast { get; set; } = 100;
        public double Saturation { get; set; } = 100;
        public double Volume { get; set; } = 100;
        public double Speed { get; set; } = 100;
        public double FadeInDuration { get; set; }
        public double FadeOutDuration { get; set; }
        public bool IsCutFromOriginal { get; set; } // Indicates this clip was created by cutting
        public int CutIndex { get; set; } // Index if this is part of a series of cuts from same source
        
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

