using NUnit.Framework;
using ReviveMod.Source.Common.Commands;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace ReviveMod.Testing.Common.Commands
{
    [TestFixture]
    internal class TestModCommandUtils
    {
        private Player[] _players;

        [SetUp]
        public void SetUp()
        {
            _players = new Player[Main.maxPlayers];

            for (int i = 0; i < Main.maxPlayers; i++) {
                _players[i] = new Player() { active = false };
            }
        }

        private void SetPlayerNames(string[] allNames)
        {
            int i = 0;
            foreach (string name in allNames) {
                _players[i].active = true;
                _players[i].name = name;
                i++;
            }
        }

        [TestCase("Doomimic", new string[0])]
        [TestCase("Doomimic", new string[] { "John" })]
        [TestCase("Doomimic", new string[] { "John", "Steven", "Emily", "doomimic", "DOOMIMIC" })]
        public void TryGetPlayer_ExcludedName_ReturnFalse(string excludedName, string[] allNames)
        {
            SetPlayerNames(allNames);
            Assert.IsFalse(ModCommandUtils.TryGetPlayer(excludedName, _players, out Player player));
            Assert.IsNull(player);
        }

        [TestCase("Doomimic", new string[] { "Doomimic" })]
        [TestCase("Doomimic", new string[] { "John", "Steven", "Doomimic", "doomimic", "DOOMIMIC" })]
        [TestCase("Doomimic", new string[] { "doomimic", "DOOMIMIC", "Doomimic", "Doomimic", "Doomimic" })]
        public void TryGetPlayer_IncludedName_ReturnTrue(string includedName, string[] allNames)
        {
            SetPlayerNames(allNames);
            Assert.IsTrue(ModCommandUtils.TryGetPlayer(includedName, _players, out Player player));
            Assert.AreEqual(includedName, player.name);
        }

        [TestCase(new string[] { "Doomimic" }, new string[0])]
        [TestCase(new string[] { "Doomimic", "doomimic", "DOOMIMIC" }, new string[] { "John" })]
        [TestCase(new string[] { "Doomimic", "doom", "DOOM" }, new string[] { "John", "Steven", "Emily", "doomimic", "DOOMIMIC" })]
        public void GetPlayers_ExcludedNames_ReturnError(string[] excludedNames, string[] allNames)
        {
            SetPlayerNames(allNames);
            IEnumerable<Player> players = ModCommandUtils.GetPlayers(excludedNames, _players, out string errorMessage);
            Assert.AreEqual(0, players.Count());

            string expectedErrorMessage = "The following player name(s) are invalid: " + string.Join(", ", excludedNames) + ".";
            Assert.AreEqual(expectedErrorMessage, errorMessage);
        }

        [Test]
        public void GetPlayers_OnlyRightName()
        {
            string[] getNames = { "Doomimic" };

            _players[0] = new() { active = true, name = getNames[0] };

            IEnumerable<Player> players = ModCommandUtils.GetPlayers(getNames, _players, out string errorMessage);
            int i = 0;
            foreach (Player player in players) {
                Assert.AreEqual(getNames[i], player.name);
                i++;
            }
            Assert.AreEqual(1, i);
            Assert.IsNull(errorMessage);
        }

        [Test]
        public void GetPlayers_ManyRightName()
        {
            string[] getNames = { "Doomimic", "Fred", "Johnny" };

            _players[0] = new() { active = true, name = getNames[0] };
            _players[1] = new() { active = true, name = getNames[1] };
            _players[2] = new() { active = true, name = getNames[2] };

            IEnumerable<Player> players = ModCommandUtils.GetPlayers(getNames, _players, out string errorMessage);
            int i = 0;
            foreach (Player player in players) {
                Assert.AreEqual(getNames[i], player.name);
                i++;
            }
            Assert.IsNull(errorMessage);
        }

        [Test]
        public void GetPlayers_ManyMixedName()
        {
            string[] getNames = { "Doomimic", "Fred", "Johnny", "Sally", "Bob" };

            _players[0] = new() { active = true, name = "John" };
            _players[1] = new() { active = true, name = "Sarah" };
            _players[2] = new() { active = true, name = getNames[1] };
            _players[3] = new() { active = true, name = getNames[3] };
            _players[4] = new() { active = true, name = "Andrew" };

            IEnumerable<Player> players = ModCommandUtils.GetPlayers(getNames, _players, out string errorMessage);
            int i = 0;
            foreach (Player player in players) {
                if (i == 0) {
                    Assert.AreEqual(getNames[1], player.name);
                } else {
                    Assert.AreEqual(getNames[3], player.name);
                }
                i++;
            }
            Assert.AreEqual(2, i);
            Assert.AreEqual($"The following player name(s) are invalid: {getNames[0]}, {getNames[2]}, {getNames[4]}.", errorMessage);
        }
    }
}
