using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;


namespace ResLoad
{
	public class PackedFileMgr
	{
		#region 单例
		private static PackedFileMgr _ins = null;
		public static PackedFileMgr Ins
		{
			get
			{
				if (_ins == null) 
				{
					_ins = new PackedFileMgr ();
					_ins.Init ();
				}

				return _ins;
			}
		}
		#endregion


		#region Data
		private Dictionary<string, PackageInfo> m_PackedResInfosDic = new Dictionary<string, PackageInfo>();

		private Dictionary<string, PackedFileInfo> m_FilesPackInfoDic = new Dictionary<string, PackedFileInfo>();

		public List<string> PackedFileNames
		{
			get
			{
				return new List<string> (m_FilesPackInfoDic.Keys);
			}
		}

		public int PackFilesCount
		{
			get
			{
				return m_FilesPackInfoDic.Count;
			}
		}

		private Dictionary<string, byte[]> m_LoadedDataDic = new Dictionary<string, byte[]> ();
		#endregion


		#region 初始化
		private void Init()
		{
			string[] resPaths = Directory.GetFiles (GlobalSetting.PackToFolderPath, "*", SearchOption.TopDirectoryOnly);
			for (int n = 0; n < resPaths.Length; n++) 
			{
				FileInfo tempResFileInfo = new FileInfo (resPaths [n]);
				if (tempResFileInfo.Extension.ToLower ().Equals (GlobalSetting.PackageExtension.ToLower())) 
				{
					PackedFileInfoReader.Read(ref m_PackedResInfosDic, ref m_FilesPackInfoDic, resPaths[n]);
				}
			}
		}
		#endregion


		#region 提供给外部的接口
		/// <summary>
		/// 加载文件内容
		/// </summary>
		/// <param name="fileName">要加载的文件名（相对路径）</param>
		/// <param name="isSave">If set to <c>true</c> 该文件加载后的内容是否需要缓存 </param>
		public byte[] Load(string fileName, bool isSave = false)
		{
			if (m_LoadedDataDic.ContainsKey (fileName)) 
			{
				byte[] returnData = m_LoadedDataDic [fileName];
				if (!isSave) 
				{
					m_LoadedDataDic.Remove (fileName);
				}

				return returnData;
			} 
			else 
			{
				byte[] fileData = LoadItem (fileName);
				if (isSave) 
				{
					m_LoadedDataDic [fileName] = fileData;
				}
				return fileData;
			}
		}

		/// <summary>
		/// 获取资源包信息
		/// </summary>
		/// <returns>The res package info.</returns>
		/// <param name="packageName">package name.</param>
		public PackageInfo GetPackageInfo(string packageName)
		{
			if (m_PackedResInfosDic.ContainsKey (packageName)) 
			{
				return m_PackedResInfosDic [packageName];
			}

			return null;
		}

		/// <summary>
		/// 获取打包后的文件信息
		/// </summary>
		/// <returns>文件打包后的信息</returns>
		/// <param name="resName">文件名</param>
		public PackedFileInfo GetFileInfo(string fileName)
		{
			if (m_FilesPackInfoDic.ContainsKey (fileName)) 
			{
				return m_FilesPackInfoDic [fileName];
			}

			return null;
		}
		#endregion

		/// <summary>
		/// 从资源包中加载某一个文件内容
		/// </summary>
		/// <returns>The item.</returns>
		/// <param name="fileName">File name.</param>
		private byte[] LoadItem(string fileName)
		{
			try
			{
				string packagePath = GlobalSetting.PackToFolderPath + @"/" + m_FilesPackInfoDic[fileName].PackageName + GlobalSetting.PackageExtension;
				FileStream fs = new FileStream(packagePath, FileMode.Open, FileAccess.Read);

				//Seek索引默认从0开始(注意,不是从1开始)
				fs.Seek(m_FilesPackInfoDic[fileName].StartPos, SeekOrigin.Begin);

				byte[] datas = new byte[m_FilesPackInfoDic[fileName].Size];// 要读取的内容会放到这个数组里
				fs.Read(datas, 0, datas.Length);// 开始读取，读取的内容放到datas数组里，0是从第一个开始放，datas.length是最多允许放多少个
				return datas;

			}
			catch(Exception e)
			{
				Console.WriteLine(e.ToString());
				return null;
			}
		}

	}

	/// <summary>
	/// 文件打包后的信息
	/// </summary>
	public class PackedFileInfo
	{
		public string PackageName;						//文件在哪个资源包中
		public long StartPos;        					//文件在资源包中存放的起始位置
		public int Size;            					//文件大小(字节)
	}

	/// <summary>
	/// 打包后资源包的信息
	/// </summary>
	public class PackageInfo
	{
		public int Version = 0;
		public string TypeName = string.Empty;
		public uint TotalSize = 0;
	}
}

