/*
    Implementation of ISynchronizeInvoke for Unity3D game engine.
    Can be used to invoke anything on main Unity thread.
    ISynchronizeInvoke is used extensively in .NET forms it's is elegant and quite useful in Unity as well.
    I implemented it so i can use it with System.IO.FileSystemWatcher.SynchronizingObject.

    help from: http://www.codeproject.com/Articles/12082/A-DelegateQueue-Class
    example usage: https://gist.github.com/aeroson/90bf21be3fdc4829e631

    license: WTFPL (http://www.wtfpl.net/)
    contact: aeroson (theaeroson @gmail.com)
*/

using System.Collections;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace Neitri
{
    public class DeferredSynchronizeInvoke : ISynchronizeInvoke
    {
        public class Owner
        {
            public Thread MainThread { get; private set; }
            List<DeferredSynchronizeInvoke> ownedDeferredSynchronizeInvoke = new List<DeferredSynchronizeInvoke>();
            public Owner()
            {
                MainThread = Thread.CurrentThread;
            }
            public void ProcessQueue()
            {
                if (Thread.CurrentThread != MainThread)
                {
                    throw new TargetException(
                        this.GetType() + "." + MethodBase.GetCurrentMethod().Name + "() " +
                        "must be called from the same thread it was created on " +
                        "(created on thread id: " + MainThread.ManagedThreadId + ", called from thread id: " + Thread.CurrentThread.ManagedThreadId
                    );
                }
                foreach (var d in ownedDeferredSynchronizeInvoke)
                {
                    d.ProcessQueue();
                }
            }
        }


        Queue<AsyncResult> queueToExecute = new Queue<AsyncResult>();
        Owner owner;
        public bool InvokeRequired { get { return owner.MainThread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId; } }

        public DeferredSynchronizeInvoke(Owner owner)
        {
            this.owner = owner;
        }
        public IAsyncResult BeginInvoke(Delegate method, object[] args)
        {
            var asyncResult = new AsyncResult()
            {
                method = method,
                args = args,
                IsCompleted = false,
                AsyncWaitHandle = new ManualResetEvent(false),
            };
            lock (queueToExecute)
            {
                queueToExecute.Enqueue(asyncResult);
            }
            return asyncResult;
        }
        public object EndInvoke(IAsyncResult result)
        {
            if (!result.IsCompleted)
            {
                result.AsyncWaitHandle.WaitOne();
            }
            return result.AsyncState;
        }
        public object Invoke(Delegate method, object[] args)
        {
            if (InvokeRequired)
            {
                var asyncResult = BeginInvoke(method, args);
                return EndInvoke(asyncResult);
            }
            else
            {
                return method.DynamicInvoke(args);
            }
        }
        void ProcessQueue()
        {
            bool loop = true;
            AsyncResult data = null;
            while (loop)
            {
                lock (queueToExecute)
                {
                    loop = queueToExecute.Count > 0;
                    if (!loop) break;
                    data = queueToExecute.Dequeue();
                }

                data.AsyncState = Invoke(data.method, data.args);
                data.IsCompleted = true;
                (data.AsyncWaitHandle as ManualResetEvent).Set();
            }
        }
        class AsyncResult : IAsyncResult
        {
            public Delegate method;
            public object[] args;
            public bool IsCompleted { get; set; }
            public WaitHandle AsyncWaitHandle { get; internal set; }
            public object AsyncState { get; set; }
            public bool CompletedSynchronously { get { return IsCompleted; } }
        }
    }
}