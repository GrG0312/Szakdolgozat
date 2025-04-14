namespace Model.Interfaces
{
    public interface ICommandFactory
    {
        public IUnitCommand Produce<T>(params object[] args) where T : IUnitCommand;
    }
}
