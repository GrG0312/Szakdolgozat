namespace Model.Commands
{
    /// <summary>
    /// Base class for every command that can be given out ot units
    /// </summary>
    public abstract class Command
    {
        public abstract void Execute();
    }
}
