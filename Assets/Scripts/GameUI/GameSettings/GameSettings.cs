using UnityEngine.UIElements;

namespace THLL.UISystem
{
    public class GameSettings : BaseGameUI
    {
        #region 自身数据
        //根界面
        public VisualElement RootPanel { get; private set; }
        //系统设置按钮
        public Button GameSystemButton { get; private set; }
        //游戏玩法设置按钮
        public Button GamePlayButton { get; private set; }
        //游戏补丁设置按钮
        public Button GamePatchesButton { get; private set; }
        //返回按键
        public Button ReturnButton { get; private set; }
        #endregion

        #region 初始化与相关方法
        //获取视觉元素
        protected override void GetVisualElements()
        {
            RootPanel = Document.rootVisualElement.Q<VisualElement>("RootPanel");
            GameSystemButton = RootPanel.Q<Button>("GameSystemButton");
            GamePlayButton = RootPanel.Q<Button>("GamePlayButton");
            GamePatchesButton = RootPanel.Q<Button>("GamePatchesButton");
            ReturnButton = RootPanel.Q<Button>("ReturnButton");
        }
        //绑定方法
        protected override void RegisterMethods()
        {
            GameSystemButton.clicked += () => GameUI.ShowInterface(GameUI.GameSystemSettingsInterface);
            GamePlayButton.clicked += () => GameUI.ShowInterface(GameUI.GameplaySettingsInterface);
            GamePatchesButton.clicked += () => GameUI.ShowInterface(GameUI.GamePatchesSettingsInterface);
        }
        #endregion
    }
}
