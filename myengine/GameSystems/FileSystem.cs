using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MyEngine
{
	public class FileSystem : GameSystemBase
	{
		string rootResourceDirectoryPath;
		DirectoryInfo rootResourceDirectoryInfo;

		public Dictionary<string, Type> extensionToTypeAssociation = new Dictionary<string, Type>()
		{
			{"obj", typeof(Mesh)},
			{"glsl", typeof(Shader)},
			{"shader", typeof(Shader)},
		};

		public FileSystem(string rootResourceDirectoryPath)
		{
			this.rootResourceDirectoryInfo = new DirectoryInfo(rootResourceDirectoryPath);
			if (!rootResourceDirectoryInfo.Exists) throw new Exception(rootResourceDirectoryInfo + ", root resource folder does not exist");
			this.rootResourceDirectoryPath = rootResourceDirectoryInfo.FullName;
		}

		public string CombineDirectory(params string[] pathParts)
		{
			return pathParts.SelectMany(p => p.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries)).Join(System.IO.Path.DirectorySeparatorChar);
		}



		public bool FileExists(string virtualPath)
		{
			var realPath = CombineDirectory(rootResourceDirectoryPath, virtualPath);
			return System.IO.File.Exists(realPath);
		}

		public bool FileExists(string virtualPath, MyFolder startSearchInFolder)
		{
			var realPath = CombineDirectory(rootResourceDirectoryPath, startSearchInFolder.VirtualPath, virtualPath);
			if (System.IO.File.Exists(realPath))
			{
				return true;
			}
			else
			{
				if (System.IO.File.Exists(realPath))
				{
					return true;
				}
			}
			return false;
		}


		public string GetPhysicalPath(string virtualPath)
		{
			var realPath = CombineDirectory(rootResourceDirectoryPath, virtualPath);
			return realPath;
		}

		public bool TryFindFile(string virtualPath, out MyFile file)
		{
			var realPath = CombineDirectory(rootResourceDirectoryPath, virtualPath);
			if (GlobSearch.IsNeeded(virtualPath))
			{
				var fileInfo = GlobSearch.FindFile(realPath);
				file = new MyFile(this, fileInfo.FullName.RemoveFromBegin(rootResourceDirectoryInfo.FullName.Length), fileInfo.FullName);
				return true;
			}
			else
			{
				if (System.IO.File.Exists(realPath))
				{
					file = new MyFile(this, virtualPath, realPath);
					return true;
				}
			}
			file = null;
			return false;
		}

		public MyFile FindFile(string virtualPath)
		{
            MyFile file;
            if (TryFindFile(virtualPath, out file)) return file;
			throw new FileNotFoundException("File " + virtualPath + " doesnt exits");
		}

		public List<MyFile> Findfiles(params string[] virtualPaths)
		{
			var ret = new List<MyFile>();
			foreach (var p in virtualPaths)
			{
				ret.Add(FindFile(p));
			}
			return ret;
		}

		public MyFile FindFile(string virtualPath, MyFolder startSearchInFolder)
		{
			var realPath = CombineDirectory(rootResourceDirectoryPath, startSearchInFolder.VirtualPath, virtualPath);
			if (System.IO.File.Exists(realPath))
			{
				return new MyFile(this, CombineDirectory(startSearchInFolder.VirtualPath, virtualPath), realPath);
			}
			else
			{
				realPath = CombineDirectory(rootResourceDirectoryPath, virtualPath);
				if (System.IO.File.Exists(realPath))
				{
					return new MyFile(this, virtualPath, realPath);
				}
				else
				{
					throw new FileNotFoundException("File " + CombineDirectory(startSearchInFolder.VirtualPath, virtualPath) + " doesnt exits");
				}
			}
		}

		public MyFolder GetFolder(MyFile file)
		{
			var virtualDir = Path.GetDirectoryName(file.VirtualPath);
			return new MyFolder(virtualDir);
		}
	}
}