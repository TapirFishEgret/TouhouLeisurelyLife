using UnityEngine.UIElements;

namespace THLL.GameUI.GameMainTitleInterface
{
    public class GameStartInterface : VisualElement
    {
        #region 自身数据
        //主界面
        public GameMainTitleInterface GameMainTitleInterface { get; private set; }
        //根界面
        public VisualElement GameStartPanel { get; private set; }
        #endregion

        #region 构造函数与初始化与相关方法
        //构造函数
        public GameStartInterface(GameMainTitleInterface gameMainTitleInterface, VisualTreeAsset visualTree)
        {
            //设定自身
            style.flexGrow = 1;
            style.display = DisplayStyle.None;
            GameMainTitleInterface = gameMainTitleInterface;

            //获取UI
            visualTree.CloneTree(this);

            //获取根界面
            GameStartPanel = this.Q<VisualElement>("GameStartPanel");
        }
        #endregion
    }
}
