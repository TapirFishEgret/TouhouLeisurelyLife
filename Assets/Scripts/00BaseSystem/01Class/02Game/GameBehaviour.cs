using THLL.TimeSystem;
using UnityEngine;

namespace THLL.BaseSystem
{
    /// <summary>
    /// MonoBehaviour扩展，加入游戏内的周期函数
    /// </summary>
    public class GameBehaviour : MonoBehaviour
    {
        #region 周期函数
        protected virtual void Awake()
        {
            //唤醒时增加相关订阅
            //时间
            GameTime.SecondChangedEvent += OnSecondChanged;
            GameTime.MinuteChangedEvent += OnMinuteChanged;
            GameTime.HourChangedEvent += OnHourChanged;
            GameTime.DayChangedEvent += OnDayChanged;
            GameTime.WeekChangedEvent += OnWeekChanged;
            GameTime.MonthChangedEvent += OnMonthChanged;
            GameTime.YearChangedEvent += OnYearChanged;
            GameTime.CycleChangedEvent += OnCycleChanged;
        }

        protected virtual void OnDestory()
        {
            //摧毁时消灭相关订阅
            //时间
            GameTime.SecondChangedEvent -= OnSecondChanged;
            GameTime.MinuteChangedEvent -= OnMinuteChanged;
            GameTime.HourChangedEvent -= OnHourChanged;
            GameTime.DayChangedEvent -= OnDayChanged;
            GameTime.WeekChangedEvent -= OnWeekChanged;
            GameTime.MonthChangedEvent -= OnMonthChanged;
            GameTime.YearChangedEvent -= OnYearChanged;
            GameTime.CycleChangedEvent -= OnCycleChanged;
        }
        #endregion

        #region 游戏内周期函数
        //时间相关
        protected virtual void OnSecondChanged(int count) { }
        protected virtual void OnMinuteChanged(int count) { }
        protected virtual void OnHourChanged(int count) { }
        protected virtual void OnDayChanged(int count) { }
        protected virtual void OnWeekChanged(int count) { }
        protected virtual void OnMonthChanged(int count) { }
        protected virtual void OnYearChanged(int count) { }
        protected virtual void OnCycleChanged(int count) { }
        #endregion
    }
}