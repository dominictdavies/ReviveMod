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
            return Enumerable.Range(0, Main.maxPlayers)
                .Select(i => new Player {
                    active = i < allActiveNames.Length,
                    name = i < allActiveNames.Length ? allActiveNames[i] : null
                }).ToArray();
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
            Assert.That(actualPlayers.Count(), Is.Zero);

            string expectedWarning = "The following player name(s) are invalid: " + string.Join(", ", excludedNames) + ".";
            Assert.AreEqual(expectedWarning, warning);
        }

        [TestCase(new string[] { "Doomimic" }, new string[] { "Doomimic" })]
        [TestCase(new string[] { "Doomimic", "Mike", "Emily" }, new string[] { "John", "Steven", "Emily", "Doomimic", "Mike" })]
        public void GetPlayers_IncludedNames_ReturnsPlayers(string[] includedNames, string[] allActiveNames)
        {
            var players = CreatePlayers(allActiveNames);

            var actualPlayers = ModCommandUtils.GetPlayers(includedNames, players, out string warning);
            Assert.That(actualPlayers.Select(player => player.name).ToArray(), Is.EquivalentTo(includedNames));

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
            Assert.That(actualPlayers.Select(player => player.name).ToArray(), Is.EquivalentTo(includedNames));

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
            Assert.That(actualPlayers.Count(), Is.Zero);

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
            Assert.That(actualPlayers.Count(), Is.Zero);

            string expectedWarning = "The following player name(s) are invalid: " + string.Join(", ", names) + ".";
            Assert.AreEqual(expectedWarning, warning);
        }

        [Test]
        public void GetPlayers_KeepOrder_ReturnsPlayersAndWarning()
        {
            var names = new string[] { "John", "Doomimic", "Sarah", "Mike", "Dylan", "Emily" };
            var allActiveNames = new string[] { "Sarah", "Doomimic", "Dylan" };

            var players = CreatePlayers(allActiveNames);
            var includedNames = names.Intersect(allActiveNames);
            var excludedNames = names.Except(allActiveNames);

            var actualPlayers = ModCommandUtils.GetPlayers(names, players, out string warning);
            Assert.That(actualPlayers.Select(player => player.name).ToArray(), Is.EqualTo(includedNames));

            string expectedWarning = "The following player name(s) are invalid: " + string.Join(", ", excludedNames) + ".";
            Assert.AreEqual(expectedWarning, warning);
        }
    }
}
