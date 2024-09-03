using UnityEngine.UIElements;

namespace THLL.GameUI.GameMainTitleInterface
{
    public class GameSettingsInterface : VisualElement
    {
        #region 自身数据
        //主界面
        public GameMainTitleInterface GameMainTitleInterface { get; private set; }
        //根界面
        public VisualElement GameSettingsPanel { get; private set; }
        //GameSystemSettings
        public GameSystemSettingsInterface GameSystemSettingsInterface { get; private set; }
        public Button GameSystemSettingsButton { get; private set; }
        //GamePlaySettings
        public GamePlaySettingsInterface GamePlaySettingsInterface { get; private set; }
        public Button GamePlaySettingsButton { get; private set; }
        //GamePatchesSettings
        public GamePatchesSettingsInterface GamePatchesSettingsInterface { get; private set; }
        public Button GamePatchesSettingsButton { get; private set; }
        #endregion

        #region 构造函数与初始化与相关方法
        //构造函数
        public GameSettingsInterface(GameMainTitleInterface gameMainTitleInterface, VisualTreeAsset visualTree)
        {
            //设定自身
            style.flexGrow = 1;
            style.display = DisplayStyle.None;
            GameMainTitleInterface = gameMainTitleInterface;

            //获取UI
            visualTree.CloneTree(this);

            //获取根界面
            GameSettingsPanel = this.Q<VisualElement>("GameSettingsPanel");
            GameSystemSettingsButton = GameSettingsPanel.Q<Button>("GameSystemSettingsButton");
            GamePlaySettingsButton = GameSettingsPanel.Q<Button>("GamePlaySettingsButton");
            GamePatchesSettingsButton = GameSettingsPanel.Q<Button>("GamePatchesSettingsButton");

            //具体界面
            //系统设置
            GameSystemSettingsInterface = new(this, GameMainTitleInterface.gameSystemSettingsInterface);
            GameMainTitleInterface.GameInterfaceContainer.Add(GameSystemSettingsInterface);
            GameSystemSettingsButton.clicked += () => GameMainTitleInterface.OpenNewPanel(GameSystemSettingsInterface);
            //游戏玩法
            GamePlaySettingsInterface = new(this, GameMainTitleInterface.gamePlaySettingsInterface);
            GameMainTitleInterface.GameInterfaceContainer.Add(GamePlaySettingsInterface);
            GamePlaySettingsButton.clicked += () => GameMainTitleInterface.OpenNewPanel(GamePlaySettingsInterface);
            //游戏补丁
            GamePatchesSettingsInterface = new(this, GameMainTitleInterface.gamePatchesSettingsInterface);
            GameMainTitleInterface.GameInterfaceContainer.Add(GamePatchesSettingsInterface);
            GamePatchesSettingsButton.clicked += () => GameMainTitleInterface.OpenNewPanel(GamePatchesSettingsInterface);
        }
        #endregion
    }
}
