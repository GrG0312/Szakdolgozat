namespace Model.Interfaces
{
    public interface IUndoableCommand : IUnitCommand
    {
        public void Undo();
    }
}
