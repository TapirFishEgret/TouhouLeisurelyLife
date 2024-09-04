using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace THLL.UISystem
{
    public static class GameUI
    {
        #region 数据
        //各类界面
        //背景图层
        public static BackgroundLayer BackgroundLayer { get; set; }
        //主标题界面
        public static MainTitle MainTitleInterface { get; set; }
        //新游戏界面
        public static NewGame NewGameInterface { get; set; }
        //读取与加载界面
        public static SaveAndLoadGame SaveAndLoadGameInterface { get; set; }
        //设置界面
        public static GameSettings GameSettingsInterface { get; set; }
        //系统设定界面
        public static Settings.GameSystem GameSystemInterface { get; set; }
        //游戏玩法设定界面
        public static Settings.GamePlay GamePlaySInterface { get; set; }
        //游戏补丁设定界面
        public static Settings.GamePatches GamePatchesInterface { get; set; }
        //动画图层
        public static AnimationLayer AnimationLayer { get; set; }

        //界面显示状态存储，主要用于返回功能
        public static List<BaseGameUI> ShowedInterfaces { get; } = new();
        #endregion

        #region 静态方法
        //显示界面
        public static void ShowInterface(BaseGameUI @interface, bool needsAnimation = true)
        {
            //检测是否需要动画
            if (needsAnimation)
            {
                //若需要，执行动画图层方法
                AnimationLayer.PlayOnce(() => ShowInterface(@interface));
            }
            else
            {
                //若不需要，直接执行方法
                ShowInterface(@interface);
            }
        }
        //显示界面本体
        private static void ShowInterface(BaseGameUI @interface)
        {
            //正常执行，首先让存储中的界面归于底层
            ShowedInterfaces.ForEach(i => i.Document.sortingOrder = -1);
            //然后让要显示的界面浮于表面
            @interface.Document.sortingOrder = 1;
            //将它加入存储中
            ShowedInterfaces.Add(@interface);
        }
        //返回上一层界面
        public static void ReturnInterface(bool needsAnimation = true)
        {
            //检测是否需要动画
            if (needsAnimation)
            {
                //若需要，借助动画图层执行方法
                AnimationLayer.PlayOnce(ReturnInterface);
            }
            else
            {
                //若不需要，直接执行方法
                ReturnInterface();
            }
        }
        //返回上一层界面本体
        private static void ReturnInterface()
        {
            //检测数量
            if (ShowedInterfaces.Count > 1)
            {
                //大于1时，继续
                //让存储中的界面归于底层
                ShowedInterfaces.ForEach(i => i.Document.sortingOrder = -1);
                //剔除最后一个
                ShowedInterfaces.Remove(ShowedInterfaces[^1]);
                //让接下来的最后一个浮于表面
                ShowedInterfaces[^1].Document.sortingOrder = 1;
            }
        }
        #endregion
    }
}
