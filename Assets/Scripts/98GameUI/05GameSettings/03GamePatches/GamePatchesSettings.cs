using UnityEngine.UIElements;

namespace THLL.UISystem.Settings
{
    public class GamePatchesSettings : BaseGameUI
    {
        #region 自身数据
        //根界面
        public VisualElement RootPanel { get; private set; }
        //游戏地点按钮
        public Button GameLocationsButton { get; private set; }
        //游戏角色按钮
        public Button GameCharactersButton { get; private set; }
        //返回按钮
        public Button ReturnButton { get; private set; }
        #endregion

        #region 初始化与相关方法
        //绑定UI元素
        protected override void GetVisualElements()
        {
            RootPanel = Document.rootVisualElement.Q<VisualElement>("RootPanel");
            GameLocationsButton = RootPanel.Q<Button>("GameLocationsButton");
            GameCharactersButton = RootPanel.Q<Button>("GameCharactersButton");
            ReturnButton = RootPanel.Q<Button>("ReturnButton");
        }
        #endregion
    }
}
