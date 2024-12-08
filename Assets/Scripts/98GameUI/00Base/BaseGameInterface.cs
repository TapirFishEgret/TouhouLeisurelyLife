using System.Collections.Generic;
using THLL.BaseSystem;
using UnityEngine.UIElements;

namespace THLL.UISystem
{
    public class BaseGameInterface : GameBehaviour
    {
        #region 静态数据
        //打开的界面列表
        public static List<BaseGameInterface> OpenedInterfaces { get; } = new();
        #endregion

        #region 数据
        //UI文档组件
        public UIDocument Document { get; protected set; }
        #endregion

        #region 周期函数
        protected override void Awake()
        {
            //父类初始化方法
            base.Awake();
            //获取UIDocument组件
            Document = GetComponent<UIDocument>();
            //获取所有UI元素
            GetVisualElements();
        }
        protected virtual void Start()
        {
            //绑定UI方法
            RegisterMethods();
            //检索所有的返回按钮并添加通用返回方法
            Document.rootVisualElement.Query<Button>("ReturnButton").ForEach(button => button.clicked += () => Return(true));
        }
        #endregion

        #region 初始化及相关方法
        //获取UI元素
        protected virtual void GetVisualElements()
        {

        }
        //绑定各项方法
        protected virtual void RegisterMethods()
        {

        }
        #endregion

        #region 显示及返回界面方法
        //显示
        public virtual void Show(bool needAnimation = true)
        {
            if (needAnimation)
            {
                GameUI.AnimationLayer.CoverOnce(Show);
            }
            else
            {
                Show();
            }
        }
        protected virtual void Show()
        {
            //让打开的界面排序放置底层
            OpenedInterfaces.ForEach(i => i.Document.sortingOrder = -1);
            //让当前界面排序放置最高层
            Document.sortingOrder = 1;
            //添加到打开的界面列表
            OpenedInterfaces.Add(this);
        }
        //返回上一层界面
        public virtual void Return(bool needAnimation = true)
        {
            if (needAnimation)
            {
                GameUI.AnimationLayer.CoverOnce(Return);
            }
            else
            {
                Return();
            }
        }
        protected virtual void Return()
        {
            //检测打开的界面的数量
            if (OpenedInterfaces.Count > 1)
            {
                //若大于一，首先将所有界面置于底层
                OpenedInterfaces.ForEach(i => i.Document.sortingOrder = -1);
                //剔除最后一个界面
                OpenedInterfaces.Remove(OpenedInterfaces[^1]);
                //显示现在的最后一个界面
                OpenedInterfaces[^1].Document.sortingOrder = 1;
            }
        }
        #endregion
    }
}
