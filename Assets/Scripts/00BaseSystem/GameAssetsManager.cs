using System;
using System.Collections;
using System.IO;
using THLL.CharacterSystem;
using THLL.SceneSystem;
using THLL.UISystem;
using UnityEngine;

namespace THLL.BaseSystem
{
    public class GameAssetsManager : Singleton<GameAssetsManager>
    {
        #region 数据
        //默认背景图(白)
        public Sprite DefaultBackground;
        //默认头像
        public Sprite DefaultAvatar;
        //默认立绘
        public Sprite DefaultPortrait;
        //资源加载事件
        public event Action OnAllResourcesLoaded;
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

            //启动数据加载流程
            LoadAllSceneData();
            LoadAllCharacterData();

            //数据加载结束后启动资源加载流程
            StartCoroutine(LoadAllRootSceneBackgroundsCoroutine());
        }
        #endregion

        #region 资源加载方法
        //加载所有场景数据
        public void LoadAllSceneData()
        {
            //数据存储路径
            string dataPath = Path.Combine(Application.streamingAssetsPath, "Scene");
            //判断路径是否存在
            if (!Directory.Exists(dataPath))
            {
                //若不存在，报错并退出
                GameHistory.LogError("Scene data path not exists: " + dataPath);
                Application.Quit();
            }
            //确认路径存在后，获取所有文件
            string[] allFilePaths = Directory.GetFiles(dataPath, "*.json", SearchOption.AllDirectories);
            //遍历所有文件
            foreach (string filePath in allFilePaths)
            {
                //检测是否为目标数据文件
                if (Path.GetFileNameWithoutExtension(filePath).StartsWith("SceneData"))
                {
                    //若是，加载并生成实例
                    new Scene(filePath, GameScene.SceneDB);
                }
            }
            //场景数据一次加载完成后，进行数据库的初始化
            GameScene.SceneDB.Init();
            //并进行记录
            GameHistory.LogNormal($"场景数据加载完成，共加载{GameScene.SceneDB.Count}个场景数据。");
        }
        //加载所有角色数据
        public void LoadAllCharacterData()
        {
            //数据存储路径
            string dataPath = Path.Combine(Application.streamingAssetsPath, "Character");
            //判断路径是否存在
            if (!Directory.Exists(dataPath))
            {
                //若不存在，报错并退出
                GameHistory.LogError("Character data path not exists: " + dataPath);
                Application.Quit();
            }
            //确认路径存在后，获取所有文件
            string[] allFilePaths = Directory.GetFiles(dataPath, "*.json", SearchOption.AllDirectories);
            //遍历所有文件
            foreach (string filePath in allFilePaths)
            {
                //检测是否为目标数据文件
                if (Path.GetFileNameWithoutExtension(filePath).StartsWith("CharacterData"))
                {
                    //若是，加载并生成实例
                    new Character(filePath, GameCharacter.CharacterDB);
                }
            }
            //角色数据一次加载完成后，进行数据库的初始化
            GameCharacter.CharacterDB.Init();
            //并进行记录
            GameHistory.LogNormal($"角色数据加载完成，共加载{GameCharacter.CharacterDB.Count}个角色数据。");
        }
        #endregion

        #region 辅助方法
        //加载根场景背景图片协程
        private IEnumerator LoadAllRootSceneBackgroundsCoroutine()
        {
            //对根场景进行遍历
            foreach (Scene scene in GameScene.SceneDB.RootSceneStorage.Values)
            {
                //加载背景图片协程
                yield return scene.LoadBackgroundsCoroutine();
            }
            //全部加载完成后，触发事件
            OnAllResourcesLoaded?.Invoke();
        }
        #endregion 
    }
}
