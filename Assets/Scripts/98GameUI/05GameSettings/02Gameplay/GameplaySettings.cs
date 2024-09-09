using THLL.TimeSystem;
using UnityEngine;
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

        //具体选项及对应工具指引
        //游戏时间相关
        //游戏时间流逝速度
        public SliderInt GameTimeScaleSliderInt { get; private set; }
        public string GameTimeScaleDescription { get; } = "用于更改游戏内时间流逝速度。" +
            "\n其数值为游戏中每秒相当于现实中X秒。" +
            "\n数值为1时游戏内时间流逝与现实等同，为24时游戏内1天为现实1小时(自然流逝的情况下)。";
        //是否显示秒数
        public Toggle ShowSecondsToggle { get; private set; }
        public string ShowSecondsDescription { get; } = "是否在UI界面上显示秒数。\n对游戏本身运行并没有实际影响。";
        //游戏UI相关
        //是否显示角色面板
        public Toggle ShowCharacterPanelToggle { get; private set; }
        public string ShowCharacterPanelDescription { get; } = "开关角色立绘区域显示。\n不过实际上当没有角色被选中时它自个儿就关了其实。";
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
            ShowSecondsToggle = OptionsScrollView.Q<Toggle>("ShowSecondsToggle");
            //UI
            ShowCharacterPanelToggle = OptionsScrollView.Q<Toggle>("ShowCharacterPanelToggle");
        }
        //绑定相关方法
        protected override void RegisterMethods()
        {
            //设定每一位的具体功能及描述显示，使用MouseEnterEvent实现悬浮时显示的功能
            //时间相关
            //时间流逝速度
            GameTimeScaleSliderInt.RegisterValueChangedCallback(evt => GameTime.TimeScale = evt.newValue);
            GameTimeScaleSliderInt.RegisterCallback<PointerEnterEvent>(evt => GameUI.GradientDisplayText(this, OptionDescriptionLabel, GameTimeScaleDescription, 0.5f));
            //是否显示秒数
            ShowSecondsToggle.RegisterValueChangedCallback(evt => GameTime.ShowSeconds = evt.newValue);
            ShowSecondsToggle.RegisterCallback<PointerEnterEvent>(evt => GameUI.GradientDisplayText(this, OptionDescriptionLabel, ShowSecondsDescription, 0.5f));
            //UI相关
            ShowCharacterPanelToggle.RegisterValueChangedCallback(evt => GameUI.BasicPlayInterface.CharacterPanel.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None);
            ShowCharacterPanelToggle.RegisterCallback<PointerEnterEvent>(evt => GameUI.GradientDisplayText(this, OptionDescriptionLabel, ShowCharacterPanelDescription, 0.5f));

            //给标签父级增添点击时显示全部文本功能
            OptionDescriptionLabel.parent.RegisterCallback<PointerDownEvent>(evt => { if (evt.button == 0) GameUI.DirectlyDisplayText(this, OptionDescriptionLabel); });
        }
        #endregion
    }
}
