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
		/// <summary>
		/// 资源包信息集合(key是资源包名)
		/// </summary>
		private Dictionary<string, PackageInfo> m_PackageInfosDic = new Dictionary<string, PackageInfo>();
		/// <summary>
		/// 所有资源包里面的文件信息集合(key是文件的相对路径名)
		/// </summary>
		private Dictionary<string, PackedFileInfo> m_FilesPackInfoDic = new Dictionary<string, PackedFileInfo>();

		/// <summary>
		/// 所有被打包文件的文件名(用的是相对路径)列表
		/// </summary>
		/// <value>The packed file names.</value>
		public List<string> PackedFileNames
		{
			get
			{
				return new List<string> (m_FilesPackInfoDic.Keys);
			}
		}

		/// <summary>
		/// 所有被打包的文件数量
		/// </summary>
		/// <value>The pack files count.</value>
		public int PackFilesCount
		{
			get
			{
				return m_FilesPackInfoDic.Count;
			}
		}
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
					PackedFileInfoReader.Read(ref m_PackageInfosDic, ref m_FilesPackInfoDic, resPaths[n]);
				}
			}
		}
		#endregion


		#region 提供给外部的接口
		private Dictionary<string, FileStream> m_FileStreamDic = new Dictionary<string, FileStream> ();
		private static object DataLockObj = new object ();

		/// <summary>
		/// 从资源包中加载某一个文件的完整数据
		/// </summary>
		/// <returns>该文件的完整数据</returns>
		/// <param name="fileName">要加载的文件名（相对路径）</param>
		public byte[] Load(string fileName)
		{
			lock (DataLockObj) 
			{
				try 
				{
					FileStream fs = GetPackageFileStream (fileName);

					//Seek索引默认从0开始(注意,不是从1开始)
					fs.Seek (m_FilesPackInfoDic [fileName].StartPos, SeekOrigin.Begin);

					byte[] datas = new byte[m_FilesPackInfoDic [fileName].Size];		// 要读取的内容会放到这个数组里
					fs.Read (datas, 0, datas.Length);									// 开始读取，读取的内容放到datas数组里，0是从第一个开始放，datas.length是最多允许放多少个
					return datas;

				} 
				catch (Exception e) 
				{
					Console.WriteLine (e.ToString ());
					return null;
				}
			}
		}

		/// <summary>
		/// 从资源包中加载某一个文件的内容片断
		/// </summary>
		/// <param name="fileName">要加载的文件名(相对路径）</param>
		/// <param name="buffer">加载后数据的存放数组</param>
		/// <param name="beginPos">在该文件中的起始位置（从0开始）</param>
		/// <param name="size">要读取该文件内容片断的字节大小</param>
		public void LoadFragment(string fileName, ref byte[] buffer, int beginPos, int size)
		{
			lock (DataLockObj)
			{
				try 
				{
					FileStream fs = GetPackageFileStream (fileName);

					//Seek索引默认从0开始(注意,不是从1开始)
					long newStartPos = m_FilesPackInfoDic [fileName].StartPos + beginPos;
					fs.Seek (newStartPos, SeekOrigin.Begin);

					fs.Read (buffer, 0, size);
				} 
				catch (Exception e) 
				{
					Console.WriteLine (e.ToString ());
				}
			}
		}

		/// <summary>
		/// 获取资源包信息
		/// </summary>
		/// <returns>资源包信息</returns>
		/// <param name="packageName">资源包名</param>
		public PackageInfo GetPackageInfo(string packageName)
		{
			if (m_PackageInfosDic.ContainsKey (packageName)) 
			{
				return m_PackageInfosDic [packageName];
			}

			return null;
		}
		#endregion


		#region Other
		/// <summary>
		/// 获取某个文件所在资源包的FileStream
		/// </summary>
		/// <returns>资源包的FileStream</returns>
		/// <param name="fileName">文件名</param>
		private FileStream GetPackageFileStream(string fileName)
		{
			if(m_FileStreamDic.ContainsKey(m_FilesPackInfoDic[fileName].PackageName))
			{
				return m_FileStreamDic[m_FilesPackInfoDic[fileName].PackageName];
			}
			else
			{
				string packagePath = GlobalSetting.PackToFolderPath + @"/" + m_FilesPackInfoDic[fileName].PackageName + GlobalSetting.PackageExtension;
				FileStream tempFs = new FileStream(packagePath, FileMode.Open, FileAccess.Read);
				m_FileStreamDic.Add(m_FilesPackInfoDic[fileName].PackageName, tempFs);

				return tempFs;
			}
		}

		/// <summary>
		/// 释放内存及对资源包FileStream的控制
		/// </summary>
		public void Release()
		{
			try
			{
				m_PackageInfosDic.Clear ();
				m_FilesPackInfoDic.Clear ();

				List<string> packageNamesList = new List<string> (m_FileStreamDic.Keys);
				for (int n = 0; n < packageNamesList.Count; n++) 
				{
					FileStream tempStream = m_FileStreamDic [packageNamesList [n]];
					tempStream.Flush ();
					tempStream.Close ();
					tempStream = null;
				}
				m_FileStreamDic.Clear ();

				packageNamesList.Clear ();
				packageNamesList = null;

				m_PackageInfosDic = null;
				m_FilesPackInfoDic = null;
				m_FileStreamDic = null;
			}
			catch(Exception e) 
			{
				ConsoleMgr.LogRed (e.ToString ());
			}
		}
		#endregion
	}

	/// <summary>
	/// 文件打包后的信息
	/// </summary>
	public class PackedFileInfo
	{
		public string PackageName;						//文件在哪个资源包中
		public long StartPos;        					//文件在资源包中存放的起始位置(实际用的是uint存储)
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

