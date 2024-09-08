using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.UISystem
{
    public class MainTitle : BaseGameUI
    {
        #region 数据
        //根界面
        public VisualElement RootPanel { get; private set; }
        //VersionLabel
        public Label VersionLabel { get; private set; }
        //LocationLabel
        public Label LocationLabel { get; private set; }
        //GameSystemButton
        public Button NewGameButton { get; private set; }
        //GamePlayButton
        public Button LoadGameButton { get; private set; }
        //GamePatchesButton
        public Button SettingsButton { get; private set; }
        //ReturnButton
        public Button QuitGameButton { get; private set; }
        #endregion

        #region 初始化与相关方法
        //绑定UI元素
        protected override void GetVisualElements()
        {
            RootPanel = Document.rootVisualElement.Q<VisualElement>("RootPanel");
            VersionLabel = RootPanel.Q<Label>("VersionLabel");
            LocationLabel = RootPanel.Q<Label>("LocationLabel");
            NewGameButton = RootPanel.Q<Button>("NewGameButton");
            LoadGameButton = RootPanel.Q<Button>("LoadGameButton");
            SettingsButton = RootPanel.Q<Button>("SettingsButton");
            QuitGameButton = RootPanel.Q<Button>("QuitGameButton");
        }
        //绑定方法
        protected override void RegisterMethods()
        {
            NewGameButton.clicked += () => GameUI.ShowInterface(GameUI.NewGameInterface);
            LoadGameButton.clicked += () => GameUI.ShowInterface(GameUI.SaveAndLoadGameInterface);
            SettingsButton.clicked += () => GameUI.ShowInterface(GameUI.GameSettingsInterface);
            QuitGameButton.clicked += QuitGame;
        }
        #endregion

        #region UI方法
        //游戏主界面
        //当退出时
        private void QuitGame()
        {
            //退出游戏
            Application.Quit();
        }
        #endregion
    }
}
