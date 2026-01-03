namespace VideoEditor.Commands
{
    /// <summary>
    /// Command interface for undo/redo system
    /// </summary>
    public interface ICommand
    {
        void Execute();
        void Undo();
        string Description { get; }
    }
}

