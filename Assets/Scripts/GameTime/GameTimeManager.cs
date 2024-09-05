using THLL.BaseSystem;
using UnityEngine;

namespace THLL.TimeSystem
{
    public class GameTimeManager : Singleton<GameTimeManager>
    {
        #region 周期函数
        private void Update()
        {
            //时间自然流逝
            GameTime.TimeChange();
        }
        #endregion
    }
}
