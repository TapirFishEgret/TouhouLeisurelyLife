using THLL.BaseSystem;
using UnityEngine;

namespace THLL.TimeSystem
{
    public class GameTimeManager : Singleton<GameTimeManager>
    {
        #region 面板信息
        //初始时间
        public int startHour = 7;
        public int startDay = 1;
        public int startDayOfWeek = 1;
        public int startMonth = 1;
        public int startYear = 140;
        #endregion

        #region 周期函数
        protected override void Start()
        {
            base.Start();

            //时间的初始化
            GameTime.Init();
        }

        private void Update()
        {
            //时间自然流逝
            GameTime.TimeChange();
        }
        #endregion

        #region 游戏内周期函数
        protected override void OnMinuteChanged(int currentMinute)
        {
            base.OnMinuteChanged(currentMinute);
            Debug.Log(GameTime.TestMethod());
        }
        #endregion
    }
}
