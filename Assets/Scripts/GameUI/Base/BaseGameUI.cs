﻿using THLL.BaseSystem;
using UnityEngine.UIElements;

namespace THLL.UISystem
{
    public class BaseGameUI : GameBehaviour
    {
        #region 数据
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
            Document.rootVisualElement.Query<Button>("ReturnButton").ForEach(button => button.clicked += () => GameUI.ReturnInterface());
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
    }
}
