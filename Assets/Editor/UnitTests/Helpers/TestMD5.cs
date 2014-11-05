using System;
using System.Text;
using NUnit.Framework;
using DeltaDNA;

namespace DeltaDNA.Helpers
{
	[TestFixture]
	[Category("Helper Tests")]
	internal class TestMD5
	{
		[Test]
		public void MD5Tests()
		{
			Assert.That(getMd5Hash(""), Is.EqualTo("d41d8cd98f00b204e9800998ecf8427e"));
			Assert.That(getMd5Hash("a"), Is.EqualTo("0cc175b9c0f1b6a831c399e269772661"));
			Assert.That(getMd5Hash("abc"), Is.EqualTo("900150983cd24fb0d6963f7d28e17f72"));
			Assert.That(getMd5Hash("message digest"), Is.EqualTo("f96b697d7cb7938d525a2f31aaf161d0"));
			Assert.That(getMd5Hash("abcdefghijklmnopqrstuvwxyz"), Is.EqualTo("c3fcd3d76192e4007dfb496cca67e13b"));
			Assert.That(getMd5Hash("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"), Is.EqualTo("d174ab98d277d9f5a5611c2c9f419d9f"));
			Assert.That(getMd5Hash("12345678901234567890123456789012345678901234567890123456789012345678901234567890"), Is.EqualTo("57edf4a22be3c955ac49da2e2107b67a"));
		}

		// Hash an input string and return the hash as
		// a 32 character hexadecimal string.
		private static string getMd5Hash(string input)
		{
			// Create a new instance of the MD5CryptoServiceProvider object.
			MD5 md5Hasher = MD5.Create();
			
			// Convert the input string to a byte array and compute the hash.
			byte[] data = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(input));
			
			// Create a new Stringbuilder to collect the bytes
			// and create a string.
			StringBuilder sBuilder = new StringBuilder();
			
			// Loop through each byte of the hashed data 
			// and format each one as a hexadecimal string.
			for (int i = 0; i < data.Length; i++)
			{
				sBuilder.Append(data[i].ToString("x2"));
			}
			
			// Return the hexadecimal string.
			return sBuilder.ToString();
		}
	}
}

