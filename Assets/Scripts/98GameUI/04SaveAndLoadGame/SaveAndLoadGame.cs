using UnityEngine.UIElements;

namespace THLL.UISystem
{
    public class SaveAndLoadGame : BaseGameInterface
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
        #endregion

        #region 初始化与相关方法
        //获取视觉元素
        protected override void GetVisualElements()
        {
            RootPanel = Document.rootVisualElement.Q<VisualElement>("RootPanel");
            OptionDescriptionLabel = RootPanel.Q<Label>("OptionDescriptionLabel");
            OptionsScrollView = RootPanel.Q<ScrollView>("OptionsScrollView");
            ReturnButton = RootPanel.Q<Button>("ReturnButton");
        }
        #endregion
    }
}
