using System.Collections.Generic;
using System.IO;
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
        #region 数据
        //资源类型-需要加载的资源字典
        private Dictionary<GameAssetTypeEnum, List<AssetGroupInfo>> AssetGroupsNeedToLoad { get; set; } = new();
        #endregion

        #region 周期函数
        protected override void Awake()
        {
            //加载时不销毁
            DontDestroyOnLoad(this);

            //加载资源
            using (ExecutionTimer catalogTimer = new("Catalog加载"))
            {
                LoadAllCatalog();
            }
            using (ExecutionTimer assetGroupInfoTimer = new("资源组信息加载"))
            {
                LoadAllAssetGroupInfo();
            }
            using (ExecutionTimer LocationTimer = new("地点资源加载"))
            {
                LoadLocUnitResource();
            }
            using (ExecutionTimer characterTimer = new("角色资源加载"))
            {
                LoadCharacterResource();
            }
        }
        #endregion

        #region 资源加载方法
        //加载所有Catalog
        public void LoadAllCatalog()
        {
            //创建目录变量
            string path = string.Empty;

            //获取目录
            path = Application.streamingAssetsPath;
            //若为编辑器模式，则获取可寻址资源包的构建路径
#if UNITY_EDITOR
            path = Addressables.BuildPath;
#endif
            //获取所有Catalog文件，设置中选择了以JSON为结尾，此处同理
            string[] catalogFiles = Directory.GetFiles(path, "catalog.json");
            //遍历所有文件
            foreach (string catalogFile in catalogFiles)
            {
                //获取具体路径
                string catalogPath = Path.Combine(path, catalogFile).Replace("\\", "/");
                catalogPath = Path.GetFullPath(catalogPath).Replace("\\", "/");
                //使用方法加载catalog
                AsyncOperationHandle handle = Addressables.LoadContentCatalogAsync(catalogPath);

                //当加载完成时
                handle.Completed += (operation) =>
                {
                    //判断加载状态
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        //TODO:若加载成功
                    }
                    else
                    {
                        //TODO:若失败
                    }
                };
            }
        }
        //加载所有资源组信息
        public void LoadAllAssetGroupInfo()
        {
            //计数
            int number = 0;

            //获取操作句柄
            AsyncOperationHandle handle = Addressables.LoadAssetsAsync<AssetGroupInfo>
                (
                "AssetGroupInfo",
                (resource) =>
                {
                    //加载
                    //存放入库
                    if (!AssetGroupsNeedToLoad.ContainsKey(resource.AssetType))
                    {
                        AssetGroupsNeedToLoad[resource.AssetType] = new();
                    }
                    AssetGroupsNeedToLoad[resource.AssetType].Add(resource);
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
                    //TODO:若成功
                }
                else
                {
                    //TODO:若不成功
                }
            };
        }
        //加载地点单元
        public void LoadLocUnitResource()
        {
            //计数
            int number = 0;
            //需要加载的资源的地址
            List<string> loadedAssets = new();

            //遍历所需加载的包
            foreach (AssetGroupInfo assetGroupInfo in AssetGroupsNeedToLoad[GameAssetTypeEnum.Location])
            {
                loadedAssets.Concat(assetGroupInfo.AssetAddresses);
            }

            //获取操作句柄
            AsyncOperationHandle handle = Addressables.LoadAssetsAsync<LocUnitData>
                (
                loadedAssets,
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
        }
        //加载角色资源方法
        public void LoadCharacterResource()
        {
            //计数
            int number = 0;
            //需要加载的资源的地址
            List<string> loadedAssets = new();

            //遍历所需加载的包
            foreach (AssetGroupInfo assetGroupInfo in AssetGroupsNeedToLoad[GameAssetTypeEnum.Character])
            {
                loadedAssets.Concat(assetGroupInfo.AssetAddresses);
            }

            //获取操作句柄
            AsyncOperationHandle handle = Addressables.LoadAssetsAsync<CharacterData>
                (
                loadedAssets,
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
        }
        #endregion
    }
}
