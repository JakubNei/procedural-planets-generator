using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.IO;

namespace MyEngine
{
    public class FileChangedWatcher
    {
        FileSystemWatcher watcher;

        public void WatchFile(string filePath, Action<string> callBack, ISynchronizeInvoke synchronizeInvoke = null)
        {
            watcher = new FileSystemWatcher();
            watcher.Path = Path.GetDirectoryName(filePath);
            /* Watch for changes in LastAccess and LastWrite times, and  the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = Path.GetFileName(filePath);

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler((object o, FileSystemEventArgs a) => 
            {
                callBack.Raise(filePath);
            });
            //watcher.Created += new FileSystemEventHandler((object o, FileSystemEventArgs a) => { });
            //watcher.Deleted += new FileSystemEventHandler((object o, FileSystemEventArgs a) => { });
            watcher.Renamed += new RenamedEventHandler((object o, RenamedEventArgs a) =>
            {                
                callBack.Raise(a.FullPath);
            });
            if (synchronizeInvoke != null)
            {
                watcher.SynchronizingObject = synchronizeInvoke;
            }
            watcher.EnableRaisingEvents = true;
        }


        public void StopAllWatchers()
        {
            watcher.Dispose();
            watcher = null;
        }
    }
}
