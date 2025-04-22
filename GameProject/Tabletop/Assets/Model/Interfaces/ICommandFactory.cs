namespace Model.Interfaces
{
    /// <summary>
    /// An object which can be used to create commands for the model, based on the provided parameters
    /// </summary>
    public interface ICommandFactory
    {
        /// <summary>
        /// Creates and returns a <see cref="IGameCommand"/>, which exact type is specified by <typeparamref name="T"/>, 
        /// and the necessary parameters will be provided in <paramref name="args"/>
        /// </summary>
        public IGameCommand Produce<T>(params object[] args) where T : IGameCommand;
    }
}
