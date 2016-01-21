using System;

namespace PemaChip8
{
	public static class Util
	{
		public static string ToHex(this int Num)
		{
			return Num.ToString("X4");
		}

		public static string ToHex(this short Num)
		{
			return Num.ToString("X4");
		}

		public static string ToHex(this byte Num)
		{
			return Num.ToString("X4");
		}

		public static int ToInt(this string Hex)
		{
			return int.Parse(Hex, System.Globalization.NumberStyles.HexNumber);
		}

		public static short ToShort(this string Hex)
		{
			return short.Parse(Hex, System.Globalization.NumberStyles.HexNumber);
		}

		public static byte ToByte(this string Hex)
		{
			return byte.Parse(Hex, System.Globalization.NumberStyles.HexNumber);
		}

		public static bool GetBit(this byte b, int bitNumber) 
		{
			return (b & (1 << 7-bitNumber)) != 0;
		}
	}
}

