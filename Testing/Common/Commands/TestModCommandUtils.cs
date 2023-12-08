using NUnit.Framework;
using ReviveMod.Source.Common.Commands;
using System;
using System.Linq;
using Terraria;

namespace ReviveMod.Testing.Common.Commands
{
    [TestFixture]
    internal class TestModCommandUtils
    {
        private static Player[] CreatePlayers(string[] allActiveNames)
        {
            var players = new Player[Main.maxPlayers];

            for (int i = 0; i < Main.maxPlayers; i++) {
                Player player = new() { active = false };

                if (i < allActiveNames.Length) {
                    player.active = true;
                    player.name = allActiveNames[i];
                }

                players[i] = player;
            }

            return players;
        }

        [TestCase("Doomimic", new string[] { "John" })]
        [TestCase("Doomimic", new string[] { "John", "Steven", "Emily", "Sarah", "Mike" })]
        public void TryGetPlayer_ExcludedName_ReturnsFalse(string excludedName, string[] allActiveNames)
        {
            var players = CreatePlayers(allActiveNames);

            Assert.IsFalse(ModCommandUtils.TryGetPlayer(excludedName, players, out Player player));

            Assert.IsNull(player);
        }

        [TestCase("Doomimic", new string[] { "Doomimic" })]
        [TestCase("Doomimic", new string[] { "John", "Steven", "Doomimic", "Emily", "Sarah" })]
        public void TryGetPlayer_IncludedName_ReturnsTrue(string includedName, string[] allActiveNames)
        {
            var players = CreatePlayers(allActiveNames);

            Assert.IsTrue(ModCommandUtils.TryGetPlayer(includedName, players, out Player player));

            Assert.AreEqual(includedName, player.name);
        }

        [Test]
        public void TryGetPlayer_NoActivePlayers_ReturnsFalse()
        {
            var name = "Doomimic";
            var allActiveNames = Array.Empty<string>();

            var players = CreatePlayers(allActiveNames);

            Assert.IsFalse(ModCommandUtils.TryGetPlayer(name, players, out Player player));

            Assert.IsNull(player);
        }

        [Test]
        public void TryGetPlayer_CaseSensitivity_ReturnsFalse()
        {
            var name = "Doomimic";
            var allActiveNames = new string[] { "doomimic", "DOOMIMIC" };

            var players = CreatePlayers(allActiveNames);

            Assert.IsFalse(ModCommandUtils.TryGetPlayer(name, players, out Player player));

            Assert.IsNull(player);
        }

        [TestCase(new string[] { "Doomimic", "Mike", "Emily" }, new string[] { "John" })]
        [TestCase(new string[] { "John", "Doomimic", "Sarah" }, new string[] { "Mike", "Steven", "Emily", "Josh", "Dylan" })]
        public void GetPlayers_ExcludedNames_ReturnsWarning(string[] excludedNames, string[] allActiveNames)
        {
            var players = CreatePlayers(allActiveNames);

            var actualPlayers = ModCommandUtils.GetPlayers(excludedNames, players, out string warning);
            Assert.AreEqual(0, actualPlayers.Count());

            string expectedWarning = "The following player name(s) are invalid: " + string.Join(", ", excludedNames) + ".";
            Assert.AreEqual(expectedWarning, warning);
        }

        [TestCase(new string[] { "Doomimic" }, new string[] { "Doomimic" })]
        [TestCase(new string[] { "Doomimic", "Mike", "Emily" }, new string[] { "John", "Steven", "Emily", "Doomimic", "Mike" })]
        public void GetPlayers_IncludedNames_ReturnsPlayers(string[] includedNames, string[] allActiveNames)
        {
            var players = CreatePlayers(allActiveNames);

            var actualPlayers = ModCommandUtils.GetPlayers(includedNames, players, out string warning);
            int i = 0;
            foreach (Player actualPlayer in actualPlayers) {
                Assert.AreEqual(includedNames[i++], actualPlayer.name);
            }
            Assert.AreEqual(i, actualPlayers.Count());

            Assert.IsNull(warning);
        }

        [TestCase(new string[] { "John", "Doomimic", "Sarah" }, new string[] { "John", "Doomimic", "Steven" })]
        [TestCase(new string[] { "Doomimic", "Sarah", "Steven" }, new string[] { "Sarah", "Steven" })]
        [TestCase(new string[] { "Doomimic", "Josh", "Steven" }, new string[] { "John", "Doomimic", "Steven", "Mike" })]
        public void GetPlayers_MixedNames_ReturnsPlayersAndWarning(string[] mixNames, string[] allActiveNames)
        {
            var players = CreatePlayers(allActiveNames);

            var includedNames = mixNames.Intersect(allActiveNames);
            var excludedNames = mixNames.Except(allActiveNames);

            var actualPlayers = ModCommandUtils.GetPlayers(mixNames, players, out string warning);
            int i = 0;
            foreach (Player player in actualPlayers) {
                Assert.AreEqual(includedNames.ElementAt(i++), player.name);
            }
            Assert.AreEqual(i, actualPlayers.Count());

            string expectedWarning = "The following player name(s) are invalid: " + string.Join(", ", excludedNames) + ".";
            Assert.AreEqual(expectedWarning, warning);
        }

        [Test]
        public void GetPlayers_NoActivePlayers_ReturnsWarning()
        {
            var names = new string[] { "John", "Doomimic", "Sarah" };
            var allActiveNames = Array.Empty<string>();

            var players = CreatePlayers(allActiveNames);

            var actualPlayers = ModCommandUtils.GetPlayers(names, players, out string warning);
            Assert.AreEqual(0, actualPlayers.Count());

            string expectedWarning = "The following player name(s) are invalid: " + string.Join(", ", names) + ".";
            Assert.AreEqual(expectedWarning, warning);
        }

        [Test]
        public void GetPlayers_CaseSensitivity_ReturnsWarning()
        {
            var names = new string[] { "John", "Doomimic", "Sarah" };
            var allActiveNames = new string[] { "john", "JOHN", "dooMIMIC", "sarah", "SARAH"};

            var players = CreatePlayers(allActiveNames);

            var actualPlayers = ModCommandUtils.GetPlayers(names, players, out string warning);
            Assert.AreEqual(0, actualPlayers.Count());

            string expectedWarning = "The following player name(s) are invalid: " + string.Join(", ", names) + ".";
            Assert.AreEqual(expectedWarning, warning);
        }
    }
}
