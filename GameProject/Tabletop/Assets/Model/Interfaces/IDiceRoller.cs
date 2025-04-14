using System.Threading.Tasks;

namespace Model.Interfaces
{
    public interface IDiceRoller
    {
        public Task<int[]> RollDice(int n);
    }
}
