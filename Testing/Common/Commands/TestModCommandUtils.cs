using NUnit.Framework;
using ReviveMod.Source.Common.Commands;
using Terraria;

namespace ReviveMod.Testing.Common.Commands
{
	[TestFixture]
	internal class TestModCommandUtils
	{
		[Test]
		public void TestGetDoomimic()
		{
			Player[] players = new Player[]
			{
				new()
				{
					active = true,
					name = "Doomimic"
				}
			};

			Assert.True(ModCommandUtils.TryGetPlayer("Doomimic", players, out Player player));
			Assert.AreEqual(player.name, "Doomimic");
		}
	}
}
