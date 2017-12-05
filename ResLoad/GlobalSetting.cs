using System;

namespace ResLoad
{
	public class GlobalSetting
	{
		/// <summary>
		/// 要打包的源文件夹
		/// </summary>
		public static string PackFromFolderPath = @"/Users/huzhen/Desktop/ResPack/FromFolder";
		/// <summary>
		/// 打包完成后的资源包所存放的文件夹
		/// </summary>
		public static string PackToFolderPath = @"/Users/huzhen/Desktop/ResPack/ToFolder";

		/// <summary>
		/// 允许创建线程的最大数量
		/// </summary>
		public const int MThreadMaxCount = 3;
	}
}

