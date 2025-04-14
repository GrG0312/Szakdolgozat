using Model.Interfaces;
using System.Threading.Tasks;

namespace Model.GameModel.Commands
{
    /// <summary>
    /// Command representing a 'Move' order. This has a start and end location for executing / undoing the order.
    /// </summary>
    /// <typeparam name="WorldPositionType"></typeparam>
    public class MoveCommand<WorldPositionType> : IUndoableCommand
    {
        public Phase ExecutingPhase { get; }
        public IMovable<WorldPositionType> TargetUnit { get; private set; }
        public WorldPositionType StartPosition { get; private set; }
        public WorldPositionType EndLocation { get; private set; }

        public MoveCommand(IMovable<WorldPositionType> unit, WorldPositionType end)
        {
            TargetUnit = unit;
            StartPosition = unit.Position;
            EndLocation = end;
            ExecutingPhase = Phase.Movement;
        }
        
        public Task Execute()
        {
            TargetUnit.MoveTo(EndLocation);
            return Task.CompletedTask;
        }

        public void Undo()
        {
            TargetUnit.Position = StartPosition;
            TargetUnit.CanMove = true;
        }

        public bool CanExecute(Phase current)
        {
            return TargetUnit.CanMove && ExecutingPhase == current;
        }
    }
}
