using Model;
using Unity.Netcode;

namespace Controller
{
    /// <summary>
    ///     <para>
    ///         The Application's controller class.
    ///         The class is singleton. In order to access it, use <see cref="NetworkManager.Singleton"/>.
    ///     </para>
    ///     <para>
    ///         Since this class derives from <see cref="NetworkManager"/> class, this will not be destroyed when changing scenes.
    ///         Because of this, it is essential to NOT create an another instance of this GameObject in other scenes.
    ///     </para>
    /// </summary>
    public sealed partial class SessionController : NetworkManager
    {
        public SessionModel CurrentSession { get; private set; }

        private void StartSession(SessionSettings settings)
        {
            CurrentSession = new SessionModel(settings);
        }

        private void Awake()
        {
            
        }
    }
}
