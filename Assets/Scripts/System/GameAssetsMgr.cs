using System.Collections;
using System.Collections.Generic;
using System.Linq;
using THLL.CharacterSystem;
using THLL.LocationSystem;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace THLL.BaseSystem
{
    public class GameAssetsMgr : Singleton<GameAssetsMgr>
    {
        #region 周期函数
        protected override void Awake()
        {
            //加载时不销毁
            DontDestroyOnLoad(this);

            //依次加载资源
            StartCoroutine(LoadResourcesSequentially());
        }
        #endregion

        #region 资源加载方法
        //依次加载资源
        private IEnumerator LoadResourcesSequentially()
        {
            //加载所需加载的地点
            yield return LoadLocUnitResource();

            //加载所需加载的角色
            yield return LoadCharacterResource();
        }
        //加载地点单元
        private IEnumerator LoadLocUnitResource()
        {
            //协程返回值
            bool isComplete = false;
            //计数
            int number = 0;

            //获取操作句柄
            AsyncOperationHandle handle = Addressables.LoadAssetsAsync<LocUnitData>
                (
                "Location",
                (resource) =>
                {
                    //加载
                    //生成实例并移交至数据库中
                    LocUnit locUnit = new(resource);
                    GameLocation.LocUnitDb.Add(resource, locUnit);
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
                }
                else
                {
                    //TODO:若不成功
                }
            };

            //返回
            yield return new WaitUntil(() => isComplete);
        }
        //加载角色资源方法
        private IEnumerator LoadCharacterResource()
        {
            //协程返回值
            bool isComplete = false;
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
                    //若成功
                }
                else
                {
                    //TODO:若不成功
                }
            };

            //返回
            yield return new WaitUntil(() => isComplete);
        }
        #endregion
    }
}
