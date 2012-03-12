using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyUtilities
{
    public class SystemClock : IDisposable
    {
        private static DateTime? now;

        public static DateTime Now
        {
            get { return now ?? DateTime.Now; }
        }

        public static IDisposable SetNow(DateTime dateTime)
        {
            now = dateTime;
            return new SystemClock();
        }

        public void Dispose()
        {
            now = null;
        }
    };
}
