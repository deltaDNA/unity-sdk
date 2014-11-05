using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DeltaDNA.Messaging
{
	[TestFixture]
	[Category("Image Tests")]
	internal class TestSpriteMap
	{
		[Test]
		public void BuildSpriteMap()
		{
			var smDict = new Dictionary<string, object>() {
				{"url", "http://test.jpg"},
				{"width", 1024},
				{"height", 512},
				{"format", "JPG"}
			};

			SpriteMap s = SpriteMap.BuildFromDictionary(smDict);

			Assert.IsNotNull(s);
			Assert.IsNotNull(s.Url);
			Assert.That(s.Url, Is.EqualTo("http://test.jpg"));
			Assert.That(s.Width, Is.EqualTo(1024));
			Assert.That(s.Height, Is.EqualTo(512));
			Assert.IsNotNull(s.Format);
			Assert.That(s.Format, Is.EqualTo("JPG"));
		}

		[Test]
		[ExpectedException(typeof(ArgumentException), ExpectedMessage = "url is missing")]
		public void BuildBadSpriteMap()
		{
			var smDict = new Dictionary<string, object>() {
				{"width", 1024},
				{"height", 512},
				{"format", "JPG"}
			};

			SpriteMap s = SpriteMap.BuildFromDictionary(smDict);
			Assert.IsNull(s);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException), ExpectedMessage = "url is missing, width is not a valid number")]
		public void BuildBadSpriteMap2()
		{
			var smDict = new Dictionary<string, object>() {
				{"width", "not a number"},
				{"height", 512},
				{"format", "JPG"}
			};

			SpriteMap s = SpriteMap.BuildFromDictionary(smDict);
			Assert.IsNull(s);
		}
	}
}

