using THLL.BaseSystem;

namespace THLL.UISystem
{
    public class GameUIManager : Singleton<GameUIManager>
    {
        #region Unity周期函数
        protected override void Start()
        {
            //调用基类函数
            base.Start();

            //设定自身为不销毁
            DontDestroyOnLoad(this);

            //初始化
            Init();
        }
        #endregion

        #region 初始化及相关方法
        //初始化
        private void Init()
        {
            //获取各个面板
            GameUI.BackgroundLayer = GetComponentInChildren<BackgroundLayer>();
            GameUI.MainTitleInterface = GetComponentInChildren<MainTitle>();
            GameUI.NewGameInterface = GetComponentInChildren<NewGame>();
            GameUI.SaveAndLoadGameInterface = GetComponentInChildren<SaveAndLoadGame>();
            GameUI.GameSettingsInterface = GetComponentInChildren<GameSettings>();
            GameUI.GameSystemSettingsInterface = GetComponentInChildren<Settings.GameSystemSettings>();
            GameUI.GamePlaySettingsInterface = GetComponentInChildren<Settings.GamePlaySettings>();
            GameUI.GamePatchesSettingsInterface = GetComponentInChildren<Settings.GamePatchesSettings>();
            GameUI.AnimationLayer = GetComponentInChildren<AnimationLayer>();

            //显示主面板
            GameUI.ShowInterface(GameUI.MainTitleInterface, false);
        }
        #endregion
    }
}
