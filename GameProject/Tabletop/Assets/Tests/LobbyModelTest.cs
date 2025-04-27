using Model.Lobby;
using NUnit.Framework;
using Model;
using System.Collections.Generic;

namespace Tests
{
    public class LobbyModelTest
    {
        private LobbyModel<ulong> model;

        [SetUp]
        public void SetupBeforeTest()
        {
            model = new LobbyModel<ulong>();
            for (int i = 0; i < LobbyModel<ulong>.LOBBY_SIZE; i++)
            {
                model.LobbySlots.Add(new LobbySlot(i % 2 == 0 ? Side.Imperium : Side.Chaos));
            }
        }

        [Test]
        public void ConstructorTest()
        {
            foreach (LobbySlot slot in model.LobbySlots)
            {
                Assert.AreEqual(SlotOccupantStatus.Open, slot.OccupantStatus);
            }
            Assert.AreEqual(0, model.ConnectedClients.Count);
        }

        [Test]
        public void ReserveSlotTest()
        {
            Assert.AreEqual(SlotOccupantStatus.Open, model.LobbySlots[0].OccupantStatus);
            Assert.IsTrue(model.ReserveEmptySlot()); // 0
            Assert.AreEqual(SlotOccupantStatus.Reserved, model.LobbySlots[0].OccupantStatus);

            Assert.IsTrue(model.ReserveEmptySlot()); // 1
            model.LobbySlots[2].OccupantStatus = SlotOccupantStatus.Closed;

            Assert.AreEqual(SlotOccupantStatus.Closed, model.LobbySlots[2].OccupantStatus);
            Assert.IsTrue(model.ReserveEmptySlot()); // 3
            Assert.AreEqual(SlotOccupantStatus.Closed, model.LobbySlots[2].OccupantStatus);

            Assert.IsFalse(model.ReserveEmptySlot());
        }

        [Test]
        public void FindReservedTest()
        {
            Assert.AreEqual(-1, model.FindReservedSlot());
            model.ReserveEmptySlot();
            Assert.AreEqual(0, model.FindReservedSlot());
        }

        [Test]
        public void AddPlayerTest()
        {
            // Slot is not reserved
            Assert.Throws<TabletopException>(() => model.AddNewPlayer(0, "RandomPlayer", 0));
            model.ReserveEmptySlot();
            Assert.IsNull(model.LobbySlots[0].PlayerData);
            model.AddNewPlayer(0, "RandomPlayer", 0);
            Assert.IsNotNull(model.LobbySlots[0].PlayerData);
            Assert.AreEqual(SlotOccupantStatus.Occupied, model.LobbySlots[0].OccupantStatus);
            model.ReserveEmptySlot();
            // Same id
            Assert.Throws<TabletopException>(() => model.AddNewPlayer(0, "RandomPlayer2", 1));
        }

        [Test]
        public void ReassignPlayerTest()
        {
            model.ReserveEmptySlot();
            model.AddNewPlayer(0, "RandomPlayer", 0);

            Assert.AreEqual(SlotOccupantStatus.Open, model.LobbySlots[2].OccupantStatus);
            Assert.IsNull(model.LobbySlots[2].PlayerData);
            // Wrong player ID
            Assert.Throws<TabletopException>(() => model.ReassignPlayerToSlot(1, 2));
            model.LobbySlots[1].OccupantStatus = SlotOccupantStatus.Closed;
            // Closed slot
            Assert.Throws<TabletopException>(() => model.ReassignPlayerToSlot(0, 1));
            model.ReassignPlayerToSlot(0, 2);

            Assert.AreEqual(SlotOccupantStatus.Open, model.LobbySlots[0].OccupantStatus);
            Assert.AreEqual(SlotOccupantStatus.Occupied, model.LobbySlots[2].OccupantStatus);

            Assert.IsNull(model.LobbySlots[0].PlayerData);
            Assert.IsNotNull(model.LobbySlots[2].PlayerData);
        }

        [Test]
        public void GetSlotTest()
        {
            Assert.Throws<TabletopException>(() => model.GetSlotOfPlayer(0));
            model.ReserveEmptySlot();
            model.AddNewPlayer(0, "RandomPlayer", 0);
            Assert.AreEqual(model.LobbySlots[0], model.GetSlotOfPlayer(0));
        }

        [Test]
        public void RemovePlayerTest()
        {
            model.ReserveEmptySlot();
            model.AddNewPlayer(0, "RandomPlayer", 0);
            // No player
            Assert.Throws<TabletopException>(() => model.RemovePlayer(1));
            LobbySlot slot = model.GetSlotOfPlayer(0);
            slot.PlayerData = null;
            // No assigned slot
            Assert.Throws<TabletopException>(() => model.RemovePlayer(0));
            slot.PlayerData = model.ConnectedClients[0];
            LobbyPlayerData data = model.ConnectedClients[0];
            Assert.AreEqual(data, model.RemovePlayer(0));
            Assert.IsNull(model.LobbySlots[0].PlayerData);
            Assert.AreEqual(SlotOccupantStatus.Open, model.LobbySlots[0].OccupantStatus);
            Assert.Throws<TabletopException>(() => model.GetSlotOfPlayer(0));
        }
    }
}
