﻿using NUnit.Framework;
using ReviveMod.Source.Common.Commands;
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

        [TestCase("Doomimic", new string[0])]
        [TestCase("Doomimic", new string[] { "John" })]
        [TestCase("Doomimic", new string[] { "John", "Steven", "Emily", "doomimic", "DOOMIMIC" })]
        public void TryGetPlayer_ExcludedName_ReturnsFalse(string excludedName, string[] allNames)
        {
            var players = CreatePlayers(allNames);

            Assert.IsFalse(ModCommandUtils.TryGetPlayer(excludedName, players, out Player player));

            Assert.IsNull(player);
        }

        [TestCase("Doomimic", new string[] { "Doomimic" })]
        [TestCase("Doomimic", new string[] { "John", "Steven", "Doomimic", "doomimic", "DOOMIMIC" })]
        [TestCase("Doomimic", new string[] { "doomimic", "DOOMIMIC", "Doomimic", "Doomimic", "Doomimic" })]
        public void TryGetPlayer_IncludedName_ReturnsTrue(string includedName, string[] allNames)
        {
            var players = CreatePlayers(allNames);

            Assert.IsTrue(ModCommandUtils.TryGetPlayer(includedName, players, out Player player));

            Assert.AreEqual(includedName, player.name);
        }

        [TestCase(new string[] { "Doomimic" }, new string[0])]
        [TestCase(new string[] { "Doomimic", "doomimic", "DOOMIMIC" }, new string[] { "John" })]
        [TestCase(new string[] { "Doomimic", "doom", "DOOM" }, new string[] { "John", "Steven", "Emily", "doomimic", "DOOMIMIC" })]
        public void GetPlayers_ExcludedNames_ReturnsError(string[] excludedNames, string[] allNames)
        {
            var players = CreatePlayers(allNames);

            var actualPlayers = ModCommandUtils.GetPlayers(excludedNames, players, out string errorMessage);
            Assert.AreEqual(0, actualPlayers.Count());

            string expectedErrorMessage = "The following player name(s) are invalid: " + string.Join(", ", excludedNames) + ".";
            Assert.AreEqual(expectedErrorMessage, errorMessage);
        }

        [TestCase(new string[] { "Doomimic" }, new string[] { "Doomimic" })]
        [TestCase(new string[] { "Doomimic", "doomimic", "DOOMIMIC" }, new string[] { "John", "Steven", "Doomimic", "doomimic", "DOOMIMIC" })]
        [TestCase(new string[] { "doomimic", "DOOMIMIC", "John" }, new string[] { "John", "Steven", "Emily", "doomimic", "DOOMIMIC" })]
        public void GetPlayers_IncludedNames_ReturnsNullError(string[] includedNames, string[] allNames)
        {
            var players = CreatePlayers(allNames);

            var actualPlayers = ModCommandUtils.GetPlayers(includedNames, players, out string errorMessage);
            int i = 0;
            foreach (Player actualPlayer in actualPlayers) {
                Assert.AreEqual(includedNames[i++], actualPlayer.name);
            }
            Assert.AreEqual(i, actualPlayers.Count());

            Assert.IsNull(errorMessage);
        }

        [TestCase(new string[] { "Doomimic", "doomimic", "DOOMIMIC" }, new string[] { "John", "doomimic", "DOOMIMIC" })]
        [TestCase(new string[] { "Doomimic", "doomimic", "Steven" }, new string[] { "Doomimic", "Steven" })]
        [TestCase(new string[] { "Doomimic", "Bob", "DOOMIMIC", "Sarah" }, new string[] { "Doomimic", "doomimic", "Bob", "Billy" })]
        public void GetPlayers_MixedNames_ReturnsError(string[] mixNames, string[] allNames)
        {
            var players = CreatePlayers(allNames);

            var includedNames = mixNames.Intersect(allNames);
            var excludedNames = mixNames.Except(allNames);

            var actualPlayers = ModCommandUtils.GetPlayers(mixNames, players, out string errorMessage);
            int i = 0;
            foreach (Player player in actualPlayers) {
                Assert.AreEqual(includedNames.ElementAt(i++), player.name);
            }
            Assert.AreEqual(i, actualPlayers.Count());

            string expectedErrorMessage = "The following player name(s) are invalid: " + string.Join(", ", excludedNames) + ".";
            Assert.AreEqual(expectedErrorMessage, errorMessage);
        }
    }
}
