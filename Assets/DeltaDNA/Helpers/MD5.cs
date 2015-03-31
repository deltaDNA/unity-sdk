namespace DeltaDNA
{

	/// <summary>
	/// Helps manage MD5 hashing algorithm across the Unity platforms.
	/// </summary>
	public class MD5
	{
	
		public static MD5 Create() 
		{
			return new MD5();
		}
		
		public byte[] ComputeHash(byte[] buffer)
		{
			#if NETFX_CORE

			return UnityEngine.Windows.Crypto.ComputeMD5Hash(buffer);
			
			#else 
			
			// Use MD5CryptoServiceProvider instead of MD5 class with iOS stripping level set to micro mscorlib.
			var md5Hasher = new System.Security.Cryptography.MD5CryptoServiceProvider();
			return md5Hasher.ComputeHash(buffer);
			
			#endif
		}
		
	}
}

