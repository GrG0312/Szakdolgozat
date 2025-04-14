using System;

namespace Model.Interfaces
{
    public interface ISelectable
    {
        public event EventHandler<bool> Selected;
        public void SetSelected(bool status);
    }
    public interface ISelectable<IdType> : IOwned<IdType>, ISelectable { }
}
