using THLL.BaseSystem;
using System.Collections.Generic;
using UnityEngine;

namespace THLL.GeographySystem
{
    public class World : BaseGameEntity<WorldData>
    {
        #region 数据
        //世界拥有的域
        public List<Realm> Realms { get; } = new();
        //TODO:世界包含角色
        #endregion

        #region 构造及初始化
        //初始化
        public override void Init()
        {
            //暂时没什么要做的
        }
        #endregion
    }
}
