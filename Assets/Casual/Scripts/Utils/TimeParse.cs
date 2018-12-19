using UnityEngine;
using System;
using System.Collections;
using System.Globalization;

namespace Utils
{
    public class TimeParse
    {
        const long TICKS_PER_SECOND = 10000000;
        const string DATEFORMAT = "yyyyMMdd HH:mm:ss";
        const string SHORTDATEFORMAT = "yyyyMMdd";

		private static long sTimeOffset = 62135625600;
		public static long TimeOffset
		{
			get
			{
				return sTimeOffset;
			}
			set
			{
				sTimeOffset = value;
			}
		}

		static public long CutTime(long time, string pattern)
        {
            return String2Long(Long2DateTime(time).ToString(pattern), pattern);
        }

        static public DateTime Long2DateTime(long time)
        {
            return new DateTime((time + TimeOffset) * TICKS_PER_SECOND);
        }

        static public long DateTime2Long(DateTime data)
        {
            return data.Ticks / TICKS_PER_SECOND - TimeOffset;
        }

        static public long String2Long(string data, string pattern)
        {
            DateTimeFormatInfo dtFormat = new DateTimeFormatInfo();
            dtFormat.ShortDatePattern = pattern;
			DateTime dateTime = DateTime.ParseExact (data, pattern, CultureInfo.CurrentCulture);//Convert.ToDateTime(data, dtFormat);
            return DateTime2Long(dateTime);
        }

        static public long String2Long(string data)
        {
            return String2Long(data, DATEFORMAT);
        }

        static public TimeSpan TimeSpan(long timeOffset)
        {
            return new TimeSpan(timeOffset * TICKS_PER_SECOND);
        }

        public static string Seconds2Countdown(int time, bool hasHours = true)
        {
            TimeSpan span = new TimeSpan(0, 0, time);
            string pattern = hasHours ? "{2:D2}:{1:D2}:{0:D2}" : "{1:D2}:{0:D2}";
            return string.Format(pattern, span.Seconds, span.Minutes, span.Hours + span.Days * 24);
        }
    }
}

