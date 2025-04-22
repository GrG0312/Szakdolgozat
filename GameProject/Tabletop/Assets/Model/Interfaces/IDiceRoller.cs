using System.Threading.Tasks;

namespace Model.Interfaces
{
    /// <summary>
    /// An object that can simulate D6 rolling.
    /// </summary>
    public interface IDiceRoller
    {
        /// <summary>
        /// Rolls <paramref name="n"/> D6s and returns the results in an <see cref="int"/> array.
        /// </summary>
        public Task<int[]> RollDice(int n);
    }
}
