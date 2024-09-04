using System;
using UnityEngine;

namespace THLL.TimeSystem
{
    /// <summary>
    /// 游戏时间静态类
    /// 所有游戏时间相关的数据及方法都在这里
    /// </summary>
    public static class GameTime
    {
        #region 常量
        public const int TimePerSecond = 1;
        public const int SecondsPerMinute = 60;
        public const int SecondsPerHour = TimePerSecond * SecondsPerMinute;
        public const int SecondsPerDay = SecondsPerHour * HoursPerDay;
        public const int SecondsPerWeek = SecondsPerDay * DaysPerWeek;
        public const int SecondsPerMonth = SecondsPerDay * DaysPerMonth;
        public const int SecondsPerYear = SecondsPerMonth * MonthsPerYear;
        public const int MinutesPerHour = 60;
        public const int MinutesPerDay = MinutesPerHour * HoursPerDay;
        public const int MinutesPerWeek = MinutesPerDay * DaysPerWeek;
        public const int MinutesPerMonth = MinutesPerDay * DaysPerMonth;
        public const int MinutesPerYear = MinutesPerMonth * MonthsPerYear;
        public const int HoursPerDay = 24;
        public const int HoursPerWeek = HoursPerDay * DaysPerWeek;
        public const int HoursPerMonth = HoursPerDay * DaysPerMonth;
        public const int HoursPerYear = HoursPerMonth * MonthsPerYear;
        public const int DaysPerWeek = 7;
        public const int DaysPerMonth = 30;
        public const int DaysPerYear = DaysPerMonth * MonthsPerYear;
        public const int MonthsPerYear = 4;
        public const int YearsPerCycle = 60;
        #endregion

        #region 时间数据
        //时间流逝速度
        public static int TimeScale { get; private set; }
        //毫秒
        public static float Millisecond { get; private set; }
        //秒
        public static int Second { get; private set; }
        //分
        public static int Minute { get; private set; }
        //时
        public static int Hour { get; private set; }
        //天
        public static int Day { get; private set; }
        //星期
        public static int DayOfWeek { get; private set; }
        //月份
        public static int Month { get; private set; }
        //年份
        public static int Year { get; private set; }
        //游戏内当日时间
        public static int DayTime_Seconds
        {
            get
            {
                return Second + (Minute * SecondsPerMinute) + (Hour * SecondsPerHour);
            }
        }
        public static int DayTime_Minutes
        {
            get
            {
                return Minute + (Hour * MinutesPerHour);
            }
        }
        //游戏内当月时间
        public static int MonthTime_Minutes
        {
            get
            {
                return Minute + (Hour * MinutesPerHour) + ((Day - 1) * MinutesPerDay);
            }
        }
        public static int MonthTime_Hours
        {
            get
            {
                return Hour + ((Day - 1) * HoursPerDay);
            }
        }
        #endregion

        #region 时间事件
        //秒
        public static event Action<int> SecondChangedEvent;
        //分
        public static event Action<int> MinuteChangedEvent;
        //时
        public static event Action<int> HourChangedEvent;
        //天
        public static event Action<int> DayChangedEvent;
        //星期
        public static event Action WeekChangedEvent;
        //月份
        public static event Action<int> MonthChangedEvent;
        //年份
        public static event Action<int> YearChangedEvent;
        //循环
        public static event Action CycleChangedEvent;
        #endregion

        #region 时间自然流逝方法
        //时间变更
        public static void TimeChange()
        {
            //自增
            Millisecond += TimeScale * Time.deltaTime;

            //检测
            if (Millisecond >= TimePerSecond)
            {
                //计算秒(防止时间流速过快导致现实一秒对应游戏多秒)
                int seconds = Mathf.FloorToInt(Millisecond / TimePerSecond);
                //触发下一个方法
                AddSeconds(seconds);
                //重置毫秒数
                Millisecond -= seconds * TimePerSecond;
                //触发事件
                SecondChangedEvent?.Invoke(Second);
            }
        }
        //增加秒数
        private static void AddSeconds(int seconds)
        {
            //自增
            Second += seconds;

            //检测
            if (Second >= SecondsPerMinute)
            {
                //计算分钟
                int minutes = Mathf.FloorToInt(Second / SecondsPerMinute);
                //触发下一个方法
                AddMinutes(minutes);
                //重置秒数
                Second -= minutes * SecondsPerMinute;
                //触发事件
                MinuteChangedEvent?.Invoke(Minute);
            }
        }
        //增加分钟
        private static void AddMinutes(int minutes)
        {
            //自增
            Minute += minutes;

            //检测
            if (Minute >= MinutesPerHour)
            {
                //计算小时
                int hours = Mathf.FloorToInt(Minute / MinutesPerHour);
                //触发下一个方法
                AddHours(hours);
                //重置分钟
                Minute -= hours * MinutesPerHour;
                //触发事件
                HourChangedEvent?.Invoke(Hour);
            }
        }
        //增加小时
        private static void AddHours(int hours)
        {
            //自增
            Hour += hours;

            //检测
            if (Hour >= HoursPerDay)
            {
                //计算天数
                int days = Mathf.FloorToInt(Hour / HoursPerDay);
                //触发下一个方法
                AddDays(days);
                //重置小时
                Hour -= hours * HoursPerDay;
                //触发事件
                DayChangedEvent?.Invoke(Day);
            }
        }
        //增加天数
        private static void AddDays(int days)
        {
            //自增
            Day += days;
            DayOfWeek += days;

            //检测星期
            if (DayOfWeek > DaysPerWeek)
            {
                //计算星期
                int weeks = Mathf.FloorToInt(DayOfWeek / DaysPerWeek);
                //重置星期
                DayOfWeek -= weeks * DaysPerWeek;
                //触发事件
                WeekChangedEvent?.Invoke();
            }

            //检测月份
            if (Day > DaysPerMonth)
            {
                //计算月份数
                int months = Mathf.FloorToInt(Day / DaysPerMonth);
                //触发下一个方法
                AddMonths(months);
                //重置日期
                Day -= months * DaysPerMonth;
                //触发事件
                MonthChangedEvent?.Invoke(Month);
            }
        }
        //增加月份
        public static void AddMonths(int months)
        {
            //自增
            Month += months;

            //检测年份
            if (Month > MonthsPerYear)
            {
                //计算年份
                int years = Mathf.FloorToInt(Month / MonthsPerYear);
                //触发下一个方法
                AddYears(years);
                //重置月份
                Month -= years * MonthsPerYear;
                //触发事件
                YearChangedEvent?.Invoke(Year);
            }
        }
        //增加年份
        public static void AddYears(int years)
        {
            //自增
            Year += years;

            //检测循环
            if (Year > YearsPerCycle)
            {
                //触发事件
                CycleChangedEvent?.Invoke();
            }
        }
        #endregion

        #region 其他方法
        //初始化
        public static void Init()
        {
            TimeScale = 24;
            Millisecond = 0;
            Second = 0;
            Minute = 0;
            Hour = GameTimeManager.Instance.startHour;
            Day = GameTimeManager.Instance.startDay;
            DayOfWeek = GameTimeManager.Instance.startDayOfWeek;
            Month = GameTimeManager.Instance.startMonth;
            Year = GameTimeManager.Instance.startYear;
        }
        //报时
        public static string TestMethod()
        {
            return string.Join("/", new string[] { Year.ToString(), Month.ToString(), Day.ToString() })
                + " " +
                string.Join(":", new string[] { Hour.ToString(), Minute.ToString(), Second.ToString() })
                + " " +
                DayOfWeek.ToString();
        }
        #endregion
    }
}
