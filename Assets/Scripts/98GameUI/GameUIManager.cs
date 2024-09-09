using THLL.BaseSystem;
using THLL.UISystem.Settings;

namespace THLL.UISystem
{
    public class GameUIManager : Singleton<GameUIManager>
    {
        #region Unity周期函数
        //Awake
        protected override void Awake()
        {
            //调用基类
            base.Awake();

            //UI管理器比较特殊，直接设定为启用吧
            enabled = true;

            //获取面板
            GetInterfaces();
        }
        //Start
        private void Start()
        {
            //显示主面板
            GameUI.ShowInterface(GameUI.MainTitleInterface, false);
        }
        #endregion

        #region 初始化相关方法
        //获取面板
        private void GetInterfaces()
        {
            //系统相关面板
            GameUI.MainTitleInterface = GetComponentInChildren<MainTitle>();
            GameUI.NewGameInterface = GetComponentInChildren<NewGame>();
            GameUI.SaveAndLoadGameInterface = GetComponentInChildren<SaveAndLoadGame>();
            GameUI.GameSettingsInterface = GetComponentInChildren<GameSettings>();
            GameUI.GameSystemSettingsInterface = GetComponentInChildren<GameSystemSettings>();
            GameUI.GameplaySettingsInterface = GetComponentInChildren<GameplaySettings>();
            GameUI.GamePatchesSettingsInterface = GetComponentInChildren<GamePatchesSettings>();
            //游玩面板
            GameUI.BasicPlayInterface = GetComponentInChildren<BasicPlay>();
            //辅助面板
            GameUI.BackgroundLayer = GetComponentInChildren<BackgroundLayer>();
            GameUI.AnimationLayer = GetComponentInChildren<AnimationLayer>();
        }
        #endregion
    }
}
