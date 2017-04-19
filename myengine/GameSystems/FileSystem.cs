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

		public bool FileExists(string virtualPath, FolderExisting startSearchInFolder)
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

		public bool TryFindFile(string virtualPath, out FileExisting file)
		{
			var realPath = CombineDirectory(rootResourceDirectoryPath, virtualPath);
			if (GlobSearch.IsNeeded(virtualPath))
			{
				var fileInfo = GlobSearch.FindFile(realPath);
				if (fileInfo == null)
				{
					file = null;
					return false;
				}
				else
				{
					file = new FileExisting(this, fileInfo.FullName.RemoveFromBegin(rootResourceDirectoryInfo.FullName.Length), fileInfo.FullName);
					return true;
				}
			}
			else
			{
				if (System.IO.File.Exists(realPath))
				{
					file = new FileExisting(this, virtualPath, realPath);
					return true;
				}
			}
			file = null;
			return false;
		}

		public FileOptional FindOptionalFile(string virtualPath)
		{
			FileExisting file;
			bool exists = TryFindFile(virtualPath, out file);

			if (file == null)
			{
				return new FileOptional(this, virtualPath, GetPhysicalPath(virtualPath), exists);
			}
			else
			{
				return new FileOptional(this, file.VirtualPath, file.RealPath, exists);
			}
		}

		public FileExisting FindExistingFile(string virtualPath)
		{
			FileExisting file;
			if (TryFindFile(virtualPath, out file)) return file;
			throw new FileNotFoundException("File " + virtualPath + " doesnt exits");
		}

		public List<FileExisting> Findfiles(params string[] virtualPaths)
		{
			var ret = new List<FileExisting>();
			foreach (var p in virtualPaths)
			{
				ret.Add(FindExistingFile(p));
			}
			return ret;
		}

		public FileExisting FindFile(string virtualPath, FolderExisting startSearchInFolder)
		{
			var realPath = CombineDirectory(rootResourceDirectoryPath, startSearchInFolder.VirtualPath, virtualPath);
			if (System.IO.File.Exists(realPath))
			{
				return new FileExisting(this, CombineDirectory(startSearchInFolder.VirtualPath, virtualPath), realPath);
			}
			else
			{
				realPath = CombineDirectory(rootResourceDirectoryPath, virtualPath);
				if (System.IO.File.Exists(realPath))
				{
					return new FileExisting(this, virtualPath, realPath);
				}
				else
				{
					throw new FileNotFoundException("File " + CombineDirectory(startSearchInFolder.VirtualPath, virtualPath) + " doesnt exits");
				}
			}
		}

		public FolderExisting GetFolder(FileExisting file)
		{
			var virtualDir = Path.GetDirectoryName(file.VirtualPath);
			return new FolderExisting(virtualDir);
		}
	}
}