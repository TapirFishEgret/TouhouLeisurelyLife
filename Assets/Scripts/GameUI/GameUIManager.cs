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
        }
        #endregion

        #region 初始化及相关方法
        //初始化
        protected override void Init()
        {
            //获取各个面板
            //系统相关面板
            GameUI.MainTitleInterface = GetComponentInChildren<MainTitle>();
            GameUI.NewGameInterface = GetComponentInChildren<NewGame>();
            GameUI.SaveAndLoadGameInterface = GetComponentInChildren<SaveAndLoadGame>();
            GameUI.GameSettingsInterface = GetComponentInChildren<GameSettings>();
            GameUI.GameSystemSettingsInterface = GetComponentInChildren<GameSystemSettings>();
            GameUI.GamePlaySettingsInterface = GetComponentInChildren<GamePlaySettings>();
            GameUI.GamePatchesSettingsInterface = GetComponentInChildren<GamePatchesSettings>();
            //游玩面板
            GameUI.PlayInterface = GetComponentInChildren<Play>();
            //辅助面板
            GameUI.BackgroundLayer = GetComponentInChildren<BackgroundLayer>();
            GameUI.AnimationLayer = GetComponentInChildren<AnimationLayer>();

            //显示主面板
            GameUI.ShowInterface(GameUI.MainTitleInterface, false);
        }
        #endregion
    }
}
