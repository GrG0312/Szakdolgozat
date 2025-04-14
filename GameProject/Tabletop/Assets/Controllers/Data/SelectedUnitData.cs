using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace Controllers.Data
{
    public struct SelectedUnitData : INetworkSerializable, IEquatable<SelectedUnitData>
    {
        public static SelectedUnitData Empty => new SelectedUnitData(-1, false, -1, -1);

        public int InstanceID;
        public int UnitIdentifier; // constants come from defines
        public bool CanUnitMove;
        public int WoundNow;

        public SelectedUnitData(int uid, bool cm, int wn, int iid)
        {
            InstanceID = iid;
            UnitIdentifier = uid;
            CanUnitMove = cm;
            WoundNow = wn;
        }

        public bool Equals(SelectedUnitData other)
        {
            return InstanceID == other.InstanceID;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref UnitIdentifier);
            serializer.SerializeValue(ref CanUnitMove);
            serializer.SerializeValue(ref WoundNow);
        }
    }
}
