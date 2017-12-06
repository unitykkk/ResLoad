using System;
using System.Collections.Generic;
using System.IO;
using System.Text;



namespace ResLoad
{
	/// <summary>
	/// 资源包中文件信息的解读器
	/// </summary>
	public class FilePackInfoReader
	{
		public static void Read(ref Dictionary<string, PackageInfo> packageInfosDic, ref Dictionary<string, PackedFileInfo> filesPackInfoDic, string packagePath)
		{
			try
			{
				FileStream fs = new FileStream(packagePath, FileMode.Open);

				PackageInfo tempPackageInfo = new PackageInfo ();

				//1.资源包信息区域
				//1.1资源包版本（int)
				byte[] resVersionData = new byte[4];
				fs.Read (resVersionData, 0, 4);
				tempPackageInfo.Version = MyBinaryReader.ReadInt (resVersionData);
				resVersionData = null;
				//1.2读取资源包类型名字节大小(ushort)，占2个字节
				byte[] resTypeNameSizeData = new byte[2];
				fs.Read (resTypeNameSizeData, 0, 2);
				ushort resTypeNameSize = MyBinaryReader.ReadUshort (resTypeNameSizeData);
				resTypeNameSizeData = null;
				//1.3读取资源包类型名称
				byte[] resTypeNameData = new byte[resTypeNameSize];
				fs.Read (resTypeNameData, 0, resTypeNameSize);
				tempPackageInfo.TypeName = MyBinaryReader.ReadString (resTypeNameData);
				resTypeNameData = null;
				//1.4读取资源包总共字节大小(uint)
				byte[] resTotalSizeData = new byte[4];
				fs.Read (resTotalSizeData, 0, 4);
				tempPackageInfo.TotalSize = MyBinaryReader.ReadUint(resTotalSizeData);
				resTotalSizeData = null;
				//获取资源包名称
				FileInfo resInfo = new FileInfo(packagePath);
				string resName = resInfo.Name.Split ('.')[0];
				packageInfosDic.Add (resName, tempPackageInfo);

				//2.文件信息集合区域
				//2.1文件信息集合所占字节大小（int）
				byte[] infoRegionSizeData = new byte[4];		
				fs.Read(infoRegionSizeData, 0, 4);
				int infoRegionSize = MyBinaryReader.ReadInt(infoRegionSizeData);
				infoRegionSizeData = null;
				//2.2文件信息集合里的文件信息个数（int)
				byte[] filesCountData = new byte[4];
				fs.Read (filesCountData, 0, 4);
				int filesCount = MyBinaryReader.ReadInt (filesCountData);
				filesCountData = null;

				int fileInfosRegionSize = 4 + 4;
				for (int n = 0; n < filesCount; n++) 
				{
					PackedFileInfo tempInfo = new PackedFileInfo();
					//2.3.1文件名字节大小（ushort)
					byte[] nameLengthData = new byte[2];
					fs.Read(nameLengthData, 0, 2);
					ushort nameLength = MyBinaryReader.ReadUshort(nameLengthData);
					fileInfosRegionSize += 2;
					nameLengthData = null;
					//2.3.2文件名(UTF8)
					byte[] nameData = new byte[nameLength];
					fs.Read(nameData, 0, nameLength);
					string fileName = MyBinaryReader.ReadString (nameData);
					fileInfosRegionSize += nameLength;
					nameData = null;
					//2.3.3文件起始位置(uint)
					byte[] startPosData = new byte[4];
					fs.Read(startPosData, 0, 4);
					tempInfo.StartPos = MyBinaryReader.ReadUint(startPosData);
					fileInfosRegionSize += 4;
					startPosData = null;

					//2.3.4文件大小(int)
					byte[] sizeData = new byte[4];
					fs.Read(sizeData, 0, 4);
					tempInfo.Size = MyBinaryReader.ReadInt(sizeData);
					fileInfosRegionSize += 4;
					sizeData = null;

					tempInfo.PackageName = resName;

					if (!filesPackInfoDic.ContainsKey (fileName)) 
					{
						filesPackInfoDic.Add (fileName, tempInfo);
					} 
					else 
					{
						ConsoleMgr.LogRed ("有重复文件名:" + fileName);
					}
				}

				if (fileInfosRegionSize != infoRegionSize) 
				{
					ConsoleMgr.LogRed ("错误：文件信息集合区域的字节大小计算有误!");
				}

				fs.Flush();
				fs.Close();
			}
			catch(Exception e) 
			{
				ConsoleMgr.LogRed(e.ToString());
			}
		}
	}
}

