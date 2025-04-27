using Model;
using Model.Interfaces;
using Model.Units;
using Model.UnityDependant;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using UnityEngine;

namespace Tests
{
    public class UnitModelTest
    {
        private UnitModel model;
        private UnitIdentifier id;

        [SetUp]
        public void SetupBeforeTest()
        {
            GameObject g = new GameObject("UnitObject", typeof(UnitModel));
            model = g.GetComponent<UnitModel>();
            id = UnitIdentifier.Baneblade;
            model.SetupData(0, id, Defines.UnitValues[id], Vector3.zero);
        }

        [Test]
        public void SetupTest()
        {
            Assert.AreEqual(0, model.Owner);
            Assert.AreEqual(id, model.Identity);
            Assert.AreEqual(Defines.UnitValues[id], model.Constants);
            Assert.AreEqual(Defines.UnitValues[id].Wound, model.CurrentHP);
            Assert.AreEqual(Defines.UnitValues[id].Weapons.Count, model.UsableWeapons.Count);
            Assert.IsTrue(model.Alive);
            Assert.AreEqual(Vector3.zero, model.Position);
        }

        [Test]
        public void DistanceTest()
        {
            Mock<IMapObject<Vector3>> mockedObject = new Mock<IMapObject<Vector3>>();
            mockedObject.Setup(m => m.Position).Returns(new Vector3(15, 0, 0));
            Assert.AreEqual(15, model.DistanceTo(mockedObject.Object));
        }

        [Test]
        public void CanTargetTest()
        {
            Mock<IDamageable<Vector3>> mockedObject1 = new Mock<IDamageable<Vector3>>();
            mockedObject1.As<ISidedObject>().Setup(m => m.Side).Returns(Side.Chaos);
            mockedObject1.As<IOwned<ulong>>().Setup(m => m.Owner).Returns(1);

            Mock<IDamageable<Vector3>> mockedObject2 = new Mock<IDamageable<Vector3>>();
            mockedObject2.As<ISidedObject>().Setup(m => m.Side).Returns(Side.Imperium);
            mockedObject2.As<IOwned<ulong>>().Setup(m => m.Owner).Returns(0);

            Mock<IDamageable<Vector3>> mockedObject3 = new Mock<IDamageable<Vector3>>();
            mockedObject3.As<ISidedObject>().Setup(m => m.Side).Returns(Side.Imperium);
            mockedObject3.As<IOwned<ulong>>().Setup(m => m.Owner).Returns(2);

            Assert.IsTrue(model.CanTarget(mockedObject1.Object));
            Assert.IsFalse(model.CanTarget(mockedObject2.Object));
            Assert.IsFalse(model.CanTarget(mockedObject3.Object));
        }

        [Test]
        public void TakeDamageTest()
        {
            bool didEventFire = false;
            Assert.AreEqual(Defines.UnitValues[id].Wound, model.CurrentHP);
            Assert.IsTrue(model.Alive);
            model.DamageTaken += (o, e) => didEventFire = true;
            model.TakeDamage(1);
            Assert.AreEqual(Defines.UnitValues[id].Wound - 1, model.CurrentHP);
            model.DamageTaken -= (o, e) => didEventFire = true;

            didEventFire = false;
            model.UnitDestroyed += (o, e) => didEventFire = true;
            model.TakeDamage(100);
            Assert.AreEqual(0, model.CurrentHP);
            Assert.IsFalse(model.Alive);
            Assert.IsTrue(didEventFire);
        }

        [Test]
        public async Task ArmorSaveTest()
        {
            Mock<IDiceRoller> mockedDiceRoller = new Mock<IDiceRoller>();
            mockedDiceRoller.Setup(m => m.RollDice(It.IsAny<int>())).Returns(Task.FromResult(new int[]{ 1, 2, 3, 4, 5, 6 }));

            Assert.AreEqual(4, await model.ArmorSave(6, 0, mockedDiceRoller.Object));
            Assert.AreEqual(1, await model.ArmorSave(6, 3, mockedDiceRoller.Object));
            Assert.AreEqual(0, await model.ArmorSave(6, 10, mockedDiceRoller.Object));
        }

        [Test]
        public void MoveTest()
        {

        }

        [Test]
        public void UsableTest()
        {

        }

        [Test]
        public void ResetTest()
        {

        }
    }
}
