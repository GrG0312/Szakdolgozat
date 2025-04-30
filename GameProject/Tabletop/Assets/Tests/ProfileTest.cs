using Model;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class ProfileTest
    {
        private Profile profile;

        [SetUp]
        public void SetupBeforeTests()
        {
            profile = Profile.Default;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.AreEqual("Player", profile.DisplayName);
            Assert.AreEqual(0, profile.GamesPlayed);
            Assert.AreEqual(0, profile.GamesWon);
        }

        [Test]
        public void ChangeNameTest()
        {
            profile.ChangeName("");
            Assert.AreEqual("Player", profile.DisplayName);
            profile.ChangeName("Asder");
            Assert.AreEqual("Asder", profile.DisplayName);
        }
    }
}
