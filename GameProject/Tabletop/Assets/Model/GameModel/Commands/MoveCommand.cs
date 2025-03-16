using Model.Units.Interfaces;

namespace Model.Commands
{
    /// <summary>
    /// Command representing a 'Move' order. This has a start and end location for executing / undoing the order.
    /// </summary>
    /// <typeparam name="WorldPositionType"></typeparam>
    public class MoveCommand<WorldPositionType> : Command
    {
        public IMovable<WorldPositionType> TargetUnit { get; private set; }
        public WorldPositionType EndLocation { get; private set; }

        public MoveCommand(IMovable<WorldPositionType> unit, WorldPositionType end)
        {
            TargetUnit = unit;
            EndLocation = end;
        }
        
        public override void Execute()
        {
            TargetUnit.Move(EndLocation);
        }
    }
}
