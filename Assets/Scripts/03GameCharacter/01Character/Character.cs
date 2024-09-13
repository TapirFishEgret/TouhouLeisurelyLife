using THLL.BaseSystem;
using UnityEngine;

namespace THLL.CharacterSystem
{
    public class Character : BaseGameEntity<CharacterData>
    {
        #region 来自角色数据类的数据成员
        //角色所属系列
        public string OriginatingSeries => BaseGameData.OriginatingSeries;
        //角色所属组织
        public string Affiliation => BaseGameData.Affiliation;
        //角色头像
        public Sprite Avatar => BaseGameData.Avatar;
        //角色立绘
        public Sprite Portrait => BaseGameData.Portrait;
        #endregion

        #region 角色自身数据成员
        //当前所处地点
        public Scene Location { get; private set; }
        #endregion

        #region 方法
        //构造函数
        public Character(CharacterData data) : base(data) { }
        //移动
        public void MoveTo(Scene end)
        {
            //TODO:理论上移动需要有过程，但暂时不做，仅进行位置及数据更改
            //从原地点移除
            Location.Character.Remove(this);
            //更改当前地点
            Location = end;
            //把自己加到当前地点的角色列表中
            Location.Character.Add(this);
        }
        #endregion
    }
}
