using Model.GameModel;
using Model;
using Model.Deck;
using NUnit.Framework;
using Moq;
using Model.Interfaces;
using System;
using System.Collections.Generic;
using Model.Units;

namespace Tests
{
    public class GamePlayerDataTest
    {
        private GamePlayerData data;

        [SetUp]
        public void SetupBeforeTests()
        {
            data = new GamePlayerData("RandomPlayer", new DeckObject(), Side.Imperium);
        }

        [Test]
        public void ContructorTest()
        {
            Assert.AreEqual("RandomPlayer", data.Name);
            Assert.AreEqual(0, data.Deck.Entries.Count);
            Assert.AreEqual(Side.Imperium, data.Side);
            Assert.AreEqual(0, data.Currency);
            Assert.AreEqual(0, data.CapturedPoints);
            Assert.IsTrue(data.IsConnected);
            Assert.IsFalse(data.IsDefeated);
        }

        [Test]
        public void ResetUnitsTest()
        {
            Mock<IUnit> mockedUnit1 = new Mock<IUnit>();
            mockedUnit1.As<IUsable>().Setup(m => m.ResetUse());
            mockedUnit1.As<IDamageable>().Setup(m => m.Alive).Returns(true);

            Mock<IUnit> mockedUnit2 = new Mock<IUnit>();
            mockedUnit2.As<IUsable>().Setup(m => m.ResetUse());
            mockedUnit2.As<IDamageable>().Setup(m => m.Alive).Returns(false);

            Mock<IUnit> mockedUnit3 = new Mock<IUnit>();
            mockedUnit3.As<IUsable>().Setup(m => m.ResetUse());
            mockedUnit3.As<IDamageable>().Setup(m => m.Alive).Returns(true);

            data.GetUnit(mockedUnit1.Object);
            data.GetUnit(mockedUnit2.Object);
            data.GetUnit(mockedUnit3.Object);

            Assert.AreEqual(3, data.UnitsInPlay.Count);

            data.ResetUnits();

            mockedUnit1.As<IUsable>().Verify(m => m.ResetUse(), Times.Once());
            mockedUnit2.As<IUsable>().Verify(m => m.ResetUse(), Times.Never());
            mockedUnit3.As<IUsable>().Verify(m => m.ResetUse(), Times.Once());

            Assert.AreEqual(2, data.UnitsInPlay.Count);
        }

        [Test]
        public void ForceDeleteTest()
        {
            Mock<IUnit> mockedUnit1 = new Mock<IUnit>();
            mockedUnit1.As<IDamageable>().Setup(m => m.Alive).Returns(true);
            mockedUnit1.As<IDisposable>().Setup(m => m.Dispose());

            Mock<IUnit> mockedUnit2 = new Mock<IUnit>();
            mockedUnit2.As<IDamageable>().Setup(m => m.Alive).Returns(false);
            mockedUnit2.As<IDisposable>().Setup(m => m.Dispose());

            Mock<IUnit> mockedUnit3 = new Mock<IUnit>();
            mockedUnit3.As<IDamageable>().Setup(m => m.Alive).Returns(true);
            mockedUnit3.As<IDisposable>().Setup(m => m.Dispose());

            data.GetUnit(mockedUnit1.Object);
            data.GetUnit(mockedUnit2.Object);
            data.GetUnit(mockedUnit3.Object);

            Assert.AreEqual(3, data.UnitsInPlay.Count);

            data.DeleteUnits(true);

            Assert.AreEqual(0, data.UnitsInPlay.Count);

            mockedUnit1.As<IDisposable>().Verify(m => m.Dispose(), Times.Once());
            mockedUnit2.As<IDisposable>().Verify(m => m.Dispose(), Times.Once());
            mockedUnit3.As<IDisposable>().Verify(m => m.Dispose(), Times.Once());
        }

        [Test]
        public void SimpleCycleTest()
        {
            Mock<IUnit> mockedUnit1 = new Mock<IUnit>();
            mockedUnit1.As<IDamageable>().Setup(m => m.Alive).Returns(false);
            mockedUnit1.As<IUsable>().Setup(m => m.IsUsable(It.IsAny<Phase>())).Returns(true);

            Mock<IUnit> mockedUnit2 = new Mock<IUnit>();
            mockedUnit2.As<IDamageable>().Setup(m => m.Alive).Returns(true);
            mockedUnit2.As<IUsable>().Setup(m => m.IsUsable(It.IsAny<Phase>())).Returns(true);

            Mock<IUnit> mockedUnit3 = new Mock<IUnit>();
            mockedUnit3.As<IDamageable>().Setup(m => m.Alive).Returns(true);
            mockedUnit3.As<IUsable>().Setup(m => m.IsUsable(It.IsAny<Phase>())).Returns(false);

            data.GetUnit(mockedUnit1.Object);
            data.GetUnit(mockedUnit2.Object);
            data.GetUnit(mockedUnit3.Object);

            Assert.AreEqual(mockedUnit2.Object, data.Cycle(Phase.Command));

            mockedUnit2.As<IDamageable>().Setup(m => m.Alive).Returns(false);

            Assert.IsNull(data.Cycle(Phase.Command));
        }

