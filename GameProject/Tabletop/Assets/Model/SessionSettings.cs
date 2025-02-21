using Model.UnityDependant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public struct SessionSettings
    {
        public bool IsLimited { get; }
        public int TurnLimit { get; }
        public List<PlayerObject> Players { get; }
    }
}
