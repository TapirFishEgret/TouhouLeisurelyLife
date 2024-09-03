using UnityEngine.UIElements;

namespace THLL.GameUI.GameMainTitleInterface
{
    public class GamePlaySettingsInterface : VisualElement
    {
        #region 自身数据
        //父界面
        public GameSettingsInterface GameSettingsInterface { get; private set; }
        //根界面
        public VisualElement GamePlaySettingsPanel { get; private set; }
        #endregion

        #region 构造函数与初始化与相关方法
        //构造函数
        public GamePlaySettingsInterface(GameSettingsInterface gameSettingsInterface, VisualTreeAsset visualTree)
        {
            //设定自身
            style.flexGrow = 1;
            style.display = DisplayStyle.None;
            GameSettingsInterface = gameSettingsInterface;

            //获取UI
            visualTree.CloneTree(this);

            //获取根界面
            GamePlaySettingsPanel = this.Q<VisualElement>("GamePlaySettingsPanel");
        }
        #endregion
    }
}
