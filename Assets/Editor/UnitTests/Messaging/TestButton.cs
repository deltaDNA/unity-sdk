using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DeltaDNA.Messaging
{
	[TestFixture]
	[Category("Image Tests")]
	internal class TestButton
	{
		[Test]
		public void BuildButton()
		{
			var btnDict = new Dictionary<string, object>() {
				{"x", 479},
				{"y", 289},
				{"imgX", 25},
				{"imgY", 32},
				{"width", 48},
				{"height", 48},
				{"actionType", "NONE"}
			};

			ImageAsset b = ImageAsset.BuildFromDictionary(btnDict);

			Assert.IsNotNull(b);
			Assert.IsNotNull(b.GlobalPosition);
			Assert.That(b.GlobalPosition.x, Is.EqualTo(479));
			Assert.That(b.GlobalPosition.y, Is.EqualTo(289));
			Assert.That(b.GlobalPosition.width, Is.EqualTo(48));
			Assert.That(b.GlobalPosition.height, Is.EqualTo(48));
			Assert.IsNotNull(b.ImagePosition);
			Assert.That(b.ImagePosition.x, Is.EqualTo(25));
			Assert.That(b.ImagePosition.y, Is.EqualTo(32));
			Assert.That(b.ImagePosition.width, Is.EqualTo(48));
			Assert.That(b.ImagePosition.height, Is.EqualTo(48));
		}

		[Test]
		[ExpectedException(typeof(ArgumentException), ExpectedMessage = "actionType is missing, height is missing, imgX is missing, imgY is missing, width is missing, x is not a valid number, y is missing")]
		public void BuildBadButton()
		{
			var btnDict = new Dictionary<string, object>() {
				{"x", "asdf"}
			};

			ImageAsset b = ImageAsset.BuildFromDictionary(btnDict);

			Assert.IsNull(b);
		}

		[Test]
		public void BuildSimpleButton()
		{
			var btnDict = new Dictionary<string, object>() {
				{"x", 479},
				{"y", 289},
				{"imgX", 25},
				{"imgY", 32},
				{"width", 48},
				{"height", 48},
				{"actionType", "DISMISS"}
			};

			ImageAsset b = ImageAsset.BuildFromDictionary(btnDict);

			Assert.IsNotNull(b);
			Assert.IsNotNull(b.GlobalPosition);
			Assert.That(b.GlobalPosition.x, Is.EqualTo(479));
			Assert.That(b.GlobalPosition.y, Is.EqualTo(289));
			Assert.That(b.GlobalPosition.width, Is.EqualTo(48));
			Assert.That(b.GlobalPosition.height, Is.EqualTo(48));
			Assert.IsNotNull(b.ImagePosition);
			Assert.That(b.ImagePosition.x, Is.EqualTo(25));
			Assert.That(b.ImagePosition.y, Is.EqualTo(32));
			Assert.That(b.ImagePosition.width, Is.EqualTo(48));
			Assert.That(b.ImagePosition.height, Is.EqualTo(48));
		}
	}
}

