using System.Linq;
using UnityEngine.UIElements;

namespace THLL.UISystem
{
    public class BaseGamePanel : VisualElement
    {
        #region 静态数据
        //当前显示的面板
        public static BaseGamePanel CurrentPanel { get; set; }
        #endregion

        #region 数据
        //所属界面
        public BaseGameInterface BelongingInterface { get; private set; }
        //根面板
        public VisualElement RootPanel { get; private set; }
        //容器面板
        public VisualElement ContainerPanel { get; private set; }
        //父级面板
        public VisualElement ParentPanel { get; private set; }
        //标题
        public Label TitleLabel { get; private set; }
        #endregion

        #region 构建及初始化与相关方法
        //构建函数
        public BaseGamePanel(BaseGameInterface @interface, VisualTreeAsset visualTreeAsset, VisualElement parentPanel)
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
            GameUI.SetVisualElementAllTransitionAnimationDuration(this, 0.5f);
            //设置父级面板
            ParentPanel = parentPanel;
            parentPanel?.Add(this);
            //初始化
            GetVisualElements();
            RegisterMethods();
        }
        //获取元素
        protected virtual void GetVisualElements()
        {
            //找寻一下根面板
            RootPanel = this.Q<VisualElement>("RootPanel");
            //找寻一下容器面板
            ContainerPanel = this.Q<VisualElement>("ContainerPanel");
            //找寻一下标题
            TitleLabel = this.Q<Label>("Title");
        }
        //注册方法
        protected virtual void RegisterMethods()
        {
            //注册一下关闭按钮的关闭方法
            this.Query<Button>("CloseButton").ForEach(button => button.clicked += () => HidePanel());
        }
        #endregion

        #region 显示及关闭面板
        //切换面板显示
        public virtual void SwitchPanel()
        {
            //首先判断是否是自己
            if (CurrentPanel == this)
            {
                //如果是，隐藏
                HidePanel();
            }
            else
            {
                //如果不是，显示
                ShowPanel();
            }
        }
        //显示面板
        protected virtual void ShowPanel()
        {
            //首先判断当前显示面板是否为自己
            if (CurrentPanel == this)
            {
                //如果是，则直接返回
                return;
            }
            //如果不是，则关闭当前显示的面板
            CurrentPanel?.HidePanel();
            //设置当前显示的面板为自己
            CurrentPanel = this;

            //让自己显示
            style.display = DisplayStyle.Flex;
            //然后，调整不透明度为1
            style.opacity = 1f;
        }
        //关闭面板
        protected virtual void HidePanel()
        {
            //首先判断当前显示面板是否为自己
            if (CurrentPanel != this)
            {
                //如果不是，则直接返回
                return;
            }
            //设置当前显示的面板为null
            CurrentPanel = null;
            //调整不透明度为0
            style.opacity = 0f;
            //然后，让自己隐藏
            style.display = DisplayStyle.None;
        }
        #endregion
    }
}
