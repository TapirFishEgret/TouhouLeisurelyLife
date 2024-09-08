using System.Collections;
using System.Collections.Generic;
using System.Linq;
using THLL.BaseSystem;
using THLL.GeographySystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.UISystem
{
    public class BackgroundLayer : BaseGameInterface
    {
        #region 自身数据
        //UI相关
        //背景
        public VisualElement Background { get; private set; }

        //背景图相关
        //当前背景所剩时间
        public float Duration { get; private set; } = 5f;
        #endregion

        #region 初始化与相关方法
        //初始化
        protected override void Start()
        {
            //父类Start方法
            base.Start();

            //将协程启用添加到资源加载完成事件中
            GameAssetsManager.Instance.OnAllResourcesLoaded += () => CycleBackground(1.0f);
        }
        //获取视觉元素
        protected override void GetVisualElements()
        {
            Background = Document.rootVisualElement.Q<VisualElement>("Background");
        }
        #endregion

        #region 其他方法
        //循环背景图
        public void CycleBackground(float animationDuration)
        {
            //获取协程
            Coroutine coroutine = StartCoroutine(CycleBackgroundCoroutine(animationDuration));
            //存储协程，循环背景图仅仅会存在一份，所以直接以方法名为Key
            CoroutineDic["CycleBackground"] = coroutine;
        }
        //切换背景图
        public void SwitchBackground(Location location, float animationDuration)
        {
            //获取协程
            Coroutine coroutine = StartCoroutine(SwitchBackgroundCoroutine(location, animationDuration));
            //存储协程，同理仅存储一份
            CoroutineDic["SwitchBackground"] = coroutine;
        }
        #endregion

        #region 协程方法
        //背景图循环方法本体
        private IEnumerator CycleBackgroundCoroutine(float animationDuration)
        {
            //首先获取可用地点
            List<Location> locations = GameLocation.LocationDb.Datas.ToList();

            //始终循环
            while (true)
            {
                //判断剩余时间
                if (Duration > 0)
                {
                    //若大于0，不进行更换，时间自减
                    Duration -= Time.deltaTime;
                    //返回空以配合协程
                    yield return null;
                }
                else
                {
                    //若大于0，更换背景图
                    //首先获取目标地点
                    Location location = locations[Random.Range(0, locations.Count)];

                    //然后切换背景图，耗时1s
                    SwitchBackground(location, animationDuration);

                    //更新持续时间，测试期间固定5s
                    Duration = 5f;
                }
            }
        }
        //切换背景图本体
        private IEnumerator SwitchBackgroundCoroutine(Location location, float animationDuration)
        {
            //更改背景动画延迟为设定时间
            GameUI.SetVisualElementAllTransitionAnimationDuration(Background, animationDuration);

            //更改背景图不透明度为0
            Background.style.opacity = 0;
            //协程等待1s(动画用时)
            yield return new WaitForSeconds(animationDuration);

            //更换背景图
            Background.style.backgroundImage = new StyleBackground(location.Background);
            //更新主面板
            GameUI.MainTitleInterface.LocationLabel.text = location.Name;
            //协程等待到当前帧结束
            yield return new WaitForEndOfFrame();

            //修改不透明度为1
            Background.style.opacity = 1;
            //协程等待1s(动画用时)
            yield return new WaitForSeconds(animationDuration);
        }
        #endregion
    }
}
