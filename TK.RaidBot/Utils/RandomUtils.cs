using System;
using System.Security.Cryptography;

namespace TK.RaidBot.Utils
{
	public class RandomUtils
	{
		private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

		public static string GetRandomString(int length)
		{
			var result = new char[length];

			using (var rng = new RNGCryptoServiceProvider())
			{
				for (int i = 0; i < result.Length; i++)
				{
					var bytes = new byte[4];
					rng.GetBytes(bytes);
					var value = BitConverter.ToUInt32(bytes, 0) % Characters.Length;

					result[i] = Characters[(int)(value % Characters.Length)];
				}
			}

			return new string(result);
		}
	}
}
