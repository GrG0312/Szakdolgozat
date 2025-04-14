using Model.GameModel;
using System.Threading.Tasks;

namespace Model.Interfaces
{
    /// <summary>
    /// An interface for every command that can be given out ot units
    /// </summary>
    public interface IUnitCommand
    {
        public Phase ExecutingPhase { get; }
        public Task Execute();
        public bool CanExecute(Phase current);
    }
}
