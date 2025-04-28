using Model.GameModel;

namespace Model.Interfaces
{
    /// <summary>
    /// Represents an object which usability is limited. Either to a certain phase or other circumstances.
    /// </summary>
    public interface IUsable
    {
        /// <summary>
        /// Return if the object is usable in the <paramref name="p"/> Phase
        /// </summary>
        public bool IsUsable(Phase p);

        /// <summary>
        /// Reset the object's values to their original values at the beginning of a turn.
        /// </summary>
        public void ResetUse();
    }
}
