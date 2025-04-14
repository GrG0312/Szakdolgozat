using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace Controllers.Data
{
    public struct WeaponInfoData : INetworkSerializable, IEquatable<WeaponInfoData>
    {

        public int OwnerInstanceID;
        public int WeaponIdentifier; // constants come from defines
        public int Count;
        public bool CanShoot;

        public WeaponInfoData(int wi, int c, bool cs, int iid)
        {
            OwnerInstanceID = iid;
            WeaponIdentifier = wi;
            Count = c;
            CanShoot = cs;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref WeaponIdentifier);
            serializer.SerializeValue(ref Count);
            serializer.SerializeValue(ref CanShoot);
        }

        public bool Equals(WeaponInfoData other)
        {
            return
                OwnerInstanceID == other.OwnerInstanceID &&
                WeaponIdentifier == other.WeaponIdentifier &&
                Count == other.Count &&
                CanShoot == other.CanShoot;
        }
    }
}
