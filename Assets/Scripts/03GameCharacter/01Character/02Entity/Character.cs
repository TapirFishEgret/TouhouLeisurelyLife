using System.Collections.Generic;
using THLL.BaseSystem;
using UnityEngine;

namespace THLL.CharacterSystem
{
    public class Character : BaseGameEntity<CharacterData>
    {
        #region 数据
        //系列
        public string Series => Data.Series;
        //组织
        public string Group => Data.Group;
        //角色
        public string Chara => Data.Chara;
        //版本
        public string Version => Data.Version;

        //颜色
        public Color Color => Data.Color;
        #endregion

        #region 资源
        //头像字典
        public Dictionary<string, Sprite> AvatarsDict => Data.AvatarsDict;
        //立绘字典
        public Dictionary<string, Sprite> ProtraitsDict => Data.ProtraitsDict;
        #endregion

        #region 初始化及相关方法
        //有参构造函数
        public Character(string filePath, CharacterDatabase characterDb) : base(filePath)
        {
            //调用重载后的配置函数
            Configure(characterDb);
        }
        //配置函数重载
        protected void Configure(CharacterDatabase characterDb)
        {
            //首先完善系列索引
            if (!characterDb.SeriesIndex.ContainsKey(Series))
            {
                characterDb.SeriesIndex.Add(Series, new HashSet<Character>());
            }
            characterDb.SeriesIndex[Series].Add(this);
            //然后完善组织索引
            if (!characterDb.GroupIndex.ContainsKey(Group))
            {
                characterDb.GroupIndex.Add(Group, new HashSet<Character>());
            }
            characterDb.GroupIndex[Group].Add(this);
        }
        #endregion

        #region 资源相关方法
        //获取头像
        public void LoadAvatars(MonoBehaviour mono)
        {
            //通过Mono使用协程加载
            mono.StartCoroutine(Data.LoadAvatarsCoroutine(DataDirectoryPath));
        }
        //获取立绘
        public void LoadProtraits(MonoBehaviour mono)
        {
            //通过Mono使用协程加载
            mono.StartCoroutine(Data.LoadProtraitsCoroutine(DataDirectoryPath));
        }
        //卸载所有资源
        public void UnloadAllResources()
        {
            //使用数据类中的卸载方法
            Data.UnloadAllResources();
        }
        #endregion
    }
}
