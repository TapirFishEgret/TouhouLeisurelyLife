using System;
using System.Collections.Generic;
using THLL.UISystem;

namespace THLL.BaseSystem
{
    public class GameAssetsManager : Singleton<GameAssetsManager>
    {
        #region 数据
        //资源加载事件
        public event Action OnAllResourcesLoaded;
        //资源加载队列
        public Queue<Action> ResourcesLoadQueue { get; private set; } = new();
        #endregion

        #region 周期函数
        //Awake
        protected override void Awake()
        {
            //父类Awake方法
            base.Awake();
            //将自己设定为启用
            enabled = true;
        }
        //Start
        private void Start()
        {
            //显示加载界面
            GameUI.AnimationLayer.ShowLoadingScreen();
        }
        #endregion

        #region 资源加载方法

        #endregion
    }
}
