using THLL.CharacterSystem;
using THLL.LocationSystem;
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

            //加载资源
            using (ExecutionTimer timer = new("地点资源加载"))
            {
                LoadLocUnitResource("Location");
            }
            using (ExecutionTimer timer1 = new("角色资源加载"))
            {
                LoadCharacterResource("Character");
            }
        }
        #endregion

        #region 资源加载方法
        //加载地点单元
        public void LoadLocUnitResource(string key)
        {
            //计数
            int number = 0;

            //获取操作句柄
            AsyncOperationHandle handle = Addressables.LoadAssetsAsync<LocUnitData>
                (
                key,
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
        public void LoadCharacterResource(string key)
        {
            //计数
            int number = 0;

            //获取操作句柄
            AsyncOperationHandle handle = Addressables.LoadAssetsAsync<CharacterData>
                (
                key,
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