        [Test]
        public void CycleWithStartValueTest()
        {
            Mock<IUnit> mockedUnit1 = new Mock<IUnit>();
            mockedUnit1.As<IDamageable>().Setup(m => m.Alive).Returns(true);
            mockedUnit1.As<IUsable>().Setup(m => m.IsUsable(It.IsAny<Phase>())).Returns(true);

            Mock<IUnit> mockedUnit2 = new Mock<IUnit>();
            mockedUnit2.As<IDamageable>().Setup(m => m.Alive).Returns(true);
            mockedUnit2.As<IUsable>().Setup(m => m.IsUsable(It.IsAny<Phase>())).Returns(true);

            Mock<IUnit> mockedUnit3 = new Mock<IUnit>();
            mockedUnit3.As<IDamageable>().Setup(m => m.Alive).Returns(true);
            mockedUnit3.As<IUsable>().Setup(m => m.IsUsable(It.IsAny<Phase>())).Returns(false);

            Mock<IUnit> mockedUnit4 = new Mock<IUnit>();
            mockedUnit4.As<IDamageable>().Setup(m => m.Alive).Returns(true);
            mockedUnit4.As<IUsable>().Setup(m => m.IsUsable(It.IsAny<Phase>())).Returns(true);

            data.GetUnit(mockedUnit1.Object);
            data.GetUnit(mockedUnit2.Object);
            data.GetUnit(mockedUnit3.Object);
            data.GetUnit(mockedUnit4.Object);

            Assert.AreEqual(mockedUnit4.Object, data.Cycle(Phase.Command, mockedUnit2.Object));
            Assert.AreEqual(mockedUnit1.Object, data.Cycle(Phase.Command, mockedUnit4.Object));

            Mock<IUnit> mockedNotFound = new Mock<IUnit>();

            Assert.Throws<Exception>(() => data.Cycle(Phase.Command, mockedNotFound.Object));

            mockedUnit1.As<IDamageable>().Setup(m => m.Alive).Returns(false);
            mockedUnit2.As<IDamageable>().Setup(m => m.Alive).Returns(false);
            mockedUnit3.As<IDamageable>().Setup(m => m.Alive).Returns(false);
            mockedUnit4.As<IDamageable>().Setup(m => m.Alive).Returns(false);

            Assert.IsNull(data.Cycle(Phase.Command, mockedUnit2.Object));
        }

        [Test]
        public void ForfeitTest()
        {
            bool didDie = false;
            Mock<IUnit> mockedUnit = new Mock<IUnit>();
            mockedUnit.As<IDamageable>().Setup(m => m.Die()).Callback(() => didDie = true);

            data.GetUnit(mockedUnit.Object);

            data.Forfeit();
            Assert.IsTrue(data.IsDefeated);
            Assert.IsFalse(data.IsConnected);
            Assert.IsTrue(didDie);
        }

        [Test]
        public void BuyUnitTest()
        {
            UnitIdentifier id = UnitIdentifier.Kriegsman;
            data.AddPoints(Defines.UnitValues[id].Price * 3 + 1);
            for (int i = 0; i < 3; i++)
            {
                data.Deck.Add(id);
            }
            data.Deck.Add(UnitIdentifier.Baneblade);

            Assert.AreEqual(2, data.Deck.Entries.Count);
            Assert.AreEqual(3, data.Deck.Entries[0].Amount);
            Assert.AreEqual(1, data.Deck.Entries[1].Amount);

            Assert.IsTrue(data.BuyUnit(id));
            Assert.AreEqual(2, data.Deck.Entries.Count);
            Assert.AreEqual(2, data.Deck.Entries[0].Amount);
            Assert.AreEqual(Defines.UnitValues[id].Price * 2 + 1, data.Currency);

            Assert.IsTrue(data.BuyUnit(id));
            Assert.IsTrue(data.BuyUnit(id));
            Assert.AreEqual(1, data.Deck.Entries.Count);
            Assert.AreEqual(UnitIdentifier.Baneblade, data.Deck.Entries[0].TargetUnit);
            Assert.AreEqual(1, data.Currency);

            Assert.IsFalse(data.BuyUnit(UnitIdentifier.Baneblade)); // NO MONEY
            Assert.IsFalse(data.BuyUnit(UnitIdentifier.ChaosCultist)); // NO UNIT
        }
    }
}
