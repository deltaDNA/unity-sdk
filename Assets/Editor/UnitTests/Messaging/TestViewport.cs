using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DeltaDNA.Messaging
{
	[TestFixture]
	[Category("Image Tests")]
	internal class TestViewport
	{
		[Test]
		public void BuildViewport ()
		{
			var vpDict = new Dictionary<string, object>() {
				{"width", 1024},
				{"height", 768}
			};

			Viewport v = Viewport.BuildFromDictionary(vpDict);

			Assert.IsNotNull(v);
			Assert.That(v.Width, Is.EqualTo(1024));
			Assert.That(v.Height, Is.EqualTo(768));
		}

		[Test]
		[ExpectedException(typeof(ArgumentException), ExpectedMessage = "width is missing")]
		public void BuildBadViewport()
		{
			var vpDict = new Dictionary<string, object>() {
				{"height", 768}
			};

			Viewport v = Viewport.BuildFromDictionary(vpDict);
			Assert.IsNull(v);
		}
	}
}

