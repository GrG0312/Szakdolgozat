using Model;
using Model.Interfaces;
using Model.Units;
using Model.UnityDependant;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

namespace Tests
{
    public class TestUnitFactory : IUnitFactory<ulong>
    {
        private readonly Dictionary<Side, Vector3> spawnPoints;
        private UnitModel unitPrefab;

        public TestUnitFactory(Dictionary<Side, Vector3> sp, UnitModel prefab)
        {
            spawnPoints = new Dictionary<Side, Vector3>();
            unitPrefab = prefab;
            foreach (KeyValuePair<Side, Vector3> kvp in sp)
            {
                NavMesh.SamplePosition(kvp.Value, out NavMeshHit hit, 500, 1);
                spawnPoints.Add(kvp.Key, hit.position);
            }
        }

        public IUnit Produce(ulong owner, UnitIdentifier identity, Side side)
        {
            UnitModel unit = Object.Instantiate(unitPrefab);
            unit.SetupData(owner, identity, Defines.UnitValues[identity], spawnPoints[side]);
            return unit;
        }
    }
}
