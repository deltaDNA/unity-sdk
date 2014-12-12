using System;
using System.Globalization;
using NUnit.Framework;
using UnityEngine;

namespace DeltaDNA
{
	[TestFixture]
	[Category("Utility Tests")]
	internal class Timestamp
	{
		[Test]
		public void MillisecondsTest()
		{
			// Confirm timestamp format of yyyy-MM-dd HH:mm:ss.fff does what we expect
			// Bug PTL-925 suggests a value of .1000 ms can occur.

			CultureInfo ci = CultureInfo.InvariantCulture;

			for (int i = 0; i < 1000; ++i) {
				DateTime date1 = new DateTime(2008, 8, 29, 19, 27, 15, i);
				string timestamp = date1.ToString("yyyy-MM-dd HH:mm:ss.fff", ci);
				//Debug.Log("DATE:"+timestamp);
				Assert.That(timestamp.Length, Is.EqualTo(23));
			}
		}
	}
}

