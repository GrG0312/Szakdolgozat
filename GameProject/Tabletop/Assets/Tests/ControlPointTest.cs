using Model;
using Model.GameModel;
using Model.Interfaces;
using Model.Units;
using Moq;
using NUnit.Framework;

namespace Tests
{
    public class ControlPointTest
    {
        private ControlPointModel controlPoint;

        [SetUp]
        public void SetupBeforeTests()
        {
            controlPoint = new ControlPointModel();
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.AreEqual(-1, controlPoint.Owner);
        }

        [Test]
        public void CalculateTest()
        {
            Assert.AreEqual(-1, controlPoint.Owner);
            bool didEventFire = false;
            controlPoint.OwnerChanged += (o, e) => didEventFire = true;
            controlPoint.CalculateControl();

            Assert.IsTrue(didEventFire);
            Assert.AreEqual(-1, controlPoint.Owner);
        }

        [Test]
        public void SingleContesterTest()
        {
            UnitIdentifier id = UnitIdentifier.TacticalMarine;
            Mock<IUnit> mockedUnit = new Mock<IUnit>();
            mockedUnit.As<ISidedObject>().Setup(m => m.Side).Returns(Side.Imperium);
            mockedUnit.Setup(m => m.Constants).Returns(Defines.UnitValues[id]);

            controlPoint.ContesterChanged(mockedUnit.Object, true);
            Assert.AreEqual((int)Side.Imperium, controlPoint.Owner);

            controlPoint.ContesterChanged(mockedUnit.Object, false);
            Assert.AreEqual((int)Side.Imperium, controlPoint.Owner);
        }

        [Test]
        public void MultipleContesterTest()
        {
            Mock<IUnit> mockedUnit1 = new Mock<IUnit>();
            mockedUnit1.As<ISidedObject>().Setup(m => m.Side).Returns(Side.Imperium);
            mockedUnit1.Setup(m => m.Constants).Returns(Defines.UnitValues[UnitIdentifier.TacticalMarine]); // 2

            Mock<IUnit> mockedUnit2 = new Mock<IUnit>();
            mockedUnit2.As<ISidedObject>().Setup(m => m.Side).Returns(Side.Chaos);
            mockedUnit2.Setup(m => m.Constants).Returns(Defines.UnitValues[UnitIdentifier.Forgefiend]); // 3

            controlPoint.ContesterChanged(mockedUnit1.Object, true);
            controlPoint.ContesterChanged(mockedUnit2.Object, true);

            Assert.AreEqual((int)Side.Chaos, controlPoint.Owner);
            controlPoint.ContesterChanged(mockedUnit1.Object, false);
            Assert.AreEqual((int)Side.Chaos, controlPoint.Owner);
        }
    }
}
