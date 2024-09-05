﻿using THLL.TimeSystem;
using UnityEngine.UIElements;

namespace THLL.UISystem.Settings
{
    public class GameplaySettings : BaseGameUI
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
        //游戏时间流逝速度
        public SliderInt GameTimeScaleSliderInt { get; private set; }
        public string GameTimeScaleDescription { get; } = "用于更改游戏内时间流逝速度。" +
            "\n其数值为游戏中每秒相当于现实中X秒。" +
            "\n数值为1时游戏内时间流逝与现实等同，为24时游戏内1天为现实1小时(自然流逝的情况下)。";
        //测试用的小玩意儿
        public TextField TestTextField { get; private set; }
        public string TestDescription { get; } = "测试用的，看看hover与focus还有逐字显示文本功能好不好使";
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
            GameTimeScaleSliderInt = RootPanel.Q<SliderInt>("GameTimeScaleSliderInt");
            TestTextField = RootPanel.Q<TextField>("TestTextField");
        }
        //绑定相关方法
        protected override void RegisterMethods()
        {
            //设定每一位的具体功能及描述显示，使用MouseEnterEvent实现悬浮时显示的功能
            //时间流逝速度
            GameTimeScaleSliderInt.RegisterValueChangedCallback(evt => GameTime.TimeScale = evt.newValue);
            GameTimeScaleSliderInt.RegisterCallback<MouseEnterEvent>(evt => GameUI.GradientDisplayText(this, OptionDescriptionLabel, GameTimeScaleDescription, 0.3f));
            //测试用的
            TestTextField.RegisterCallback<MouseEnterEvent>(evt => GameUI.ProgressiveDisplayText(this, OptionDescriptionLabel, TestDescription, 0.5f));
        }
        #endregion
    }
}