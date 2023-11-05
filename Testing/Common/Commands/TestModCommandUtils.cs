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

        [Test]
        public void TryGetPlayer_OnlyWrongName()
        {
            string getName = "Doomimic";

            _players[0] = new() { active = true, name = "John" };

            Assert.False(ModCommandUtils.TryGetPlayer(getName, _players, out Player player));
            Assert.IsNull(player);
        }

        [Test]
        public void TryGetPlayer_ManyWrongName()
        {
            string getName = "Doomimic";

            _players[0] = new() { active = true, name = "John" };
            _players[1] = new() { active = true, name = "Sarah" };
            _players[2] = new() { active = true, name = "Steven" };
            _players[3] = new() { active = true, name = "Emily" };
            _players[4] = new() { active = true, name = "Andrew" };

            Assert.False(ModCommandUtils.TryGetPlayer(getName, _players, out Player player));
            Assert.IsNull(player);
        }

        [Test]
        public void TryGetPlayer_OnlyRightName()
        {
            string getName = "Doomimic";

            _players[0] = new() { active = true, name = getName };

            Assert.True(ModCommandUtils.TryGetPlayer(getName, _players, out Player player));
            Assert.AreEqual(getName, player.name);
        }

        [Test]
        public void TryGetPlayer_ManyRightName()
        {
            string getName = "Doomimic";

            _players[0] = new() { active = true, name = getName };
            _players[1] = new() { active = true, name = getName };
            _players[2] = new() { active = true, name = getName };
            _players[3] = new() { active = true, name = getName };
            _players[4] = new() { active = true, name = getName };

            Assert.True(ModCommandUtils.TryGetPlayer(getName, _players, out Player player));
            Assert.AreEqual(getName, player.name);
        }

        [Test]
        public void TryGetPlayer_ManyMixedName()
        {
            string getName = "Doomimic";

            _players[0] = new() { active = true, name = "John" };
            _players[1] = new() { active = true, name = "Sarah" };
            _players[2] = new() { active = true, name = getName };
            _players[3] = new() { active = true, name = getName };
            _players[4] = new() { active = true, name = "Andrew" };

            Assert.True(ModCommandUtils.TryGetPlayer(getName, _players, out Player player));
            Assert.AreEqual(getName, player.name);
        }

        [Test]
        public void GetPlayers_OnlyWrongName()
        {
            string[] getNames = { "Doomimic" };

            _players[0] = new() { active = true, name = "John" };

            IEnumerable<Player> players = ModCommandUtils.GetPlayers(getNames, _players, out string errorMessage);
            int i = 0;
            foreach (Player player in players) {
                Assert.AreEqual(getNames[i], player.name);
                i++;
            }
            Assert.AreEqual(0, i);
            Assert.AreEqual($"The following player name(s) are invalid: {getNames[0]}.", errorMessage);
        }

        [Test]
        public void GetPlayers_ManyWrongName()
        {
            string[] getNames = { "Doomimic", "Fred" };

            _players[0] = new() { active = true, name = "John" };
            _players[1] = new() { active = true, name = "Sarah" };
            _players[2] = new() { active = true, name = "Steven" };
            _players[3] = new() { active = true, name = "Emily" };
            _players[4] = new() { active = true, name = "Andrew" };

            IEnumerable<Player> players = ModCommandUtils.GetPlayers(getNames, _players, out string errorMessage);
            int i = 0;
            foreach (Player player in players) {
                Assert.AreEqual(getNames[i], player.name);
                i++;
            }
            Assert.AreEqual(0, i);
            Assert.AreEqual($"The following player name(s) are invalid: {getNames[0]}, {getNames[1]}.", errorMessage);
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
