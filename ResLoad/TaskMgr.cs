using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;


namespace ResLoad
{
	class TaskMgr
	{
		#region 单例
		private static TaskMgr ins = null;
		public static TaskMgr Ins
		{
			get
			{
				if (ins == null)
				{
					ins = new TaskMgr();
					ins.Init();
				}
				return ins;
			}
		}
		#endregion


		#region 初始化
		private void Init()
		{
			LoadResInfos ();
		}

		private Dictionary<string, ResInfo> m_ResInfoDic = null;
        //测试用
        public Dictionary<string, ResInfo> ResInfoDic
        {
            get
            {
                return m_ResInfoDic;
            }
        }

		/// <summary>
		/// 加载打包后存有各资源信息的Txt文件
		/// </summary>
		private void LoadResInfos()
		{
			m_ResInfoDic = new Dictionary<string, ResInfo>();

			FileStream fs = new FileStream(GlobalSetting.PackedTxtPath, FileMode.Open);
			StreamReader sr = new StreamReader(fs);
			string lineStr;
			while ((lineStr = sr.ReadLine()) != null)
			{
				if(!lineStr.Equals(string.Empty))
				{
					string[] items =lineStr.Split('\t');
					if(items.Length != 3)
					{
						ConsoleMgr.LogRed("错误");
					}
					else
					{
						ResInfo tempInfo = new ResInfo();
						tempInfo.StartPos = Convert.ToInt32(items[1]);
						tempInfo.Size = Convert.ToInt32(items[2]);
						m_ResInfoDic.Add(items[0],tempInfo);
					}
				}
			}
		}
		#endregion


		#region 数据
		/// <summary>
		/// 处于等待执行状态的任务队列
		/// </summary>
		public Queue<TaskInfo> m_AllTaskQueue = new Queue<TaskInfo>();
		/// <summary>
		/// 锁
		/// </summary>
		private static object AllTaskQueueLockObj = new object();

		/// <summary>
		/// 线程池
		/// </summary>
		private List<Thread> m_ThreadPools = new List<Thread>();
		#endregion


		#region Function
		public void Load(TaskInfo tempTaskInfo)
		{
			lock (AllTaskQueueLockObj)
            {
                AddToATask(tempTaskInfo);
				CreateTaskThread();
            }
		}

		private void AddToATask(TaskInfo tempTaskInfo)
		{
            m_AllTaskQueue.Enqueue(tempTaskInfo);
		}

		/// <summary>
		/// 创建任务线程
		/// </summary>
		private void CreateTaskThread()
		{
			try
			{
				if(m_AllTaskQueue.Count < 1)	return;

				if (m_ThreadPools.Count < GlobalSetting.MThreadMaxCount)
				{
					Thread thread = new Thread(new ParameterizedThreadStart(ThreadFunc));
					thread.Name = "线程" + m_ThreadPools.Count.ToString();
					TaskInfo info = m_AllTaskQueue.Dequeue();
					info.threadInfo = new ThreadInfo();
					info.threadInfo.isDoingTask = false;
					info.threadInfo.thread = thread;
					//给方法传值,启动线程
					thread.Start(info);

					m_ThreadPools.Add(thread);
				}
			}
			catch(Exception e)
			{
				ConsoleMgr.LogRed(e.ToString());
			}
		}

		public void ThreadFunc(object obj)
		{
			TaskInfo tempInfo = (TaskInfo)obj;

            tempInfo.threadInfo.isDoingTask = true;
            //Load Resource
            LoadItem(tempInfo);

            while (true)
            {
				lock (AllTaskQueueLockObj)
                {
                    //如果当前线程没在执行任务,还有工作未完成的话,交给当前线程执行
                    if (!tempInfo.threadInfo.isDoingTask)
                    {
                        if (m_AllTaskQueue.Count > 0)
                        {
                            TaskInfo nextInfo = null;
                            nextInfo = m_AllTaskQueue.Dequeue();
                            nextInfo.threadInfo = new ThreadInfo();
                            nextInfo.threadInfo.isDoingTask = false;
                            nextInfo.threadInfo.thread = tempInfo.threadInfo.thread;
                            nextInfo.threadInfo.isDoingTask = true;

                            //Load Resource
                            LoadItem(nextInfo);

                            tempInfo = nextInfo;
                        }
                    }
                    //如果当前线程正在执行任务
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
            }
		}
		#endregion


		#region 从合并后的大文件中获取其中的某个小文件
        private static object IoLockObj = new object();
		private static FileStream fs = null;
		private void LoadItem(TaskInfo nextInfo)
		{
            try
            {
				lock (IoLockObj)
                {
//					FileStream fs = new FileStream(GlobalSetting.PackedFilePath, FileMode.Open);//这里后期可能会优化,因为游戏很可能要一直不断加载资源,这个fs可考虑不用临时,只创建一个
					if(fs == null)
					{
						fs = new FileStream(GlobalSetting.PackedFilePath, FileMode.Open);
					}

					//Seek索引默认从0开始(注意,不是从1开始)
                    fs.Seek(m_ResInfoDic[nextInfo.resName].StartPos, SeekOrigin.Begin);

                    byte[] datas = new byte[m_ResInfoDic[nextInfo.resName].Size];// 要读取的内容会放到这个数组里
                    fs.Read(datas, 0, datas.Length);// 开始读取，读取的内容放到datas数组里，0是从第一个开始放，datas.length是最多允许放多少个

//                    fs.Flush();
//                    fs.Close();

                    nextInfo.threadInfo.isDoingTask = false;

                    if (nextInfo.resLoadedCallBack != null)
                    {
						nextInfo.resLoadedCallBack(nextInfo, datas);
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
		}
		#endregion
	}

	/// <summary>
	/// 任务信息
	/// </summary>
	public class TaskInfo
	{
		public bool isFinished = false;                 //该文件是否统计完成
		public bool isSave = false;    					//该文件是否要缓存
		public string resName = string.Empty;          	//要加载的资源名
		public ResLoadedAction resLoadedCallBack;       //任务完成的回调
		public ThreadInfo threadInfo;
	}

	/// <summary>
	/// 线程信息
	/// </summary>
	public class ThreadInfo
	{
		public Thread thread;
		/// <summary>
		/// 当前线程是否正在执行任务
		/// </summary>
		public bool isDoingTask = false;
	}

	/// <summary>
	/// 要加载的资源文件信息
	/// </summary>
	public class ResInfo
	{
		public int StartPos;        	//资源存放的起始位置
		public int Size;            	//资源大小(字节)
	}
}
