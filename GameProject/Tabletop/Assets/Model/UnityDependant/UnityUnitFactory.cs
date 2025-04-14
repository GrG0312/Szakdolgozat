using Model.Units;
using Model.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Model.UnityDependant
{
    public class UnityUnitFactory : IUnitFactory<ulong>
    {
        private readonly Dictionary<Side, Vector3> spawnPoints;
        private UnitModel unitPrefab;

        public UnityUnitFactory(Dictionary<Side, GameObject> sp, UnitModel prefab)
        {
            spawnPoints = new Dictionary<Side, Vector3>();
            unitPrefab = prefab;
            foreach (KeyValuePair<Side, GameObject> kvp in sp)
            {
                NavMesh.SamplePosition(kvp.Value.transform.position, out NavMeshHit hit, 500, 1);
                spawnPoints.Add(kvp.Key, hit.position);
            }
        }

        public IUnit Produce(ulong owner, UnitIdentifier identity, Side side)
        {
            UnitModel unit = Object.Instantiate(unitPrefab);
            unit.NetworkObject.Spawn(true);
            unit.SetupData(owner, identity, Defines.UnitValues[identity], spawnPoints[side]);
            return unit;
        }
    }
}
