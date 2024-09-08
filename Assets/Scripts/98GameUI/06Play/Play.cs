using UnityEngine.UIElements;

namespace THLL.UISystem
{
    public class Play : BaseGameInterface
    {
        #region 自身数据
        //UI元素
        //信息面板
        public VisualElement InfoPanel { get; private set; }
        //角色面板
        public VisualElement CharacterPanel { get; private set; }
        //角色立绘显示
        public VisualElement CharacterPortrait { get; private set; }
        //功能面板
        public VisualElement FunctionPanel { get; private set; }
        //角色名称标签
        public Label CharacterNameLabel { get; private set; }
        //动作按钮面板
        public ScrollView ActionsButtonsPanel { get; private set; }
        //对话面板
        public VisualElement DialogPanel { get; private set; }
        //对话
        public Label DialogLabel { get; private set; }
        #endregion

        #region 初始化与相关方法
        //获取界面元素
        protected override void GetVisualElements()
        {
            InfoPanel = Document.rootVisualElement.Q<VisualElement>("InfoPanel");
            CharacterPanel = Document.rootVisualElement.Q<VisualElement>("CharacterPanel");
            CharacterPortrait = Document.rootVisualElement.Q<VisualElement>("CharacterPortrait");
            FunctionPanel = Document.rootVisualElement.Q<VisualElement>("FunctionPanel");
            CharacterNameLabel = Document.rootVisualElement.Q<Label>("CharacterNameLabel");
            ActionsButtonsPanel = Document.rootVisualElement.Q<ScrollView>("ActionsButtonsPanel");
            DialogPanel = Document.rootVisualElement.Q<VisualElement>("DialogPanel");
            DialogLabel = Document.rootVisualElement.Q<Label>("DialogLabel");
        }
        #endregion
    }
}
