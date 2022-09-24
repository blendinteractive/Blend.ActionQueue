using System;
using System.Collections.Concurrent;
using System.Threading;

namespace BlendInteractive.ActionQueue
{
    public abstract class AbstractActionQueue<T>
    {
        private readonly BlockingCollection<T> Queue = new BlockingCollection<T>();
        private Thread queueThread;
        private CancellationToken token;

        protected abstract void LogException(Exception ex);
        protected abstract void ProcessItem(T item);

        public AbstractActionQueue(CancellationToken token)
        {
            this.token = token;
        }

        void StartQueueIfNotStarted()
        {
            if (queueThread == null || !queueThread.IsAlive)
            {
                queueThread = new Thread(QueueLoop);

                queueThread.Start();
            }
        }

        void QueueLoop()
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var actions = Queue.GetConsumingEnumerable(token);
                    foreach (var action in actions)
                    {
                        ProcessItem(action);
                    }
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
            }
        }

        public T QueueAction(T action)
        {
            StartQueueIfNotStarted();
            Queue.Add(action);
            return action;
        }
    }
}
