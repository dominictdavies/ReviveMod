using NUnit.Framework;
using ReviveMod.Source.Common.Commands;
using Terraria;

namespace Testing.Common.Commands
{
	public class Tests
	{
		[SetUp]
		public void Setup()
		{
			Main.player[0] = new() {
				name = "Doomimic"
			};
		}

		[Test]
		public void TestGetDoomimic()
		{
			Assert.IsTrue(ModCommandUtils.TryGetPlayer("Doomimic", out Player player));
		}
	}
}
