using System.Collections.Generic;
using THLL.TimeSystem;
using UnityEngine.UIElements;

namespace THLL.UISystem.Settings
{
    public class GameplaySettings : BaseGameInterface
    {
        #region 自身数据
        //四大天王
        //根面板
        public VisualElement RootPanel { get; private set; }
        //选项描述标签
        public Label OptionDescriptionLabel { get; private set; }
        //选项列表
        public ScrollView OptionsScrollView { get; private set; }
        //返回按键
        public Button ReturnButton { get; private set; }
        //其他数据
        //UI选项描述字典
        public Dictionary<VisualElement, string> OptionDescriptionDic { get; } = new();
        #endregion

        #region 游戏时间相关
        //游戏时间流逝速度
        public SliderInt GameTimeScaleSliderInt { get; private set; }
        //是否启用长年
        public Toggle LongYearToggle { get; private set; }
        //是否显示秒数
        public Toggle ShowSecondsToggle { get; private set; }
        //是否使用幻想乡年
        public Toggle UseGensokyoYearToggle { get; private set; }
        //是否使用月份名称
        public Toggle UseMonthNameToggle { get; private set; }
        //是否使用星期名称
        public Toggle UseDayOfWeekNameToggle { get; private set; }
        #endregion

        #region 游戏UI相关
        //是否显示角色面板
        public Toggle ShowCharacterPanelToggle { get; private set; }
        #endregion

        #region Unity周期函数
        //Awake
        protected override void Awake()
        {
            //基类Awake
            base.Awake();
            //赋值选项描述
            OptionDescriptionDic[GameTimeScaleSliderInt] = "用于更改游戏内时间流逝速度。\n其数值为游戏中每秒相当于现实中X秒。\n数值为1时游戏内时间流逝与现实等同，为24时游戏内1天为现实1小时(自然流逝的情况下)。";
            OptionDescriptionDic[LongYearToggle] = "启用更长的年份。\n默认状态下一年为4月，启用状态下一年为12月，可能会影响游戏在游玩体感上的速度。";
            OptionDescriptionDic[ShowSecondsToggle] = "是否在UI界面上显示秒数。\n对游戏本身运行并没有实际影响。";
            OptionDescriptionDic[UseGensokyoYearToggle] = "启用类似「日与春与土之年」的幻想乡年份表示方式。";
            OptionDescriptionDic[UseMonthNameToggle] = "启用类似「春之月」或者「睦月」的月份名称。";
            OptionDescriptionDic[UseDayOfWeekNameToggle] = "启用类似「月曜日」的日式星期名称";
            OptionDescriptionDic[ShowCharacterPanelToggle] = "开关角色立绘区域显示。\n不过实际上当没有角色被选中时它自个儿就关了其实。";
        }
        #endregion


        #region 初始化与相关方法
        //获取视觉元素
        protected override void GetVisualElements()
        {
            //基础
            RootPanel = Document.rootVisualElement.Q<VisualElement>("RootPanel");
            OptionDescriptionLabel = RootPanel.Q<Label>("OptionDescriptionLabel");
            OptionsScrollView = RootPanel.Q<ScrollView>("OptionsScrollView");
            ReturnButton = RootPanel.Q<Button>("ReturnButton");

            //具体
            //时间
            GameTimeScaleSliderInt = OptionsScrollView.Q<SliderInt>("GameTimeScaleSliderInt");
            LongYearToggle = OptionsScrollView.Q<Toggle>("LongYearToggle");
            ShowSecondsToggle = OptionsScrollView.Q<Toggle>("ShowSecondsToggle");
            UseGensokyoYearToggle = OptionsScrollView.Q<Toggle>("UseGensokyoYearToggle");
            UseMonthNameToggle = OptionsScrollView.Q<Toggle>("UseMonthNameToggle");
            UseDayOfWeekNameToggle = OptionsScrollView.Q<Toggle>("UseDayOfWeekNameToggle");
            //UI
            ShowCharacterPanelToggle = OptionsScrollView.Q<Toggle>("ShowCharacterPanelToggle");
        }
        //绑定相关方法
        protected override void RegisterMethods()
        {
            //时间相关
            //时间流逝速度
            GameTimeScaleSliderInt.RegisterValueChangedCallback(evt => GameTime.TimeScale = evt.newValue);
            //是否启用长年
            LongYearToggle.RegisterValueChangedCallback(evt => GameTime.MonthsPerYear = evt.newValue ? 12 : 0);
            //是否显示秒数
            ShowSecondsToggle.RegisterValueChangedCallback(evt => GameTime.ShowSeconds = evt.newValue);
            //是否使用幻想乡年
            UseGensokyoYearToggle.RegisterValueChangedCallback(evt => GameTime.UseGensokyoYear = evt.newValue);
            //是否使用月份名称
            UseMonthNameToggle.RegisterValueChangedCallback(evt => GameTime.UseMonthName = evt.newValue);
            //是否使用星期名称
            UseDayOfWeekNameToggle.RegisterValueChangedCallback(evt => GameTime.UseDayOfWeekName = evt.newValue);

            //UI相关
            ShowCharacterPanelToggle.RegisterValueChangedCallback(evt => GameUI.BasicPlayInterface.CharacterPanel.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None);

            //给选项加上悬停时显示选项描述的功能
            foreach (var kvp in OptionDescriptionDic)
            {
                kvp.Key.RegisterCallback<PointerEnterEvent>(evt => OptionDescriptionLabel.GradientDisplayText(kvp.Value, 0.5f));
            }
            //给标签父级增添点击时显示全部文本功能
            OptionDescriptionLabel.parent.RegisterCallback<PointerDownEvent>(evt => { if (evt.button == 0) OptionDescriptionLabel.DirectlyDisplayText(); });
        }
        #endregion
    }
}
