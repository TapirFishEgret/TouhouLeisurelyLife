using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using THLL.BaseSystem;
using THLL.SceneSystem;
using UnityEngine;

namespace THLL.CharacterSystem
{
    public class Character : BaseGameEntity<CharacterData>
    {
        #region 基础数据
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
        //获取头像
        public Sprite GetAvatar(string avatarName = "0")
        {
            //创建头像变量
            Sprite avatar;

            //检测头像数据
            if (Data.AvatarsDict.Count == 0)
            {
                //如果头像数据为空，则赋值默认头像
                avatar = GameCharacter.DefaultAvatar;
            }
            else if (!Data.AvatarsDict.TryGetValue(avatarName, out avatar))
            {
                //如果有头像数据，但没有指定头像，则尝试指定“0”
                if (!Data.AvatarsDict.TryGetValue("0", out avatar))
                {
                    //若依然没有，随机头像
                    avatar = Data.AvatarsDict.Values.First();
                }
            }

            //返回
            return avatar;
        }
        //获取立绘
        public Sprite GetPortrait(string portraitName = "0")
        {
            //创建立绘变量
            Sprite portrait;

            //检测立绘数据
            if (Data.PortraitsDict.Count == 0)
            {
                //如果立绘数据为空，则赋值默认立绘
                portrait = GameCharacter.DefaultPortrait;
            }
            else if (!Data.PortraitsDict.TryGetValue(portraitName, out portrait))
            {
                //如果有立绘数据，但没有指定立绘，则尝试指定“0”
                if (!Data.PortraitsDict.TryGetValue("0", out portrait))
                {
                    //若依然没有，随机立绘
                    portrait = Data.PortraitsDict.Values.First();
                }
            }

            //返回
            return portrait;
        }
        #endregion

        #region 初始化及相关方法
        //构造函数
        public Character(string filePath) : base(filePath)
        {
            //将自己添加到数据库中
            GameCharacter.AddCharacter(this);
        }
        #endregion

        #region 资源相关方法
        //加载资源，协程版本
        public IEnumerator LoadAllResourcesCoroutine
            (Action<string, Sprite> onAvatarLoaded = null, 
            Action onAllAvatarsLoaded = null, 
            Action<string, Sprite> onPortraitLoaded = null, 
            Action onAllPortraitsLoaded = null)
        {
            //首先加载头像
            yield return Data.LoadAvatarsCoroutine(DataDirectory, onAvatarLoaded, onAllAvatarsLoaded);
            //加载立绘
            yield return Data.LoadPortraitsCoroutine(DataDirectory, onPortraitLoaded, onAllPortraitsLoaded);
        }
        //加载资源，异步版本
        public async Task LoadAllResourcesAsync
            (Action<string, Sprite> onAvatarLoaded = null, 
            Action onAllAvatarsLoaded = null, 
            Action<string, Sprite> onPortraitLoaded = null, 
            Action onAllPortraitsLoaded = null)
        {
            //首先加载头像
            await Data.LoadAvatarsAsync(DataDirectory, onAvatarLoaded, onAllAvatarsLoaded);
            //加载立绘
            await Data.LoadPortraitsAsync(DataDirectory, onPortraitLoaded, onAllPortraitsLoaded);
        }
        //卸载所有资源
        public void UnloadAllResources()
        {
            //使用数据类中的卸载方法
            Data.UnloadAllResources();
        }
        #endregion

        #region 移动相关
        //当前场景
        public Scene CurrentScene { get; set; }
        //是否正在移动
        public bool IsMoving => TargetScene != null;
        //目标场景
        private Scene TargetScene { get; set; }
        //剩余路径
        private Queue<Scene> Path { get; set; }
        //剩余用时
        private int RemainingTime { get; set; }
        //前往某场景
        public void MoveToScene(Scene scene)
        {
            //检测目标场景与当前场景是否一致
            if (scene == CurrentScene)
            {
                //如果一致，则直接返回
                return;
            }
            //设置目标场景
            TargetScene = scene;
            //设置路径
            Path = GameScene.GetShorestPath(CurrentScene, TargetScene);
            //取出第一个场景
            Path.Dequeue();
            //设置剩余时间
            RemainingTime = 0;
            //更新一次移动
            UpdateMove(0);
        }
        //更新移动
        public void UpdateMove(int passedSeconds)
        {
            //检测是否已经到达目的地
            if (CurrentScene == TargetScene)
            {
                //如果到达目的地，则清空目标场景
                TargetScene = null;
                //清空路径
                Path = null;
                //清空剩余时间
                RemainingTime = 0;
                //返回
                return;
            }

            //若未达到目的地，检测是否有路径
            if (Path != null && Path.Count > 0)
            {
                //检测是否有剩余时间
                if (RemainingTime > 0)
                {
                    //更新剩余时间
                    RemainingTime -= passedSeconds;
                }

                //检测剩余时间是否小于零
                if (RemainingTime <= 0)
                {
                    //若是，移动，将自身从场景角色列表中移除
                    CurrentScene.CharactersInScene.Remove(this);
                    //重排
                    CurrentScene.CharactersInScene.Sort((a, b) => a.Data.SortOrder.CompareTo(b.Data.SortOrder));
                    //移动到下一个场景
                    CurrentScene = Path.Dequeue();
                    //将自身添加到下一个场景角色列表中
                    CurrentScene.CharactersInScene.Add(this);
                    //重排
                    CurrentScene.CharactersInScene.Sort((a, b) => a.Data.SortOrder.CompareTo(b.Data.SortOrder));
                    //重置剩余时间
                    RemainingTime = CurrentScene.Connections[TargetScene];
                }
            }
        }
        #endregion
    }
}
