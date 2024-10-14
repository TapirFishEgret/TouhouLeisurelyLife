using UnityEngine.UIElements;

namespace THLL.UISystem
{
    public class NewGame : BaseGameInterface
    {
        #region 自身数据
        //根面板
        public VisualElement RootPanel { get; private set; }
        //选项描述标签
        public Label OptionDescriptionLabel { get; private set; }
        //选项列表
        public ScrollView OptionsScrollView { get; private set; }
        //返回按键
        public Button ReturnButton { get; private set; }
        //继续按键
        public Button ContinueButton { get; private set; }
        #endregion

        #region 初始化与相关方法
        //获取视觉元素
        protected override void GetVisualElements()
        {
            RootPanel = Document.rootVisualElement.Q<VisualElement>("RootPanel");
            OptionDescriptionLabel = RootPanel.Q<Label>("OptionDescriptionLabel");
            OptionsScrollView = RootPanel.Q<ScrollView>("OptionsScrollView");
            ReturnButton = RootPanel.Q<Button>("ReturnButton");
            ContinueButton = RootPanel.Q<Button>("ContinueButton");
        }
        //绑定方法
        protected override void RegisterMethods()
        {
            //将继续按钮的功能暂时设定开启游玩面板并启用所有管理器并关闭背景移动
            ContinueButton.clicked += () => GameUI.ShowInterface(GameUI.BasicPlayInterface);
            ContinueButton.clicked += TouhouLeisurelyLife.EnableAllManager;
            ContinueButton.clicked += GameUI.BackgroundLayer.StopCycleMainTitleBackground;
        }
        #endregion
    }
}
