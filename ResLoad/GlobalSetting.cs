using System;

namespace ResLoad
{
	public class GlobalSetting
	{
		/// <summary>
		/// 要打包的文件夹
		/// </summary>
		public static string PackFolderPath = @"/Users/huzhen/Desktop/ResPack/Src";
		/// <summary>
		/// 打包完成后的文件
		/// </summary>
		public static string PackedFilePath = @"/Users/huzhen/Desktop/ResPack/total.bin";
		/// <summary>
		/// 打包完成后存有各资源信息的Txt文档
		/// </summary>
		public static string PackedTxtPath = @"/Users/huzhen/Desktop/ResPack/total.txt";

		/// <summary>
		/// 允许创建线程的最大数量
		/// </summary>
		public const int MThreadMaxCount = 2;
	}
}

