using Model;
using Model.Deck;
using Model.GameModel;
using Model.GameModel.Commands;
using Model.Interfaces;
using Model.Units;
using Model.UnityDependant;
using Model.Weapons;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Tests
{
    public class GameModelTest
    {

        private Mock<IUnitFactory<ulong>> mockedUnitFactory;
        private Mock<ICommandFactory> mockedCommandFactory;
        private Mock<IDiceRoller> mockedDiceRoller;
        private Dictionary<ulong, GamePlayerData> gpd;
        private GameModel<ulong> gameModel;

        [SetUp] // Runs before EACH testmethod
        public void SetupBeforeTest()
        {
            gpd = new Dictionary<ulong, GamePlayerData>()
            {
                { 0, new GamePlayerData("ImperiumPlayer", new DeckObject(), Side.Imperium) },
                { 1, new GamePlayerData("ChaosPlayer", new DeckObject(), Side.Chaos) }
            };

            mockedUnitFactory = new Mock<IUnitFactory<ulong>>();

            mockedCommandFactory = new Mock<ICommandFactory>();

            mockedDiceRoller = new Mock<IDiceRoller>();

            ControlPointModel cp = new ControlPointModel();

            gameModel = new GameModel<ulong>(
                gpd,
                mockedUnitFactory.Object,
                mockedCommandFactory.Object,
                mockedDiceRoller.Object,
                new List<ControlPointModel>() { cp });
            gameModel.StartGame();
        }

        [Test]
        public void StartGameTest()
        {
            Assert.AreEqual(Side.Imperium, gameModel.ActiveSide);
            Assert.AreEqual(gpd[0], gameModel.ActivePlayerData);
            Assert.AreEqual(0, gameModel.ActivePlayerId);
            Assert.AreEqual(1, gameModel.TurnCounter);
            Assert.AreEqual(Phase.Command, gameModel.CurrentPhase);
            Assert.IsNull(gameModel.SelectedUnit);
            Assert.IsNull(gameModel.PendingCommand);

            foreach (GamePlayerData data in gpd.Values)
            {
                Assert.AreEqual(Defines.POINTS_ON_START, data.Currency);
                Assert.AreEqual(Defines.POINTS_PER_TURN, data.PointsGainedPerTurn);
            }
        }

        [Test]
        public void SelectTest()
        {
            Mock<UnitModel> mockedUnit = new Mock<UnitModel>();


            bool didGameModelEventFire = false;
            bool didUnitEventFire = false;
            gameModel.SelectedUnitChanged += (o, e) => didGameModelEventFire = true;
            mockedUnit.Object.Selected += (o, e) => didUnitEventFire = e;

            // Current player is 0
            Assert.AreEqual(0, gameModel.ActivePlayerId);
            gameModel.SelectUnit(1, mockedUnit.Object);
            Assert.IsFalse(didGameModelEventFire);
            Assert.IsNull(gameModel.SelectedUnit);

            gameModel.SelectUnit(0, null);
            Assert.IsTrue(didGameModelEventFire);
            didGameModelEventFire = false;

            gameModel.SelectUnit(0, mockedUnit.Object);
            Assert.IsTrue(didGameModelEventFire);
            Assert.IsTrue(didUnitEventFire);
            Assert.AreEqual(mockedUnit.Object, gameModel.SelectedUnit);

            gameModel.SelectUnit(0, null);
            Assert.IsNull(gameModel.SelectedUnit);
            Assert.IsFalse(didUnitEventFire);
        }

        [Test]
        public void PlayerDoneTest()
        {
            bool didActivePlayerChange = false;
            gameModel.ActivePlayerChanged += (o, e) => didActivePlayerChange = true;

            Assert.AreEqual(0, gameModel.ActivePlayerId);
            // Invalid player Id
            gameModel.PlayerDone(1);
            // Still 0 is the active
            Assert.AreEqual(0, gameModel.ActivePlayerId);
            Assert.IsFalse(didActivePlayerChange);

            gameModel.PlayerDone(0);

            Assert.AreEqual(1, gameModel.ActivePlayerId);
            Assert.AreEqual(Phase.Command, gameModel.CurrentPhase);
            Assert.AreEqual(1, gameModel.TurnCounter);
            Assert.IsTrue(didActivePlayerChange);
        }

        [Test]
        public void PlayerDoneNextPhaseTest()
        {
            gameModel.PlayerDone(0);

            Assert.AreEqual(1, gameModel.ActivePlayerId);
            Assert.AreEqual(Phase.Command, gameModel.CurrentPhase);
            Assert.AreEqual(1, gameModel.TurnCounter);

            gameModel.PlayerDone(1);

            Assert.AreEqual(0, gameModel.ActivePlayerId);
            Assert.AreEqual(Phase.Movement, gameModel.CurrentPhase);
            Assert.AreEqual(1, gameModel.TurnCounter);

            gameModel.PlayerDone(0);

            Assert.AreEqual(1, gameModel.ActivePlayerId);
            Assert.AreEqual(Phase.Movement, gameModel.CurrentPhase);
            Assert.AreEqual(1, gameModel.TurnCounter);

            gameModel.PlayerDone(1);

            Assert.AreEqual(0, gameModel.ActivePlayerId);
            Assert.AreEqual(Phase.Fighting, gameModel.CurrentPhase);
            Assert.AreEqual(1, gameModel.TurnCounter);
        }

        [Test]
        public void PlayerDoneNextTurn()
        {
            for (int i = 0; i < 3; i++)
            {
                gameModel.PlayerDone(0);
                gameModel.PlayerDone(1);
            }

            Assert.AreEqual(0, gameModel.ActivePlayerId);
            Assert.AreEqual(Phase.Command, gameModel.CurrentPhase);
            Assert.AreEqual(2, gameModel.TurnCounter);

            Assert.AreEqual(Defines.POINTS_ON_START + Defines.POINTS_PER_TURN, gpd[0].Currency);
            Assert.AreEqual(Defines.POINTS_ON_START + Defines.POINTS_PER_TURN, gpd[1].Currency);
        }

        [Test]
        public void BuyTest()
        {
            Assert.AreEqual(0, gameModel.ActivePlayerId);
            Assert.AreEqual(Phase.Command, gameModel.CurrentPhase);
            Assert.AreEqual(Defines.POINTS_ON_START, gpd[0].Currency);

            gpd[0].Deck.Add(UnitIdentifier.TacticalMarine);

            // Wrong player
            Assert.IsFalse(gameModel.BuyUnit(1, UnitIdentifier.ChaosLegionnaire));
            // No unit found
            Assert.IsFalse(gameModel.BuyUnit(0, UnitIdentifier.MarineCaptain));
            // Correct
            Assert.IsTrue(gameModel.BuyUnit(0, UnitIdentifier.TacticalMarine));

            Assert.AreEqual(0, gpd[0].Deck.Entries.Count);
            Assert.AreEqual(Defines.POINTS_ON_START - Defines.UnitValues[UnitIdentifier.TacticalMarine].Price, gpd[0].Currency);
            mockedUnitFactory.Verify(m => m.Produce(0, UnitIdentifier.TacticalMarine, Side.Imperium), Times.Once());
        }

        [Test]
        public void BuyWithProducing()
        {
            GameObject g = new GameObject("UnitPrefab", typeof(UnitModel));
            UnitModel prefab = g.GetComponent<UnitModel>();
            UnitModel created = null;

            mockedUnitFactory.Setup(m => m.Produce(It.IsAny<ulong>(), It.IsAny<UnitIdentifier>(), It.IsAny<Side>()))
                .Callback((ulong owner, UnitIdentifier id, Side s) =>
                {
                    created = UnitModel.Instantiate(prefab);
                    created.SetupData(owner, id, Defines.UnitValues[id], Vector3.zero);
                })
                .Returns(created);

            gpd[0].Deck.Add(UnitIdentifier.TacticalMarine);

            Assert.IsTrue(gameModel.BuyUnit(0, UnitIdentifier.TacticalMarine));
            Assert.AreEqual(1, gpd[0].UnitsInPlay.Count);
        }

        [Test]
        public void CreateCommandTest()
        {
            Mock<UnitModel> mockedUnit1 = new Mock<UnitModel>();
            Mock<UnitModel> mockedUnit2 = new Mock<UnitModel>();

            IGameCommand created = null;
            mockedCommandFactory.Setup(m => m.Produce<AttackCommand<Vector3>>(It.IsAny<object[]>()))
                .Callback((object[] args) =>
                {
                    created = new AttackCommand<Vector3>(mockedUnit1.Object, mockedUnit2.Object, mockedDiceRoller.Object, new List<UsableWeapon>());
                })
                .Returns(created);

            Assert.IsNull(gameModel.SelectedUnit);

            // Wrong player
            gameModel.CreateCommand<AttackCommand<Vector3>>(1);
            mockedCommandFactory.Verify(m => m.Produce<AttackCommand<Vector3>>(It.IsAny<object[]>()), Times.Never());

            // Selected is still null
            gameModel.CreateCommand<AttackCommand<Vector3>>(0);
            mockedCommandFactory.Verify(m => m.Produce<AttackCommand<Vector3>>(It.IsAny<object[]>()), Times.Never());

            mockedUnit1.Object.SetupData(0, UnitIdentifier.TacticalMarine, Defines.UnitValues[UnitIdentifier.TacticalMarine], Vector3.zero);
            mockedUnit2.Object.SetupData(1, UnitIdentifier.ChaosLegionnaire, Defines.UnitValues[UnitIdentifier.ChaosLegionnaire], Vector3.zero);

            Assert.AreEqual(UnitIdentifier.TacticalMarine, mockedUnit1.Object.Identity);
            Assert.AreEqual(UnitIdentifier.ChaosLegionnaire, mockedUnit2.Object.Identity);
        }
    }
}
