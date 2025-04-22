using Model.GameModel;
using System.Threading.Tasks;

namespace Model.Interfaces
{
    /// <summary>
    /// A command object which can be issues in-game.
    /// </summary>
    public interface IGameCommand
    {
        /// <summary>
        /// Which phase the command can be executed in
        /// </summary>
        public Phase ExecutingPhase { get; }

        /// <summary>
        /// Executes the command
        /// </summary>
        /// <returns>A Task that can be used to monitor it's execution's state</returns>
        public Task Execute();

        /// <summary>
        /// Determines if the command can be executed
        /// </summary>
        /// <param name="current">In which phase do we want to determine the result</param>
        public bool CanExecute(Phase current);
    }
}
