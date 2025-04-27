using Model;
using Model.GameModel;
using Model.Interfaces;
using Model.Weapons;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using UnityEngine;

namespace Tests
{
    public class WeaponTest
    {
        private UsableWeapon weapon;

        [SetUp]
        public void SetupBeforeTest()
        {
            weapon = new UsableWeapon(new UnitWeapon(WeaponIdentifier.Lascannon, 2));
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.IsTrue(weapon.CanDamage);
            Assert.IsFalse(weapon.IsUsable(Phase.Movement));
            Assert.IsTrue(weapon.IsUsable(Phase.Fighting));
        }

        [Test]
        public async Task DamageTest()
        {
            // 2 Lascannon = 2 * 1 shots
            Mock<IDiceRoller> mockedDiceRoller = new Mock<IDiceRoller>();
            mockedDiceRoller.Setup(m => m.RollDice(It.IsAny<int>())).Returns(Task.FromResult(new int[] { 3, 6 })); // a hit and a miss

            Mock<IDamageable<Vector3>> mockedDamageable = new Mock<IDamageable<Vector3>>();
            mockedDamageable.As<IArmored>().Setup(m => m.ArmorSave(It.IsAny<int>(), It.IsAny<int>(), mockedDiceRoller.Object)).Returns(Task.FromResult(1)); // 1 blocked

            int total = await weapon.Damage(mockedDamageable.Object, mockedDiceRoller.Object);

            mockedDiceRoller.Verify(m => m.RollDice(It.IsAny<int>()), Times.Once());
            mockedDamageable.Verify(m => m.TakeDamage(It.IsAny<int>()), Times.Once());
            mockedDamageable.As<IArmored>().Verify(m => m.ArmorSave(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDiceRoller>()), Times.Once());

            Assert.AreEqual(0, total);
            
            mockedDamageable.As<IArmored>().Setup(m => m.ArmorSave(It.IsAny<int>(), It.IsAny<int>(), mockedDiceRoller.Object)).Returns(Task.FromResult(0)); // 0 blocked

            total = await weapon.Damage(mockedDamageable.Object, mockedDiceRoller.Object);

            Assert.AreEqual(Defines.Weapons[WeaponIdentifier.Lascannon].Damage, total);
        }
    }
}
