using Model;
using Model.Deck;
using Model.Units;
using NUnit.Framework;

namespace Tests
{
    public class DeckTest
    {
        [Test]
        public void AddSingleTest()
        {
            DeckObject d = new DeckObject();
            Assert.IsNotNull(d.Entries);

            d.Add(UnitIdentifier.MarineCaptain);
            Assert.AreEqual(1, d.Entries.Count);
            Assert.AreEqual(1, d.Entries[0].Amount);
            Assert.AreEqual(Defines.UnitValues[UnitIdentifier.MarineCaptain], d.Entries[0].Constants);

            d.Add(UnitIdentifier.MarineCaptain);
            Assert.AreEqual(1, d.Entries.Count);
            Assert.AreEqual(1, d.Entries[0].Amount);
        }

        [Test]
        public void AddMultipleTest()
        {
            DeckObject d = new DeckObject();
            for (int i = 0; i < 3; i++)
            {
                d.Add(UnitIdentifier.Kriegsman);
            }
            Assert.AreEqual(1, d.Entries.Count);
            Assert.AreEqual(3, d.Entries[0].Amount);
        }

        [Test]
        public void RemoveTest()
        {
            DeckObject d = new DeckObject();
            for (int i = 0; i < 3; i++)
            {
                d.Add(UnitIdentifier.Kriegsman);
            }

            Assert.AreEqual(0, d.RemoveOne(UnitIdentifier.Ballistus));
            Assert.AreEqual(2, d.RemoveOne(UnitIdentifier.Kriegsman));
            d.RemoveOne(UnitIdentifier.Kriegsman);
            d.RemoveOne(UnitIdentifier.Kriegsman);
            Assert.AreEqual(0, d.Entries.Count);
        }
    }
}
