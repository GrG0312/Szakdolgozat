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

        public void SingleContesterTest()
        {
            Mock<IUnit> mockedUnit = new Mock<IUnit>();
            mockedUnit.As<ISidedObject>().Setup(m => m.Side).Returns(Side.Imperium);
            mockedUnit.Setup(m => m.Constants).Returns(Defines.UnitValues[UnitIdentifier.TacticalMarine]);

            controlPoint.ContesterChanged(mockedUnit.Object, true);

        }
    }
}
