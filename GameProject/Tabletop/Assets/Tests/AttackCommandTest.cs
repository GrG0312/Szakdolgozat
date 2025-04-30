using Model.GameModel.Commands;
using Model.Interfaces;
using Model.Weapons;
using Moq;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Model.GameModel;
using System.Threading.Tasks;
using Model;

namespace Tests
{
    public class AttackCommandTest
    {
        private Mock<IWeaponUser<Vector3>> mockedAttacker;
        private Mock<IDamageable<Vector3>> mockedDefender;
        private Mock<IDiceRoller> mockedRoller;
        private List<UsableWeapon> weapons;

        [SetUp]
        public void SetupBeforeTest()
        {
            mockedAttacker = new Mock<IWeaponUser<Vector3>>();
            mockedDefender = new Mock<IDamageable<Vector3>>();
            mockedRoller = new Mock<IDiceRoller>();
            weapons = new List<UsableWeapon>();
        }

        [Test]
        public void ValidCommandTest()
        {
            mockedAttacker.Setup(m => m.CanTarget(It.IsAny<IDamageable<Vector3>>())).Returns(false);
            mockedDefender.Setup(m => m.TakeDamage(It.IsAny<int>()));

            AttackCommand<Vector3> atk = new AttackCommand<Vector3>(mockedAttacker.Object, mockedDefender.Object, mockedRoller.Object, weapons);

            Assert.AreEqual(mockedAttacker.Object, atk.Initiator);
            Assert.AreEqual(mockedDefender.Object, atk.Target);
            Assert.AreEqual(mockedRoller.Object, atk.Roller);
            Assert.AreEqual(Phase.Fighting, atk.ExecutingPhase);

            Assert.IsFalse(atk.ValidCommand(Phase.Fighting)); // no weapon
            weapons.Add(new UsableWeapon(new UnitWeapon(WeaponIdentifier.Lasgun, 4)));
            Assert.IsFalse(atk.ValidCommand(Phase.Fighting)); // cant target
            mockedAttacker.Setup(m => m.CanTarget(It.IsAny<IDamageable<Vector3>>())).Returns(true);
            Assert.IsFalse(atk.ValidCommand(Phase.Command)); // wrong phase
            
            Assert.IsTrue(atk.ValidCommand(Phase.Fighting));

            atk.RegisterUsedWeapon(WeaponIdentifier.Lasgun);
            Assert.IsTrue(atk.CanExecute(Phase.Fighting));
        }

        [Test]
        public async Task ExecuteTest()
        {
            UsableWeapon weapon = new UsableWeapon(new UnitWeapon(WeaponIdentifier.Lasgun, 4));
            weapons.Add(weapon);
            
            // Setup so that we can target
            mockedAttacker.Setup(m => m.CanTarget(It.IsAny<IDamageable<Vector3>>())).Returns(true);
            // Setup so we can take damage
            mockedDefender.Setup(m => m.TakeDamage(It.IsAny<int>()));

            // Setup so we roll for armor save
            mockedDefender.As<IArmored>().Setup(m => m.ArmorSave(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDiceRoller>()))
                .Returns(Task.FromResult(1)); // 1 evaded

            // Setup fixed values when rolling
            mockedRoller.Setup(m => m.RollDice(It.IsAny<int>())).Returns(Task.FromResult(new int[] { 1, 2, 4, 5, 6, 6 })); // 3 misses, 3 hits

            AttackCommand<Vector3> atk = new AttackCommand<Vector3>(mockedAttacker.Object, mockedDefender.Object, mockedRoller.Object, weapons);
            atk.RegisterUsedWeapon(WeaponIdentifier.Lasgun);
            await atk.Execute();

            mockedDefender.As<IArmored>().Verify(m => m.ArmorSave(
                3,
                Defines.Weapons[WeaponIdentifier.Lasgun].ArmorPiercing, 
                It.IsAny<IDiceRoller>()), 
                Times.Once());
            mockedDefender.Verify(m => m.TakeDamage(Defines.Weapons[WeaponIdentifier.Lasgun].Damage * 2), Times.Once()); // take 2 hits worth of damage
        }
    }
}
