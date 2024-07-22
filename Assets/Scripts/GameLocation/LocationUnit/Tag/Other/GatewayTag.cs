using UnityEngine;

namespace THLL.LocationSystem.Tags
{
    [CreateAssetMenu]
    public class GatewayTag : LocUnitTag
    {
        #region 方法
        //静态构造函数
        static GatewayTag()
        {
            tagName = "出入口标签";
            tagDescription = "出入口标签，允许本地点与其父地点进行往返，移动耗时固定为0";
        }
        //应用标签方法
        public override void ApplyTag(LocUnit target, LocUnitDb globalData = null, LocUnitConnDb globalConnData = null)
        {
            base.ApplyTag(target, globalData, globalConnData);

            //判断传入数据是否为空，以及目标是否包含父级
            if (globalData != null && globalConnData != null && target.Parent != null)
            {
                //若全局数据与连接数据均不为空，且地点包含父级，则添加子级到其父级的双向链接，耗时固定为0
                target.Connections[target.Parent] = 0;
                target.Parent.Connections[target] = 0;
                globalConnData.AddConnection(target, target.Parent, 0);
                globalConnData.AddConnection(target.Parent, target, 0);
            }
        }
        #endregion
    }
}
