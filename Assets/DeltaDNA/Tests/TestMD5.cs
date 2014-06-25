using System;
using System.Text;
using UnityEngine;

namespace DeltaDNA
{
	public class TestMD5 : MonoBehaviour
	{
		// Hash an input string and return the hash as
		// a 32 character hexadecimal string.
		static string getMd5Hash(string input)
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
		
		// Verify a hash against a string.
		static bool verifyMd5Hash(string input, string hash)
		{
			// Hash the input.
			string hashOfInput = getMd5Hash(input);
			
			// Create a StringComparer an comare the hashes.
			StringComparer comparer = StringComparer.OrdinalIgnoreCase;
			
			if (0 == comparer.Compare(hashOfInput, hash))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		static void test(string input, string expected)
		{
			string hash = getMd5Hash (input);
			bool passed = verifyMd5Hash (input, expected);
			Debug.Log ("MD5(\""+input+"\") = "+hash+" - "+(passed?"PASSED":"FAILED"));
		}


		void Start()
		{
			test ("", "d41d8cd98f00b204e9800998ecf8427e");
			test ("a", "0cc175b9c0f1b6a831c399e269772661");
			test ("abc", "900150983cd24fb0d6963f7d28e17f72");
			test ("message digest", "f96b697d7cb7938d525a2f31aaf161d0");
			test ("abcdefghijklmnopqrstuvwxyz", "c3fcd3d76192e4007dfb496cca67e13b");
			test ("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", "d174ab98d277d9f5a5611c2c9f419d9f");
			test ("12345678901234567890123456789012345678901234567890123456789012345678901234567890", "57edf4a22be3c955ac49da2e2107b67a");
		}
	}
}

