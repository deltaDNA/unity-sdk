using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DeltaDNA.Messaging
{
	[TestFixture]
	[Category("Image Tests")]
	internal class TestImageComposition
	{
		[Test]
		public void BuildComposition()
		{
			var imgDict = new Dictionary<string, object>() {
				{"url", "http://test.jpg"},
				{"width", 1024},
				{"height", 512},
				{"format", "JPG"},
				{"viewport", new Dictionary<string, object>() {
					{"width", 1024},
					{"height", 768}
				}},
				{"background", new Dictionary<string, object>() {
					{"x", 2},
					{"y", 2},
					{"imgX", 0},
					{"imgY", 0},
					{"width", 200},
					{"height", 300},
					{"actionType", "NONE"}
				}},
				{"button1", new Dictionary<string, object>() {
					{"x", 205},
					{"y", 2},
					{"imgX", 20},
					{"imgY", 280},
					{"width", 48},
					{"height", 24},
					{"label", "Purchase"},
					{"name", "purchase"},
					{"actionType", "LINK"},
					{"actionParam", "http://blah.com"}
				}},
				{"button2", new Dictionary<string, object>() {
					{"x", 205},
					{"y", 30},
					{"imgX", 200},
					{"imgY", 280},
					{"width", 48},
					{"height", 24},
					{"label", "Ignore"},
					{"name", "cancel"},
					{"actionType", "DISMISS"}
				}}
			};

			ImageComposition c = ImageComposition.BuildFromDictionary(imgDict);

			Assert.IsNotNull(c, "Composition is null");
			Assert.IsNotNull(c.SpriteMap, "SpriteMap is null");
			Assert.IsNotNull(c.Viewport, "Viewport is null");
			Assert.IsNotNull(c.Background, "Background is null");
			Assert.IsNotNull(c.Button1, "Button1 is null");
			Assert.IsNotNull(c.Button2, "Button2 is null");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException), ExpectedMessage = "background is missing, url is missing")]
		public void BuildBadComposition()
		{
			var imgDict = new Dictionary<string, object>() {
				{"width", 1024},
				{"height", 512},
				{"format", "JPG"},
				{"viewport", new Dictionary<string, object>() {
					{"width", 1024},
					{"height", 768}
				}},
				{"button1", new Dictionary<string, object>() {
					{"x", 205},
					{"y", 2},
					{"imgX", 20},
					{"imgY", 280},
					{"width", 48},
					{"height", 24},
					{"label", "Purchase"},
					{"name", "purchase"},
					{"actionType", "NONE"}
				}},
				{"button2", new Dictionary<string, object>() {
					{"x", 205},
					{"y", 30},
					{"imgX", 200},
					{"imgY", 280},
					{"width", 48},
					{"height", 24},
					{"label", "Ignore"},
					{"name", "cancel"},
					{"actionType", "NONE"}
				}}
			};

			ImageComposition c = ImageComposition.BuildFromDictionary(imgDict);

			Assert.IsNull(c);
		}
	}
}

