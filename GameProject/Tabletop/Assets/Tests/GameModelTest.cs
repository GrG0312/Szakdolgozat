using Model;
using Model.Deck;
using Model.GameModel;
using Model.Interfaces;
using Model.Units;
using Model.UnityDependant;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Tests
{
    public class GameModelTest
    {
        [Test]
        public void StartGameTest()
        {
            Mock<IUnitFactory<ulong>> mockedUnitFactory;
            Mock<ICommandFactory> mockedCommandFactory;
            Mock<IDiceRoller> mockedDiceRoller;
            GameModel<ulong> gameModel = CreateAndStartGameModel(out Dictionary<ulong, GamePlayerData> gpd, out mockedUnitFactory, out mockedCommandFactory, out mockedDiceRoller);

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
        public void BuyTest()
        {
            Mock<IUnitFactory<ulong>> mockedUnitFactory;
            Mock<ICommandFactory> mockedCommandFactory;
            Mock<IDiceRoller> mockedDiceRoller;
            GameModel<ulong> gameModel = CreateAndStartGameModel(out Dictionary<ulong, GamePlayerData> gpd, out mockedUnitFactory, out mockedCommandFactory, out mockedDiceRoller);

            gpd[0].Deck.Add(UnitIdentifier.TacticalMarine);

            Assert.AreEqual(0, gameModel.ActivePlayerId);
            Assert.AreEqual(Phase.Command, gameModel.CurrentPhase);

            gpd[0].Deck.Add(UnitIdentifier.TacticalMarine);

            // Wrong player
            Assert.IsFalse(gameModel.BuyUnit(1, UnitIdentifier.ChaosLegionnaire));
            // No unit found
            Assert.IsFalse(gameModel.BuyUnit(0, UnitIdentifier.MarineCaptain));
            // Correct
            Assert.IsTrue(gameModel.BuyUnit(0, UnitIdentifier.TacticalMarine));

            Assert.AreEqual(0, gpd[0].Deck.Entries.Count);
            Assert.AreEqual(Defines.POINTS_ON_START - Defines.UnitValues[UnitIdentifier.TacticalMarine].Price, gpd[0].Currency);
        }

        [Test]
        public void SelectTest()
        {
            Dictionary<ulong, GamePlayerData> gpd = new Dictionary<ulong, GamePlayerData>()
            {
                { 0, new GamePlayerData("ImperiumPlayer", new DeckObject(), Side.Imperium) },
                { 1, new GamePlayerData("ChaosPlayer", new DeckObject(), Side.Chaos) }
            };

            Dictionary<Side, GameObject> spawnpoints = new Dictionary<Side, GameObject>()
            {
                { Side.Imperium, new GameObject("Sp1")}, // creates it at 0,0,0
                { Side.Chaos, new GameObject("Sp2", typeof(Transform)) { transform = { position = new Vector3(10, 0, 10) } } }
            };
            UnitModel m = new UnitModel();
            UnityUnitFactory ufactory = new UnityUnitFactory(spawnpoints, m);

            UnityCommandFactory cfactoy = new UnityCommandFactory();

            DiceRoller r = new DiceRoller();

            ControlPointModel cp = new ControlPointModel();

            GameModel<ulong> gameModel = new GameModel<ulong>(gpd, ufactory, cfactoy, r, new List<ControlPointModel>() { cp });

            bool didGameModelEventFire = false;
            bool didUnitEventFire = false;
            gameModel.StartGame();
            gameModel.SelectedUnitChanged += (o, e) => didGameModelEventFire = true;
            m.Selected += (o, e) => didUnitEventFire = e;

            // Current player is 0
            Assert.AreEqual(0, gameModel.ActivePlayerId);
            gameModel.SelectUnit(1, m);
            Assert.IsFalse(didGameModelEventFire);
            Assert.IsNull(gameModel.SelectedUnit);

            gameModel.SelectUnit(0, null);
            Assert.IsTrue(didGameModelEventFire);
            didGameModelEventFire = false;

            gameModel.SelectUnit(0, m);
            Assert.IsTrue(didGameModelEventFire);
            Assert.IsTrue(didUnitEventFire);
            Assert.AreEqual(m, gameModel.SelectedUnit);

            gameModel.SelectUnit(0, null);
            Assert.IsNull(gameModel.SelectedUnit);
            Assert.IsFalse(didUnitEventFire);
        }

        private GameModel<ulong> CreateAndStartGameModel(
            out Dictionary<ulong, GamePlayerData> gpd, 
            out Mock<IUnitFactory<ulong>> mockedUnitFactory, 
            out Mock<ICommandFactory> mockedCommandFactory,
            out Mock<IDiceRoller> mockedDiceRoller
            )
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

            GameModel<ulong> gameModel = new GameModel<ulong>(
                gpd, 
                mockedUnitFactory.Object, 
                mockedCommandFactory.Object, 
                mockedDiceRoller.Object, 
                new List<ControlPointModel>() { cp });
            gameModel.StartGame();
            return gameModel;
        }
    }
}
