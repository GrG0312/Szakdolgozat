using Model;
using Model.Interfaces;
using Model.UnityDependant;
using System.Collections.Generic;
using Model.Units;
using UnityEngine;
using NUnit.Framework;

namespace Tests
{
    public class FactoryTest
    {
        public void UnitProduceTest()
        {
            Dictionary<Side, GameObject> spawnpoints = new Dictionary<Side, GameObject>()
            {
                { Side.Imperium, new GameObject("Sp1")}, // creates it at 0,0,0
                { Side.Chaos, new GameObject("Sp2", typeof(Transform)) { transform = { position = new Vector3(10, 0, 10) } } }
            };
            UnitModel m = new UnitModel();
            UnityUnitFactory ufactory = new UnityUnitFactory(spawnpoints, m);

            UnitModel constructed = ufactory.Produce(0, UnitIdentifier.TacticalMarine, Side.Imperium) as UnitModel;

            Assert.AreEqual(Vector3.zero, constructed.Position);
        }
    }
}
