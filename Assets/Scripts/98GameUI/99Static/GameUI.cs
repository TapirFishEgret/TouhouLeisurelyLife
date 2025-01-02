using System.Linq;
using THLL.CharacterSystem;
using THLL.UISystem.Settings;
using UnityEngine.UIElements;

namespace THLL.UISystem
{
    public static class GameUI
    {
        #region UI数据
        //系统相关界面
        //主标题界面
        public static MainTitle MainTitleInterface { get; set; }
        //新游戏界面
        public static NewGame NewGameInterface { get; set; }
        //读取与加载界面
        public static SaveAndLoadGame SaveAndLoadGameInterface { get; set; }
        //设置界面
        public static GameSettings GameSettingsInterface { get; set; }
        //系统设定界面
        public static GameSystemSettings GameSystemSettingsInterface { get; set; }
        //游戏玩法设定界面
        public static GameplaySettings GameplaySettingsInterface { get; set; }
        //游戏补丁设定界面
        public static GamePatchesSettings GamePatchesSettingsInterface { get; set; }

        //游玩相关界面
        //游玩界面
        public static BasicPlay BasicPlayInterface { get; set; }

        //辅助界面
        //背景图层
        public static BackgroundLayer BackgroundLayer { get; set; }
        //动画图层
        public static AnimationLayer AnimationLayer { get; set; }
        #endregion

        #region 游戏数据
        //设置数据
        //时间相关设置
        //是否显示秒数
        public static bool ShowSeconds { get; set; } = true;
        //是否启用幻想乡纪年法
        public static bool UseGensokyoYear { get; set; } = false;
        //是否启用特殊月份名称
        public static bool UseMonthName { get; set; } = true;
        //是否启用特殊星期名称
        public static bool UseDayOfWeekName { get; set; } = true;

        //视觉显示数据
        //是否显示角色
        public static bool ShowCharacter { get; set; } = true;
        //当前显示角色
        public static Character CurrentShowedCharacter { get; set; }
        #endregion

        #region 辅助方法
        //设定视觉元素动画间隔时间
        public static void SetVisualElementAllTransitionAnimationDuration(VisualElement visualElement, float animationDuration)
        {
            //需要设置的动画的个数
            int count = visualElement.resolvedStyle.transitionDuration.Count();
            //创建新列表
            StyleList<TimeValue> list = Enumerable.Repeat(new TimeValue(animationDuration, TimeUnit.Second), count).ToList();
            //赋值
            visualElement.style.transitionDuration = list;
        }
        #endregion
    }
}
