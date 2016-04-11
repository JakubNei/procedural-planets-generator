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
        List<Thread> watcherThreads = new List<Thread>();


        public void WatchFile(ISynchronizeInvoke synchronizeInvoke, string filePath, Action<string> callBack)
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
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
            watcher.SynchronizingObject = synchronizeInvoke;
            watcher.EnableRaisingEvents = true;
        }


        void StopAllWatchers()
        {
            foreach (var w in watcherThreads)
            {
                w.Abort();
            }
            watcherThreads.Clear();
        }
    }
}
