using Model;
using Model.Lobby;
using NUnit.Framework;

namespace Tests
{
    public class LobbySlotTest
    {
        private LobbySlot slot;

        [SetUp]
        public void SetupBeforeTest()
        {
            slot = new LobbySlot(Side.Imperium);
        }

        [Test]
        public void ContructorTest()
        {
            Assert.IsNull(slot.PlayerData);
            Assert.AreEqual(Side.Imperium, slot.Side);
            Assert.AreEqual(SlotOccupantStatus.Open, slot.OccupantStatus);
        }

        [Test]
        public void AssignPlayerTest()
        {
            LobbyPlayerData data = new LobbyPlayerData("RandomPlayer");
            slot.PlayerData = data;
            Assert.AreEqual(slot.Side, data.Side);
            Assert.AreEqual(SlotOccupantStatus.Occupied, slot.OccupantStatus);
            slot.PlayerData = null;
            Assert.AreEqual(SlotOccupantStatus.Open, slot.OccupantStatus);
        }
    }
}
