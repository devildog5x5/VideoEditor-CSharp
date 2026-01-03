using System.Collections.Generic;
using System.Linq;

namespace VideoEditor.Commands
{
    /// <summary>
    /// Manages undo/redo command stack
    /// </summary>
    public class UndoRedoManager
    {
        private readonly Stack<ICommand> _undoStack = new();
        private readonly Stack<ICommand> _redoStack = new();
        private const int MaxStackSize = 100;

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
        public string UndoDescription => _undoStack.FirstOrDefault()?.Description ?? "";
        public string RedoDescription => _redoStack.FirstOrDefault()?.Description ?? "";

        public void Execute(ICommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear(); // Clear redo stack when new command is executed
            
            // Limit stack size
            if (_undoStack.Count > MaxStackSize)
            {
                var temp = new Stack<ICommand>();
                for (int i = 0; i < MaxStackSize - 1; i++)
                {
                    temp.Push(_undoStack.Pop());
                }
                _undoStack.Clear();
                while (temp.Count > 0)
                {
                    _undoStack.Push(temp.Pop());
                }
            }
        }

        public void Undo()
        {
            if (_undoStack.Count > 0)
            {
                var command = _undoStack.Pop();
                command.Undo();
                _redoStack.Push(command);
            }
        }

        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                var command = _redoStack.Pop();
                command.Execute();
                _undoStack.Push(command);
            }
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }
}

