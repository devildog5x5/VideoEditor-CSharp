using VideoEditor.Models;

namespace VideoEditor.Commands
{
    /// <summary>
    /// Command to resize a clip (change duration and trim points)
    /// </summary>
    public class ResizeClipCommand : ICommand
    {
        private readonly VideoClip _clip;
        private readonly double _oldDuration;
        private readonly double _newDuration;
        private readonly double _oldTrimEnd;
        private readonly double _newTrimEnd;
        private readonly bool _resizeFromStart;

        public string Description => $"Resize {_clip.Name} to {_newDuration:F2}s";

        public ResizeClipCommand(VideoClip clip, double oldDuration, double newDuration, 
            double oldTrimEnd, double newTrimEnd, bool resizeFromStart = false)
        {
            _clip = clip;
            _oldDuration = oldDuration;
            _newDuration = newDuration;
            _oldTrimEnd = oldTrimEnd;
            _newTrimEnd = newTrimEnd;
            _resizeFromStart = resizeFromStart;
        }

        public void Execute()
        {
            _clip.Duration = _newDuration;
            _clip.TrimEnd = _newTrimEnd;
            if (_resizeFromStart)
            {
                // When resizing from start, also adjust StartTime and TrimStart
                var delta = _newDuration - _oldDuration;
                _clip.StartTime -= delta;
                _clip.TrimStart -= delta;
            }
        }

        public void Undo()
        {
            _clip.Duration = _oldDuration;
            _clip.TrimEnd = _oldTrimEnd;
            if (_resizeFromStart)
            {
                var delta = _newDuration - _oldDuration;
                _clip.StartTime += delta;
                _clip.TrimStart += delta;
            }
        }
    }
}

