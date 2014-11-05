using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DeltaDNA.Messaging
{
	[TestFixture]
	[Category("Image Tests")]
	internal class TestBackground
	{
		[Test]
		[ExpectedException(typeof(Exception), ExpectedMessage = "Exception throwing test")]
		public void ExceptionTest()
		{
			throw new Exception("Exception throwing test");
		}

		[Test]
		public void BuildBackground()
		{
			var bgDict = new Dictionary<string, object>() {
				{"x", 479},
				{"y", 289},
				{"imgX", 25},
				{"imgY", 32},
				{"width", 48},
				{"height", 48},
				{"actionType", "DISMISS"}
			};

			ImageAsset b = ImageAsset.BuildFromDictionary(bgDict);

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
		[ExpectedException(typeof(ArgumentException), ExpectedMessage = "y is missing")]
		public void BuildBadBackground()
		{
			var bgDict = new Dictionary<string, object>() {
				{"x", 479},
				{"imgX", 25},
				{"imgY", 32},
				{"width", 48},
				{"height", 48},
				{"actionType", "LINK"},
				{"actionParam", "http://blah.com"}
			};

			ImageAsset b = ImageAsset.BuildFromDictionary(bgDict);
			Assert.IsNull(b);
		}
	}
}

