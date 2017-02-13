using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace MyEngine
{
	public class MyFile
	{
		public MyFolder Folder
		{
			get
			{
				return FileSystem.GetFolder(this);
			}
		}
		public FileSystem FileSystem { get; private set; }
		public string VirtualPath { get; private set; }
		public bool HasRealPath
		{
			get
			{
				return string.IsNullOrWhiteSpace(RealPath) == false;
			}
		}
		public string RealPath { get; private set; }

		FileChangedWatcher fileWatcher;
		event Action onFileChanged;

		public MyFile(FileSystem FileSystem, string virtualPath, string realPath)
		{
			this.FileSystem = FileSystem;
			this.VirtualPath = virtualPath;
			this.RealPath = realPath;
		}

		public Stream GetDataStream()
		{
			int numTries = 0;
			while (numTries++ < 10)
			{
				try
				{
					return new FileStream(RealPath, FileMode.Open, FileAccess.Read, FileShare.Read);
				}
				catch
				{
					System.Threading.Thread.Sleep(numTries);
				}
			}
			return null;
		}

		public void OnFileChanged(Action action)
		{
			if (fileWatcher == null)
			{
				fileWatcher = new FileChangedWatcher();
				fileWatcher.WatchFile(RealPath, (newFileName) => onFileChanged.Raise());
			}
			onFileChanged += action;
		}

		public override string ToString()
		{
			return VirtualPath;
		}
	}
}
