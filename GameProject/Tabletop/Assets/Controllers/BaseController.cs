using Unity.Netcode;
using UnityEngine;

namespace Controllers
{
    /// <summary>
    /// Base class for every controller. Ensures that the subclasses will be singleton, as long as their contructors are private.
    /// </summary>
    public abstract class BaseController<T> : NetworkBehaviour where T : BaseController<T>
    {
        protected BaseController() { }
        public static T Instance { get; protected set; }
    }
}
