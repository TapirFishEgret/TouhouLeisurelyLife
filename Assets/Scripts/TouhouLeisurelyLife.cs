using System.Collections.Generic;
using THLL.BaseSystem;
using THLL.UISystem;

namespace THLL
{
    public static class TouhouLeisurelyLife
    {
        #region 数据
        //启用的管理器们
        public static HashSet<GameBehaviour> Managers { get; } = new();
        #endregion

        #region 方法
        //启动所有管理器
        public static void EnableAllManager()
        {
            foreach (GameBehaviour manager in Managers)
            {
                manager.enabled = true;
            }
        }
        //禁用除了UI管理器及资源加载管理器以外的所有管理器
        public static void DisableAllManager()
        {
            foreach (GameBehaviour manager in Managers)
            {
                if (!(manager.GetType() == typeof(GameUIManager) || manager.GetType() == typeof(GameAssetsManager)))
                {
                    manager.enabled = false;
                }
            }
        }
        #endregion
    }
}
