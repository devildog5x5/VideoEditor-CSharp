using VideoEditor.Models;

namespace VideoEditor.Commands
{
    /// <summary>
    /// Command to move a clip on the timeline
    /// </summary>
    public class MoveClipCommand : ICommand
    {
        private readonly VideoClip _clip;
        private readonly double _oldStartTime;
        private readonly double _newStartTime;

        public string Description => $"Move {_clip.Name} to {_newStartTime:F2}s";

        public MoveClipCommand(VideoClip clip, double oldStartTime, double newStartTime)
        {
            _clip = clip;
            _oldStartTime = oldStartTime;
            _newStartTime = newStartTime;
        }

        public void Execute()
        {
            _clip.StartTime = _newStartTime;
        }

        public void Undo()
        {
            _clip.StartTime = _oldStartTime;
        }
    }
}

