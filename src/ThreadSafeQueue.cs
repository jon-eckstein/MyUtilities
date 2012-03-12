using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MyUtilities
{
    public class ThreadSafeQueue<T>
    {
        private Queue<T> innerQueue;
        private static object queueLocker = new object();
        private int defaultTimeout = 500;


        public ThreadSafeQueue()
        {
            innerQueue = new Queue<T>();
        }


        public bool TryEnqueue(T item, int millisecondsTimeout)
        {
            if (Monitor.TryEnter(queueLocker, millisecondsTimeout))
            {
                try
                {
                    innerQueue.Enqueue(item);
                }
                finally
                {
                    Monitor.Exit(queueLocker);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryEnqueue(T item)
        {
            return TryEnqueue(item, defaultTimeout);
        }

        public bool TryDequeue(int millisecondsTimeout, out T item)
        {
            item = default(T);
            if (Monitor.TryEnter(queueLocker, millisecondsTimeout))
            {
                try
                {
                    item = innerQueue.Dequeue();
                }
                finally
                {
                    Monitor.Exit(queueLocker);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryDequeue(out T item)
        {
            return TryDequeue(defaultTimeout, out item);
        }

        public bool HasItems()
        {
            if (innerQueue.Count > 0)
                return true;
            else
                return false;
        }

        public int Count
        {
            get { return innerQueue.Count; }
        }

    }
}
