using System;

namespace ResLoad
{
	/// <summary>
	/// 二进制内容解读器
	/// </summary>
	public class MyBinaryReader
	{
		public static ushort ReadUshort(byte[] datas)
		{
			return BitConverter.ToUInt16 (datas, 0);
		}

		public static string ReadString(byte[] datas)
		{
			string str = System.Text.Encoding.UTF8.GetString (datas);
			return str;
		}

		public static uint ReadUint(byte[] datas)
		{
			return BitConverter.ToUInt32 (datas, 0);
		}

		public static int ReadInt(byte[] datas)
		{
			return BitConverter.ToInt32 (datas, 0);
		}
	}
}

