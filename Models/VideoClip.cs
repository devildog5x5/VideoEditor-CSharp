namespace VideoEditor.Models
{
    public class VideoClip
    {
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
        public bool IsSelected { get; set; }
    }
}

