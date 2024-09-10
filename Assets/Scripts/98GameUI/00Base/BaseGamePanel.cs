using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace THLL.UISystem
{
    public class BaseGamePanel : VisualElement
    {
        #region 数据
        //所属界面
        public BaseGameInterface BelongingInterface { get; private set; }
        //根面板
        public VisualElement RootPanel { get; private set; }
        //容器面板
        public VisualElement ContainerPanel { get; private set; }
        //父级面板
        public VisualElement ParentPanel { get; private set; }
        //其他面板
        public List<BaseGamePanel> OtherPanels { get; private set; }
        #endregion

        #region 构建及初始化与相关方法
        //构建函数
        public BaseGamePanel(BaseGameInterface @interface, VisualTreeAsset visualTreeAsset, VisualElement parent = null, List<BaseGamePanel> storage = null)
        {
            //指定所属界面
            BelongingInterface = @interface;
            //克隆视觉树到自身
            visualTreeAsset.CloneTree(this);
            //调整自身样式为可延展
            style.flexGrow = 1;
            style.flexShrink = 1;
            //并隐藏自身
            style.display = DisplayStyle.None;
            //将不透明度设置为0
            style.opacity = 0f;
            //并设置自身动画
            style.transitionProperty = Enumerable.Repeat(new StylePropertyName("opacity"), 1).ToList();
            //添加到父级中
            ParentPanel = parent;
            ParentPanel?.Add(this);
            //添加到存储池中
            OtherPanels = storage;
            OtherPanels?.Add(this);
            //初始化
            Init();
        }
        //初始化
        protected virtual void Init()
        {
            //获取元素
            GetVisualElements();
            //注册方法
            RegisterMethods();
        }
        //获取元素
        protected virtual void GetVisualElements()
        {
            //找寻一下根面板
            RootPanel = this.Q<VisualElement>("RootPanel");
            //找寻一下容器面板
            ContainerPanel = this.Q<VisualElement>("ContainerPanel");
        }
        //注册方法
        protected virtual void RegisterMethods()
        {
            //注册一下关闭按钮的关闭方法
            this.Query<Button>("CloseButton").ForEach(button => button.clicked += () => HidePanel(1f));
        }
        #endregion

        #region 显示及关闭面板
        //显示面板
        public virtual void ShowPanel(float animationDuration)
        {
            //首先关闭其他面板显示
            foreach (BaseGamePanel panel in OtherPanels)
            {
                panel.HidePanel(animationDuration);
            }

            //设置动画时长
            GameUI.SetVisualElementAllTransitionAnimationDuration(this, animationDuration);
            //让自己显示
            style.display = DisplayStyle.Flex;
            //然后，调整不透明度为1
            style.opacity = 1f;
        }
        //关闭面板
        public virtual void HidePanel(float animationDuration)
        {
            //设置动画时长
            GameUI.SetVisualElementAllTransitionAnimationDuration(this, animationDuration);
            //调整不透明度为0
            style.opacity = 0f;
            //然后，让自己隐藏
            style.display = DisplayStyle.None;
        }
        #endregion
    }
}
