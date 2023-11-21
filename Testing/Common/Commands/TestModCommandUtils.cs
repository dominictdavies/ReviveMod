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

        [TestCase(new string[] { "Doomimic" }, new string[] { "Doomimic" })]
        [TestCase(new string[] { "Doomimic", "doomimic", "DOOMIMIC" }, new string[] { "John", "Steven", "Doomimic", "doomimic", "DOOMIMIC" })]
        [TestCase(new string[] { "doomimic", "DOOMIMIC", "John" }, new string[] { "John", "Steven", "Emily", "doomimic", "DOOMIMIC" })]
        public void GetPlayers_IncludedNames_ReturnNullError(string[] includedNames, string[] allNames)
        {
            SetPlayerNames(allNames);

            IEnumerable<Player> players = ModCommandUtils.GetPlayers(includedNames, _players, out string errorMessage);
            int i = 0;
            foreach (Player player in players) {
                Assert.AreEqual(includedNames[i++], player.name);
            }
            Assert.AreEqual(i, players.Count());

            Assert.IsNull(errorMessage);
        }

        [TestCase(new string[] { "Doomimic", "doomimic", "DOOMIMIC" }, new string[] { "John", "doomimic", "DOOMIMIC" })]
        [TestCase(new string[] { "Doomimic", "doomimic", "Steven" }, new string[] { "Doomimic", "Steven" })]
        [TestCase(new string[] { "Doomimic", "Bob", "DOOMIMIC", "Sarah" }, new string[] { "Doomimic", "doomimic", "Bob", "Billy" })]
        public void GetPlayers_MixedNames_ReturnError(string[] mixNames, string[] allNames)
        {
            SetPlayerNames(allNames);

            IEnumerable<string> includedNames = mixNames.Intersect(allNames);
            IEnumerable<string> excludedNames = mixNames.Except(allNames);

            IEnumerable<Player> players = ModCommandUtils.GetPlayers(mixNames, _players, out string errorMessage);
            int i = 0;
            foreach (Player player in players) {
                Assert.AreEqual(includedNames.ElementAt(i++), player.name);
            }
            Assert.AreEqual(i, players.Count());

            string expectedErrorMessage = "The following player name(s) are invalid: " + string.Join(", ", excludedNames) + ".";
            Assert.AreEqual(expectedErrorMessage, errorMessage);
        }
    }
}
