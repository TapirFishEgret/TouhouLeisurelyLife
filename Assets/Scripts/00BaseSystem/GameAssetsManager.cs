using System;
using System.Collections.Generic;
using THLL.CharacterSystem;
using THLL.GeographySystem;
using THLL.UISystem;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace THLL.BaseSystem
{
    public class GameAssetsManager : Singleton<GameAssetsManager>
    {
        #region 数据
        //资源加载事件
        public event Action OnAllResourcesLoaded;
        //资源加载队列
        public Queue<Action> ResourcesLoadQueue { get; private set; } = new();
        #endregion

        #region 周期函数
        //Awake
        protected override void Awake()
        {
            //父类Awake方法
            base.Awake();
            //将自己设定为启用
            enabled = true;
        }
        //Start
        private void Start()
        {
            //显示加载界面
            GameUI.AnimationLayer.ShowLoadingScreen();

            //依次添加加载资源方法
            ResourcesLoadQueue.Enqueue(LoadLocationResource);
            ResourcesLoadQueue.Enqueue(LoadCharacterResource);

            //开始加载资源
            LoadNextResource();
        }
        #endregion

        #region 资源加载方法
        //加载下一个资源
        private void LoadNextResource()
        {
            //检测队列长度
            if (ResourcesLoadQueue.Count > 0)
            {
                //若仍有方法未执行，获取方法
                Action method = ResourcesLoadQueue.Dequeue();
                //执行
                method.Invoke();
            }
            else
            {
                //若方法执行完成，发布通知
                OnAllResourcesLoaded?.Invoke();
            }
        }
        //加载地点单元
        private void LoadLocationResource()
        {
            //计数
            int number = 0;

            //获取操作句柄
            AsyncOperationHandle handle = Addressables.LoadAssetsAsync<LocationData>
                (
                "Location",
                (resource) =>
                {
                    //加载
                    //生成实例并移交至数据库中
                    Location locUnit = new(resource);
                    GameLocation.LocationDb.Add(resource, locUnit);
                    //计数
                    number++;
                }
                );

            //设置资源加载完成时操作
            handle.Completed += (operation) =>
            {
                //检测资源加载是否成功
                if (operation.Status == AsyncOperationStatus.Succeeded)
                {
                    //若成功，对地点数据进行初始化
                    GameLocation.Init();
                    //初始化结束后，加载下一个资源
                    LoadNextResource();
                }
                else
                {
                    //TODO:若不成功
                }
            };
        }
        //加载角色资源方法
        private void LoadCharacterResource()
        {
            //计数
            int number = 0;

            //获取操作句柄
            AsyncOperationHandle handle = Addressables.LoadAssetsAsync<CharacterData>
                (
                "Character",
                (resource) =>
                {
                    //加载
                    //生成实例并移交至数据库中
                    Character character = new(resource);
                    GameCharacter.CharacterDb.Add(resource, character);
                    //计数
                    number++;
                }
                );

            //设置资源加载完成时操作
            handle.Completed += (operation) =>
            {
                //检测资源加载是否成功
                if (operation.Status == AsyncOperationStatus.Succeeded)
                {
                    //若成功，加载下一个资源
                    LoadNextResource();
                }
                else
                {
                    //TODO:若不成功
                }
            };
        }
        #endregion
    }
}
