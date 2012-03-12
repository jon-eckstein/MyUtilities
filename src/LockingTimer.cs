using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;

namespace MyUtilities
{
    public class LockingTimer
    {
        private object locker = new object();
        private System.Timers.Timer innerTimer;
        private int milliTimeout;
        private Action onElapsed;
        private Action onTimeout;
        //public event ElapsedEventHandler Elapsed;

        public LockingTimer(double milliElapsed, int milliTimeout, Action onElapsed, Action onTimeout)
        {
            this.milliTimeout = milliTimeout;
            this.onElapsed = onElapsed;
            this.onTimeout = onTimeout;
            innerTimer = new System.Timers.Timer(milliElapsed);
            InitTimer();

        }

        private void InitTimer()
        {
            innerTimer.Elapsed += timer_Elapsed;
            innerTimer.Enabled = true;
            innerTimer.Start();
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Monitor.TryEnter(locker, milliTimeout))
            {
                try
                {
                    onElapsed();
                }
                finally
                {
                    Monitor.Exit(locker);
                }
            }
            else
            {
                onTimeout();
            }
        }

        public void Stop()
        {
            innerTimer.Stop();
        }
    }
}
