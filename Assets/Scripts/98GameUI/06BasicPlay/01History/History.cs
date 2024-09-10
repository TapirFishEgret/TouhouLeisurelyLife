using System.Collections.Generic;
using THLL.BaseSystem;
using UnityEngine.UIElements;

namespace THLL.UISystem
{
    public class History : BaseGamePanel
    {
        #region 构造及初始化及相关方法
        //构造函数
        public History(BaseGameInterface @interface,
            VisualTreeAsset visualTreeAsset,
            VisualElement parent = null,
            List<BaseGamePanel> storage = null)
            : base(@interface, visualTreeAsset, parent, storage) { }
        #endregion

        #region 显示及隐藏方法
        //显示
        public override void ShowPanel(float animationDuration)
        {
            //基础的显示
            base.ShowPanel(animationDuration);
            //进阶的显示，添加历史记录进去
            foreach (Label log in GameHistory.Logs)
            {
                //添加到面板中
                ContainerPanel.Add(log);
            }
        }
        //隐藏
        public override void HidePanel(float animationDuration)
        {
            //基础的隐藏
            base.HidePanel(animationDuration);
            //进阶的隐藏，清空面板
            ContainerPanel.Clear();
        }
        #endregion
    }
}
