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
using System.Threading.Tasks;
using UnityEngine;

namespace Tests
{
    public class GameModelTest
    {

        private Mock<IUnitFactory<ulong>> mockedUnitFactory;
        private Mock<ICommandFactory> mockedCommandFactory;
        private Mock<IDiceRoller> mockedDiceRoller;
        private Dictionary<ulong, Mock<GamePlayerData>> mockedPlayerDatas;
        private GameModel<ulong> gameModel;
        private ControlPointModel cp;

        [SetUp] // Runs before EACH testmethod
        public void SetupBeforeTest()
        {
            mockedPlayerDatas = new Dictionary<ulong, Mock<GamePlayerData>>()
            {
                { 0, new Mock<GamePlayerData>("ImperialPlayer", new DeckObject(), Side.Imperium) },
                { 1, new Mock<GamePlayerData>("ChaosPlayer", new DeckObject(), Side.Chaos) },
            };

            mockedUnitFactory = new Mock<IUnitFactory<ulong>>();

            mockedCommandFactory = new Mock<ICommandFactory>();

            mockedDiceRoller = new Mock<IDiceRoller>();

            cp = new ControlPointModel();

            Dictionary<ulong, GamePlayerData> mockedData = new Dictionary<ulong, GamePlayerData>();
            foreach (var kvp in mockedPlayerDatas)
            {
                mockedData.Add(kvp.Key, kvp.Value.Object);
            }

            gameModel = new GameModel<ulong>(
                mockedData,
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
            Assert.AreEqual(mockedPlayerDatas[0].Object, gameModel.ActivePlayerData);
            Assert.AreEqual(0, gameModel.ActivePlayerId);
            Assert.AreEqual(1, gameModel.TurnCounter);
            Assert.AreEqual(Phase.Command, gameModel.CurrentPhase);
            Assert.IsNull(gameModel.SelectedUnit);
            Assert.IsNull(gameModel.PendingCommand);
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
            mockedPlayerDatas[0].CallBase = true;
            mockedPlayerDatas[1].CallBase = true;

            for (int i = 0; i < 3; i++)
            {
                gameModel.PlayerDone(0);
                gameModel.PlayerDone(1);
            }

            Assert.AreEqual(0, gameModel.ActivePlayerId);
            Assert.AreEqual(Phase.Command, gameModel.CurrentPhase);
            Assert.AreEqual(2, gameModel.TurnCounter);

            Assert.AreEqual(Defines.POINTS_ON_START + Defines.POINTS_PER_TURN, gameModel.ConnectedPlayers[0].Currency);
            Assert.AreEqual(Defines.POINTS_ON_START + Defines.POINTS_PER_TURN, gameModel.ConnectedPlayers[1].Currency);
        }

        [Test]
        public void BuyTest()
        {
            Assert.AreEqual(0, gameModel.ActivePlayerId);
            Assert.AreEqual(Phase.Command, gameModel.CurrentPhase);

            mockedPlayerDatas[0].CallBase = true;
            mockedPlayerDatas[1].CallBase = true;

            mockedPlayerDatas[0].Object.Deck.Add(UnitIdentifier.TacticalMarine);

            // Wrong player
            Assert.IsFalse(gameModel.BuyUnit(1, UnitIdentifier.ChaosLegionnaire));
            // No unit found
            Assert.IsFalse(gameModel.BuyUnit(0, UnitIdentifier.MarineCaptain));
            // Correct
            Assert.IsTrue(gameModel.BuyUnit(0, UnitIdentifier.TacticalMarine));

            mockedUnitFactory.Verify(m => m.Produce(0, UnitIdentifier.TacticalMarine, Side.Imperium), Times.Once());
        }

        [Test]
        public void BuyWithProducing()
        {
            Mock<IUnit> mockedUnit = new Mock<IUnit>();

            mockedUnitFactory.Setup(m => m.Produce(It.IsAny<ulong>(), It.IsAny<UnitIdentifier>(), It.IsAny<Side>()))
                .Returns(mockedUnit.Object);

            mockedPlayerDatas[0].CallBase = true;
            mockedPlayerDatas[0].Object.Deck.Add(UnitIdentifier.TacticalMarine);

            Assert.IsTrue(gameModel.BuyUnit(0, UnitIdentifier.TacticalMarine));
            Assert.AreEqual(1, mockedPlayerDatas[0].Object.UnitsInPlay.Count);
        }

        [Test]
        public void CreateCommandTest()
        {
            Mock<UnitModel> mockedUnit1 = new Mock<UnitModel>();
            Mock<UnitModel> mockedUnit2 = new Mock<UnitModel>();

            List<UsableWeapon> usables = new List<UsableWeapon>()
            {
                new UsableWeapon(Defines.UnitValues[UnitIdentifier.TacticalMarine].Weapons[0])
            };

            IGameCommand created = new AttackCommand<Vector3>(mockedUnit1.Object, mockedUnit2.Object, mockedDiceRoller.Object, usables);
            
            mockedCommandFactory.Setup(m => m.Produce<AttackCommand<Vector3>>(It.IsAny<object[]>()))
                .Returns(created);

            Assert.IsNull(gameModel.SelectedUnit);

            // Wrong player
            gameModel.CreateCommand<AttackCommand<Vector3>>(1);
            mockedCommandFactory.Verify(m => m.Produce<AttackCommand<Vector3>>(It.IsAny<object[]>()), Times.Never());

            // Selected is still null
            gameModel.CreateCommand<AttackCommand<Vector3>>(0);
            mockedCommandFactory.Verify(m => m.Produce<AttackCommand<Vector3>>(It.IsAny<object[]>()), Times.Never());

            gameModel.SelectUnit(0, mockedUnit1.Object);

            Assert.AreEqual(mockedUnit1.Object, gameModel.SelectedUnit);

            gameModel.CreateCommand<AttackCommand<Vector3>>(0);
            mockedCommandFactory.Verify(m => m.Produce<AttackCommand<Vector3>>(It.IsAny<object[]>()), Times.Once());

            Assert.AreEqual(created, gameModel.PendingCommand);
        }

        [Test]
        public void ExecuteCommandTest()
        {
            Assert.IsNull(gameModel.PendingCommand);
            Assert.IsNull(gameModel.SelectedUnit);

            Mock<IGameCommand> mockedCommand = new Mock<IGameCommand>();
            mockedCommand.Setup(m => m.CanExecute(It.IsAny<Phase>())).Returns(false);
            Mock<ISelectable<ulong>> mockedUnit = new Mock<ISelectable<ulong>>();

            mockedCommandFactory.Setup(m => m.Produce<AttackCommand<Vector3>>(It.IsAny<object[]>())).Returns(mockedCommand.Object);

            gameModel.SelectUnit(0, mockedUnit.Object);
            gameModel.CreateCommand<AttackCommand<Vector3>>(0);

            Assert.AreEqual(mockedCommand.Object, gameModel.PendingCommand);

            _ = gameModel.ExecuteCommand();

            // Cannot execute
            Assert.AreEqual(mockedCommand.Object, gameModel.PendingCommand);
            mockedCommand.Verify(m => m.Execute(), Times.Never());

            mockedCommand.Setup(m => m.CanExecute(It.IsAny<Phase>())).Returns(true);

            _ = gameModel.ExecuteCommand();

            mockedCommand.Verify(m => m.Execute(), Times.Once());
            Assert.IsNull(gameModel.PendingCommand);
        }

        [Test]
        public void CycleTest()
        {
            Mock<IUnit> mockedUnit1 = new Mock<IUnit>();
            mockedUnit1.As<IUsable>().Setup(m => m.IsUsable(It.IsAny<Phase>())).Returns(true);
            mockedUnit1.As<IDamageable>().Setup(m => m.Alive).Returns(true);
            mockedUnit1.As<ISelectable<ulong>>().Setup(m => m.Owner).Returns(0);

            Mock<IUnit> mockedUnit2 = new Mock<IUnit>();
            mockedUnit2.As<IUsable>().Setup(m => m.IsUsable(It.IsAny<Phase>())).Returns(false);
            mockedUnit2.As<IDamageable>().Setup(m => m.Alive).Returns(true);
            mockedUnit2.As<ISelectable<ulong>>().Setup(m => m.Owner).Returns(0);

            Mock<IUnit> mockedUnit3 = new Mock<IUnit>();
            mockedUnit3.As<IUsable>().Setup(m => m.IsUsable(It.IsAny<Phase>())).Returns(true);
            mockedUnit3.As<IDamageable>().Setup(m => m.Alive).Returns(true);
            mockedUnit3.As<ISelectable<ulong>>().Setup(m => m.Owner).Returns(0);

            mockedPlayerDatas[0].Object.GetUnit(mockedUnit1.Object);
            mockedPlayerDatas[0].Object.GetUnit(mockedUnit2.Object);
            mockedPlayerDatas[0].Object.GetUnit(mockedUnit3.Object);

            Assert.IsNull(gameModel.SelectedUnit);

            bool didUnitCycled = false;
            gameModel.UnitCycled += (o,e) => { didUnitCycled = true; };

            gameModel.CycleUnits(1);

            Assert.IsFalse(didUnitCycled);
            Assert.IsNull(gameModel.SelectedUnit);

            gameModel.CycleUnits(0);

            Assert.IsTrue(didUnitCycled);
            Assert.AreEqual(mockedUnit1.Object, gameModel.SelectedUnit);
            didUnitCycled = false;

            gameModel.CycleUnits(0);

            Assert.IsTrue(didUnitCycled);
            Assert.AreEqual(mockedUnit3.Object, gameModel.SelectedUnit);
            didUnitCycled = false;

            Mock<ISelectable<ulong>> mockedEnemyUnit = new Mock<ISelectable<ulong>>();
            mockedEnemyUnit.Setup(m => m.Owner).Returns(1);

            gameModel.SelectUnit(0, mockedEnemyUnit.Object);

            mockedUnit1.As<IUsable>().Setup(m => m.IsUsable(It.IsAny<Phase>())).Returns(false);
            mockedUnit3.As<IUsable>().Setup(m => m.IsUsable(It.IsAny<Phase>())).Returns(false);

            gameModel.CycleUnits(0);

            Assert.IsTrue(didUnitCycled);
            Assert.IsNull(gameModel.SelectedUnit);
        }

        [Test]
        public void ControlPointChangedTest()
        {
            Mock<IUnit> mockedUnit1 = new Mock<IUnit>();
            mockedUnit1.As<ISidedObject>().Setup(m => m.Side).Returns(Side.Imperium);
            mockedUnit1.Setup(m => m.Constants).Returns(Defines.UnitValues[UnitIdentifier.TacticalMarine]);

            Mock<IUnit> mockedUnit2 = new Mock<IUnit>();
            mockedUnit2.As<ISidedObject>().Setup(m => m.Side).Returns(Side.Chaos);
            mockedUnit2.Setup(m => m.Constants).Returns(Defines.UnitValues[UnitIdentifier.ChaosLegionnaire]);

            Assert.AreEqual(0, mockedPlayerDatas[0].Object.CapturedPoints);
            cp.ContesterChanged(mockedUnit1.Object, true);
            Assert.AreEqual(1, mockedPlayerDatas[0].Object.CapturedPoints);
            cp.ContesterChanged(mockedUnit1.Object, false);
            Assert.AreEqual(1, mockedPlayerDatas[0].Object.CapturedPoints);
            cp.ContesterChanged(mockedUnit2.Object, true);
            Assert.AreEqual(0, mockedPlayerDatas[0].Object.CapturedPoints);
            Assert.AreEqual(1, mockedPlayerDatas[1].Object.CapturedPoints);

        }

        [Test]
        public void ForfeitNotActiveTest()
        {
            Assert.AreEqual(0, gameModel.ActivePlayerId);
            gameModel.Forfeit(1);
            Assert.AreEqual(0, gameModel.ActivePlayerId);
            Assert.AreEqual(2, gameModel.ConnectedPlayers.Count);
            Assert.IsFalse(gameModel.ConnectedPlayers[1].IsConnected);
            Assert.IsTrue(gameModel.ConnectedPlayers[1].IsDefeated);
        }

        [Test]
        public void ForfeitActiveTest()
        {
            int winner = -1;
            gameModel.GameOver += (s, e) => winner = (int)e;
            Assert.AreEqual(0, gameModel.ActivePlayerId);
            gameModel.Forfeit(0);
            // 0 because when game is over it wont go to the next player
            Assert.AreEqual(0, gameModel.ActivePlayerId);
            Assert.AreEqual((int)Side.Chaos, winner);
        }

        [Test]
        public void AbortCommandTest()
        {
            Mock<IGameCommand> mockedCommand = new Mock<IGameCommand>();
            Mock<ISelectable<ulong>> mockedUnit = new Mock<ISelectable<ulong>>();

            mockedCommandFactory.Setup(m => m.Produce<AttackCommand<Vector3>>(It.IsAny<object[]>())).Returns(mockedCommand.Object);

            gameModel.SelectUnit(0, mockedUnit.Object);
            gameModel.CreateCommand<AttackCommand<Vector3>>(0);

            Assert.IsNotNull(gameModel.PendingCommand);
            gameModel.AbortCommand();
            Assert.IsNull(gameModel.PendingCommand);
        }

        [Test]
        public async Task UndoCommandTest()
        {
            Assert.DoesNotThrow(() => gameModel.UndoCommand(0));

            Mock<IGameCommand> mockedCommand = new Mock<IGameCommand>();
            mockedCommand.Setup(m => m.CanExecute(It.IsAny<Phase>())).Returns(true);
            mockedCommand.As<IUndoableCommand>().Setup(m => m.Undo());
            Mock<ISelectable<ulong>> mockedUnit = new Mock<ISelectable<ulong>>();

            mockedCommandFactory.Setup(m => m.Produce<MoveCommand<Vector3>>(It.IsAny<object[]>())).Returns(mockedCommand.Object);

            gameModel.SelectUnit(0, mockedUnit.Object);
            gameModel.CreateCommand<MoveCommand<Vector3>>(0);
            await gameModel.ExecuteCommand();

            Assert.IsNull(gameModel.PendingCommand);
            mockedCommand.Verify(m => m.Execute(), Times.Once());
            mockedCommand.As<IUndoableCommand>().Verify(m => m.Undo(), Times.Never());
            gameModel.UndoCommand(1);
            mockedCommand.As<IUndoableCommand>().Verify(m => m.Undo(), Times.Never());
            gameModel.UndoCommand(0);
            mockedCommand.As<IUndoableCommand>().Verify(m => m.Undo(), Times.Once());
            gameModel.UndoCommand(0);
            mockedCommand.As<IUndoableCommand>().Verify(m => m.Undo(), Times.Once());
        }
    }
}
