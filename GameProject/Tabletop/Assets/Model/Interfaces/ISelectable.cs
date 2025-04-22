using System;

namespace Model.Interfaces
{
    /// <summary>
    /// A selectable object, that is upon selection send a <see cref="Selected"/> event
    /// </summary>
    public interface ISelectable
    {
        /// <summary>
        /// Notifies about the unit's selected status
        /// </summary>
        public event EventHandler<bool> Selected;

        /// <summary>
        /// Set if the unit is selected or not
        /// </summary>
        public void SetSelected(bool status);
    }

    /// <summary>
    /// An expanded version of the <see cref="ISelectable"/> interface which incorporates the <see cref="IOwned{IdType}"/> interface, and with that, access to the Owner of the object
    /// </summary>
    public interface ISelectable<IdType> : IOwned<IdType>, ISelectable { }
}
