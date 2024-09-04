using THLL.BaseSystem;
using UnityEngine.UIElements;

namespace THLL.UISystem
{
    public class BaseGameUI : GameBehaviour
    {
        #region 数据
        public UIDocument Document { get; protected set; }
        #endregion

        #region 周期函数
        protected override void Start()
        {
            //调用父类周期函数
            base.Start();
            //执行初始化方法
            Init();
        }
        #endregion

        #region 初始化及相关方法
        //初始化
        protected virtual void Init()
        {
            //获取UIDocument组件
            Document = GetComponent<UIDocument>();
            //获取UI元素
            GetVisualElements();
            //绑定UI方法
            RegisterMethods();

            //检索所有的返回按钮并添加通用返回方法
            Document.rootVisualElement.Query<Button>("ReturnButton").ForEach(button => button.clicked += () => GameUI.ReturnInterface());
        }
        //获取UI元素
        protected virtual void GetVisualElements()
        {

        }
        //绑定各项方法
        protected virtual void RegisterMethods()
        {

        }
        #endregion
    }
}
