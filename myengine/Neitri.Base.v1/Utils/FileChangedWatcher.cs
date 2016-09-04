using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.IO;

namespace Neitri.Base
{
    public class FileChangedWatcher
    {
        public delegate void FileChangedCallback(string newFilePath);

        List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();

        public void WatchFile(string filePath, FileChangedCallback callBack, ISynchronizeInvoke synchronizeInvoke = null)
        {
            var watcher = new FileSystemWatcher();
            watcher.Path = Path.GetDirectoryName(filePath);
            /* Watch for changes in LastAccess and LastWrite times, and  the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = Path.GetFileName(filePath);

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler((object o, FileSystemEventArgs a) => 
            {
                if (callBack != null) callBack(filePath);
            });
            //watcher.Created += new FileSystemEventHandler((object o, FileSystemEventArgs a) => { });
            //watcher.Deleted += new FileSystemEventHandler((object o, FileSystemEventArgs a) => { });
            watcher.Renamed += new RenamedEventHandler((object o, RenamedEventArgs a) =>
            {
                if (callBack != null) callBack(filePath);
            });
            if (synchronizeInvoke != null)
            {
                watcher.SynchronizingObject = synchronizeInvoke;
            }
            watcher.EnableRaisingEvents = true;
            watchers.Add(watcher);
        }


        public void StopAllWatchers()
        {
            foreach (var watcher in watchers)
            {
                watcher.Dispose();
            }
            watchers.Clear();
        }
    }
}
