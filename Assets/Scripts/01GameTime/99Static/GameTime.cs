using System;
using THLL.BaseSystem;
using THLL.UISystem;
using UnityEngine;

namespace THLL.TimeSystem
{
    /// <summary>
    /// 游戏时间静态类
    /// 所有游戏时间相关的数据及方法都在这里
    /// </summary>
    public static class GameTime
    {
        #region 常用的量
        public static int TimePerSecond = 1;
        public static int SecondsPerMinute = 60;
        public static int SecondsPerHour = TimePerSecond * SecondsPerMinute;
        public static int SecondsPerDay = SecondsPerHour * HoursPerDay;
        public static int SecondsPerWeek = SecondsPerDay * DaysPerWeek;
        public static int SecondsPerMonth = SecondsPerDay * DaysPerMonth;
        public static int SecondsPerYear = SecondsPerMonth * MonthsPerYear;
        public static int MinutesPerHour = 60;
        public static int MinutesPerDay = MinutesPerHour * HoursPerDay;
        public static int MinutesPerWeek = MinutesPerDay * DaysPerWeek;
        public static int MinutesPerMonth = MinutesPerDay * DaysPerMonth;
        public static int MinutesPerYear = MinutesPerMonth * MonthsPerYear;
        public static int HoursPerDay = 24;
        public static int HoursPerWeek = HoursPerDay * DaysPerWeek;
        public static int HoursPerMonth = HoursPerDay * DaysPerMonth;
        public static int HoursPerYear = HoursPerMonth * MonthsPerYear;
        public static int DaysPerWeek = 7;
        public static int DaysPerMonth = 30;
        public static int DaysPerYear = DaysPerMonth * MonthsPerYear;
        public static int MonthsPerYear = 4;
        public static int YearsPerCycle = 60;
        #endregion

        #region 时间数据
        //时间流逝速度
        public static int TimeScale { get; set; } = 1;
        //毫秒
        public static float Millisecond { get; set; } = 0;
        //秒
        public static int Second { get; set; } = 0;
        //分
        public static int Minute { get; set; } = 0;
        //时
        public static int Hour { get; set; } = 7;
        //天
        public static int Day { get; set; } = 1;
        //星期
        public static int DayOfWeek { get; set; } = 1;
        //月份
        public static int Month { get; set; } = 1;
        //年份
        public static int Year { get; set; } = 140;
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
        //幻想乡年
        public static string GensokyoYear
        {
            get
            {
                int sanseiIndex = Year % 3;
                int shikiIndex = Year % 4;
                int goyoIndex = (Year - 1) % 5; //只有五行比较特殊，纪年法中五行以土为起始

                return $"{Sansei[sanseiIndex]}与{Shiki[shikiIndex]}与{Goyo[goyoIndex]}";
            }
        }
        //月份名称
        public static string MonthName
        {
            get
            {
                //检测月份数量
                if (MonthsPerYear == 4)
                {
                    //若是短月份制
                    return MonthNameFor4[Month - 1];
                }
                else if (MonthsPerYear == 12)
                {
                    //若是长月份制
                    return MonthNameFor12[Month - 1];
                }
                //若都不是，记录
                GameHistory.LogError("检查一下每年月份是否设置正确");
                //返回空
                return string.Empty;
            }
        }
        //星期名称
        public static string DayOfWeekName
        {
            get
            {
                //检测是否启用
                if (GameUI.UseDayOfWeekName)
                {
                    return DayOfWeekNameOld[DayOfWeek - 1];
                }
                else
                {
                    return DayOfWeekNameNormal[DayOfWeek - 1];
                }
            }
        }
        #endregion

        #region 其他数据
        //用于幻想乡纪年的三精
        private static readonly string[] Sansei = { "日", "月", "星" };
        //及四季
        private static readonly string[] Shiki = { "春", "夏", "秋", "冬" };
        //与五行
        private static readonly string[] Goyo = { "火", "水", "木", "金", "土" };
        //用于月份名称的春夏秋冬月
        private static readonly string[] MonthNameFor4 = { "春之月", "夏之月", "秋之月", "冬之月" };
        //及旧式月份名称
        private static readonly string[] MonthNameFor12 = { "睦月", "如月", "弥生", "卯月", "皋月", "水無月", "文月", "葉月", "長月", "神無月", "霜月", "師走" };
        //用于星期名称的普通星期名称
        private static readonly string[] DayOfWeekNameNormal = { "星期一", "星期二", "星期三", "星期四", "星期五", "星期六", "星期日" };
        //及日式星期名称
        private static readonly string[] DayOfWeekNameOld = { "月曜日", "火曜日", "水曜日", "木曜日", "金曜日", "土曜日", "日曜日" };
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
        public static event Action<int> WeekChangedEvent;
        //月份
        public static event Action<int> MonthChangedEvent;
        //年份
        public static event Action<int> YearChangedEvent;
        //循环
        public static event Action<int> CycleChangedEvent;
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
                SecondChangedEvent?.Invoke(seconds);
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
                MinuteChangedEvent?.Invoke(minutes);
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
                HourChangedEvent?.Invoke(hours);
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
                DayChangedEvent?.Invoke(days);
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
                WeekChangedEvent?.Invoke(weeks);
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
                MonthChangedEvent?.Invoke(months);
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
                YearChangedEvent?.Invoke(years);
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
                //计算循环
                int cycles = Mathf.FloorToInt(Year / YearsPerCycle);
                //触发事件
                CycleChangedEvent?.Invoke(cycles);
            }
        }
        #endregion
    }
}
