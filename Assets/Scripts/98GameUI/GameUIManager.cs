using System.Collections;
using System.Collections.Generic;
using THLL.BaseSystem;
using THLL.UISystem.Settings;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.UISystem
{
    public class GameUIManager : Singleton<GameUIManager>
    {
        #region 数据
        //动画协程库
        private Dictionary<VisualElement, Coroutine> AnimationCoroutinesDict { get; set; } = new();
        #endregion

        #region Unity周期函数
        //Awake
        protected override void Awake()
        {
            //调用基类
            base.Awake();

            //UI管理器比较特殊，直接设定为启用吧
            enabled = true;

            //获取面板
            GetInterfaces();
        }
        //Start
        private void Start()
        {
            //显示主面板
            GameUI.MainTitleInterface.Show(false);
        }
        #endregion

        #region 初始化相关方法
        //获取面板
        private void GetInterfaces()
        {
            //系统相关面板
            GameUI.MainTitleInterface = GetComponentInChildren<MainTitle>();
            GameUI.NewGameInterface = GetComponentInChildren<NewGame>();
            GameUI.SaveAndLoadGameInterface = GetComponentInChildren<SaveAndLoadGame>();
            GameUI.GameSettingsInterface = GetComponentInChildren<GameSettings>();
            GameUI.GameSystemSettingsInterface = GetComponentInChildren<GameSystemSettings>();
            GameUI.GameplaySettingsInterface = GetComponentInChildren<GameplaySettings>();
            GameUI.GamePatchesSettingsInterface = GetComponentInChildren<GamePatchesSettings>();
            //游玩面板
            GameUI.BasicPlayInterface = GetComponentInChildren<BasicPlay>();
            //辅助面板
            GameUI.BackgroundLayer = GetComponentInChildren<BackgroundLayer>();
            GameUI.AnimationLayer = GetComponentInChildren<AnimationLayer>();
        }
        #endregion

        #region 动画协程库相关方法
        //添加协程
        public void AddCoroutine(VisualElement key, IEnumerator routine)
        {
            //检测对应元素是否有协程在运行
            if (IsRunningCoroutine(key))
            {
                //停止当前协程
                StopCoroutine(AnimationCoroutinesDict[key]);
            }
            //创建协程
            Coroutine coroutine = StartCoroutine(routine);
            //添加到字典
            AnimationCoroutinesDict[key] = coroutine;
        }
        //移除协程
        public void RemoveCoroutine(VisualElement key)
        {
            if (AnimationCoroutinesDict.Remove(key, out Coroutine coroutine))
            {
                //停止协程
                StopCoroutine(coroutine);
            }
        }
        //检测某个视觉元素是否有协程在运行
        public bool IsRunningCoroutine(VisualElement key)
        {
            return AnimationCoroutinesDict.ContainsKey(key);
        }
        //停止所有在库中的协程
        public void StopAllCoroutinesInDict()
        {
            foreach (var coroutine in AnimationCoroutinesDict.Values)
            {
                StopCoroutine(coroutine);
            }
            AnimationCoroutinesDict.Clear();
        }
        #endregion
    }
}
