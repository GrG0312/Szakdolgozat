namespace Model.Interfaces
{
    /// <summary>
    /// A command that can be undone.
    /// </summary>
    public interface IUndoableCommand : IGameCommand
    {
        public void Undo();
    }
}
