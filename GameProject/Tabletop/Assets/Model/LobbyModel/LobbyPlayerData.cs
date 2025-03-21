using System;

namespace Model.Lobby
{
    public class LobbyPlayerData<TypeId>
    {
        #region Readiness function
        private bool ready;
        public bool IsReady
        {
            get { return ready; }
            set
            {
                ready = value;
                ReadyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler? ReadyChanged;
        #endregion

        public TypeId ID { get; }
        public string Name { get; }

        public LobbyPlayerData(TypeId id, string name)
        {
            ID = id;
            Name = name;
            ready = false;
        }
    }
}
