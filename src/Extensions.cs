using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media;
using System.Windows;
using System.Reflection;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MyUtilities
{
    public static class Extensions
    {
        #region string conversions and helpers

        public static string FormatWith(this string template, params object[] args)
        {
            return String.Format(template, args);
        }

        public static byte[] ToBytes(this string data, Encoding encoding)
        {
            return encoding.GetBytes(data);
        }

        public static byte[] ToDefaultEncodedBytes(this string data)
        {
            return data.ToBytes(Encoding.Default);
        }

        public static bool HasValue(this String str)
        {
            if (!String.IsNullOrEmpty(str))
                return true;
            else
                return false;
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return String.IsNullOrEmpty(value);
        }

        public static bool Between<T>(this T source, T low, T high) where T : IComparable
        {
            return source.CompareTo(low) >= 0 && source.CompareTo(high) <= 0;
        }

        #endregion

        #region byte array conversion and helpers

        public static string ToDefaultEncodedString(this byte[] data)
        {
            return data.ToString(Encoding.Default);
        }

        public static string ToString(this byte[] data, Encoding encoding)
        {
            return encoding.GetString(data);
        }

        public static string ToHexString(this byte[] data, int n)
        {
            return BitConverter.ToUInt32(data, 0).ToString("X" + n);
        }

        public static string ToHexString(this byte[] data)
        {
            return ToHexString(data, 2);
        }


        public static int FindIndexOf(this byte[] bytes, byte[] pattern)
        {
            int patternLength = pattern.Length;
            int totalLength = bytes.Length;
            byte firstMatchByte = pattern[0];
            for (int i = 0; i < totalLength; i++)
            {
                if (firstMatchByte == bytes[i] && totalLength - i >= patternLength)
                {
                    byte[] match = new byte[patternLength];
                    Array.Copy(bytes, i, match, 0, patternLength);
                    if (match.SequenceEqual<byte>(pattern))
                        return i;
                }
            }
            return -1;
        }


        #endregion


        #region general converters

        public static float ToFloat(this string data)
        {
            data = data.Trim();
            float retVal;
            if (float.TryParse(data, out retVal))
                if (!float.IsNaN(retVal))
                    return retVal;

            return default(float);
        }


        public static int ToInt32(this string data)
        {
            int retVal;
            if (Int32.TryParse(data, out retVal))
                return retVal;

            return default(Int32);
        }

        public static string To1Or0String(this bool value)
        {
            if (value)
                return "1";
            else
                return "0";
        }

        public static void SetToZero<T>(this T[] arr)
        {
            for (int i = 0; i <= arr.Length; i++)
                arr[i] = default(T);
        }

        /// <summary>
        /// Returns as timespan in a formated days hours minutes seconds string
        /// </summary>
        /// <param name="time">The time span.</param>
        /// <returns>The time span string.</returns>
        public static string ToFormattedString(this TimeSpan time)
        {
            StringBuilder sb = new StringBuilder();
            if (time.Days != 0)
            {
                sb.Append(time.Days.ToString());
                sb.Append("d");
                sb.Append(time.Hours.ToString());
                sb.Append("h");
                sb.Append(time.Minutes.ToString());
                sb.Append("m");
            }
            else if (time.Hours != 0)
            {
                sb.Append(time.Hours.ToString());
                sb.Append("h");
                sb.Append(time.Minutes.ToString());
                sb.Append("m");
                sb.Append(time.Seconds.ToString());
                sb.Append("s");
            }
            else if (time.Minutes != 0)
            {
                sb.Append(time.Minutes.ToString());
                sb.Append("m");
                sb.Append(time.Seconds.ToString());
                sb.Append("s");
            }
            else
            {
                sb.Append(time.Seconds.ToString());
                sb.Append("s");
            }
            return sb.ToString();
        }

        #endregion

        #region other stuff

        public static T FindParentOf<T>(this DependencyObject dep) where T : class
        {
            while ((dep != null) && !(dep is T))
                dep = VisualTreeHelper.GetParent(dep);

            return dep as T;
        }

        public static T FindChildOf<T>(this DependencyObject dep) where T : class
        {
            while ((dep != null) && !(dep is T) && VisualTreeHelper.GetChildrenCount(dep) > 0)
                dep = VisualTreeHelper.GetChild(dep, 0);

            return dep as T;
        }



        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : class
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return child as T;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static StringBuilder Clear(this StringBuilder sb)
        {
            return sb.Remove(0, sb.Length);
        }


        public static string FindAndRemoveAt(this StringBuilder sb, string find)
        {
            string result = sb.ToString();
            int firstFoundAt;
            firstFoundAt = result.LastIndexOf(find);
            string sFound = string.Empty;
            if (firstFoundAt > -1)
            {
                int secondFoundAt = result.LastIndexOf(find, firstFoundAt - 1);
                if (secondFoundAt > -1)
                    sFound = sb.ToString(secondFoundAt + find.Length, (firstFoundAt - secondFoundAt) - find.Length);
                else
                    sFound = sb.ToString(0, result.Length - find.Length);

                sb.Remove(0, firstFoundAt + find.Length);
                return sFound;
            }

            return null;

        }

        public static bool IsPast(this DateTime dateTime, TimeSpan timeSpan)
        {
            TimeSpan timeDiff = SystemClock.Now.Subtract(dateTime);

            if (timeDiff.CompareTo(timeSpan) > 0)
                return true;
            else
                return false;
        }

        public static string ToSimpleString(this TimeSpan timeSpan)
        {

            return timeSpan.Hours.ToString("00") + ":" +
                timeSpan.Minutes.ToString("00") + ":" +
                timeSpan.Seconds.ToString("00");
        }


        #endregion




        #region ReaderWriterLockSlim Extensions

        public static void ExecuteWithReadLock(this ReaderWriterLockSlim rwLocker, Action function)
        {
            rwLocker.EnterReadLock();
            try
            {
                function();
            }
            finally
            {
                rwLocker.ExitReadLock();
            }

        }

        public static TResult ExecuteWithReadLock<TResult>(this ReaderWriterLockSlim rwLocker, Func<TResult> function)
        {
            rwLocker.EnterReadLock();
            try
            {
                return function();
            }
            finally
            {
                rwLocker.ExitReadLock();
            }

        }

        public static void ExecuteWithWriteLock(this ReaderWriterLockSlim rwLocker, Action function)
        {
            rwLocker.EnterWriteLock();
            try
            {
                function();
            }
            finally
            {
                rwLocker.ExitWriteLock();
            }

        }

        #endregion

        #region Enumeration Extension Methods


        public static bool TryConvertToEnum<T>(this string value, out T converted)
        {
            converted = default(T);
            try
            {
                converted = (T)Enum.Parse(typeof(T), value);
                return true;
            }
            catch
            {
                return false;
            }
        }


        public static string GetEnumDescription(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        
        #endregion

        public static bool TryReadLine(this StreamReader reader, out string line)
        {
            line = string.Empty;
            try
            {
                if (reader.EndOfStream) return false;
                line = reader.ReadLine();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
