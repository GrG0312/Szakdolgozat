using Model.GameModel;
using Model.GameModel.Commands;
using Model.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Tests
{
    public class MoveCommandTest
    {
        private MoveCommand<Vector3> move;
        private Mock<IMoveable<Vector3>> mockedMovable;

        [SetUp]
        public void SetupBeforeTests()
        {
            mockedMovable = new Mock<IMoveable<Vector3>>();
            mockedMovable.SetupProperty(m => m.Position, Vector3.zero);
            move = new MoveCommand<Vector3>(mockedMovable.Object, new Vector3(15, 0, 15));
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.AreEqual(Vector3.zero, mockedMovable.Object.Position);
            Assert.AreEqual(new Vector3(15, 0, 15), move.EndLocation);
            Assert.AreEqual(mockedMovable.Object, move.TargetUnit);
            Assert.AreEqual(mockedMovable.Object.Position, move.StartPosition);
        }

        [Test]
        public void CanExecuteTest()
        {
            mockedMovable.Setup(m => m.CanMove).Returns(false);
            // Wrong phase
            Assert.IsFalse(move.CanExecute(Phase.Command));
            // Unit cannot move
            Assert.IsFalse(move.CanExecute(Phase.Movement));
            mockedMovable.Setup(m => m.CanMove).Returns(true);
            Assert.IsTrue(move.CanExecute(Phase.Movement));
        }

        [Test]
        public async Task ExecuteTest()
        {
            mockedMovable.SetupProperty(m => m.CanMove, true);
            mockedMovable.Setup(m => m.MoveTo(It.IsAny<Vector3>())).Callback((Vector3 v) =>
            {
                mockedMovable.Object.Position = v;
                mockedMovable.Object.CanMove = false;
            });
            await move.Execute();
            Assert.AreEqual(move.EndLocation, mockedMovable.Object.Position);
            Assert.AreEqual(false, mockedMovable.Object.CanMove);
        }

        [Test]
        public async Task UndoTest()
        {
            mockedMovable.SetupProperty(m => m.CanMove, true);
            mockedMovable.Setup(m => m.MoveTo(It.IsAny<Vector3>())).Callback((Vector3 v) =>
            {
                mockedMovable.Object.Position = v;
                mockedMovable.Object.CanMove = false;
            });
            await move.Execute();

            move.Undo();

            Assert.AreEqual(true, mockedMovable.Object.CanMove);
            Assert.AreEqual(move.StartPosition, mockedMovable.Object.Position);
        }
    }
}
