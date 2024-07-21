using THLL.BaseSystem;
using UnityEngine;

namespace THLL.LocationSystem.Tags
{
    [CreateAssetMenu(menuName = "GameData/Location/LocUnitDataTags/BaseTag", order = 0)]
    public class LocUnitTag : BaseGameData
    {
        #region 数据
        //是否可被继承
        [SerializeField]
        protected bool isInherited;
        public bool IsInherited => isInherited;
        #endregion

        #region 函数
        //构建函数
        public LocUnitTag()
        {
            //基础属性值设定
            Package = "Core";
            Category = "LocationTag";
            Author = "TapirFishEgret";
            //默认设置不可继承
            isInherited = false;
        }
        #endregion
    }
}
