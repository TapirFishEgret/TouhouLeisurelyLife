using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public Dictionary<string, Sprite> AvatarsDict
        {
            get
            {
                if (Data.AvatarsDict.Count == 0)
                {
                    //如果字典为空，则返回默认头像
                    return new Dictionary<string, Sprite>() { { "0", GameAssetsManager.Instance.DefaultAvatar } };
                }
                else
                {
                    //否则返回字典
                    return Data.AvatarsDict;
                }
            }
        }
        //立绘字典
        public Dictionary<string, Sprite> PortraitsDict
        {
            get
            {
                if (Data.PortraitsDict.Count == 0)
                {
                    //如果字典为空，则返回默认立绘
                    return new Dictionary<string, Sprite>() { { "0", GameAssetsManager.Instance.DefaultPortrait } };
                }
                else
                {
                    //否则返回字典
                    return Data.PortraitsDict;
                }
            }
        }
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
            //将自身添加到数据库中
            characterDb.Add(this);
        }
        #endregion

        #region 资源相关方法
        //加载头像，协程版本
        public IEnumerator LoadAvatarsCoroutine(Action<string, Sprite> onAvatarLoaded = null, Action onAllAvatarsLoaded = null)
        {
            //返回协程
            yield return Data.LoadAvatarsCoroutine(DataDirectoryPath, onAvatarLoaded, onAllAvatarsLoaded);
        }
        //加载头像，异步版本
        public async Task LoadAvatarsAsync(Action<string, Sprite> onAvatarLoaded = null, Action onAllAvatarsLoaded = null)
        {
            //调用异步函数
            await Data.LoadAvatarsAsync(DataDirectoryPath, onAvatarLoaded, onAllAvatarsLoaded);
        }
        //加载立绘，协程版本
        public IEnumerator LoadPortraitsCoroutine(Action<string, Sprite> onPortraitLoaded = null, Action onAllPortraitsLoaded = null)
        {
            //返回协程
            yield return Data.LoadPortraitsCoroutine(DataDirectoryPath, onPortraitLoaded, onAllPortraitsLoaded);
        }
        //加载立绘，异步版本
        public async Task LoadPortraitsAsync(Action<string, Sprite> onPortraitLoaded = null, Action onAllPortraitsLoaded = null)
        {
            //调用异步函数
            await Data.LoadPortraitsAsync(DataDirectoryPath, onPortraitLoaded, onAllPortraitsLoaded);
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
