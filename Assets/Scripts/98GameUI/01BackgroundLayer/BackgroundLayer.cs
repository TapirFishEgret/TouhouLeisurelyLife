using System.Collections;
using System.Collections.Generic;
using System.Linq;
using THLL.BaseSystem;
using THLL.SceneSystem;
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
        public float Duration { get; private set; } = 180f;
        #endregion

        #region 初始化与相关方法
        //初始化
        protected override void Start()
        {
            //父类Start方法
            base.Start();

            //将协程启用添加到资源加载完成事件中并手动启用一次移动操作
            GameAssetsManager.Instance.OnAllResourcesLoaded += () => { CycleBackground(); MoveBackground(); };
        }
        //获取视觉元素
        protected override void GetVisualElements()
        {
            Background = Document.rootVisualElement.Q<VisualElement>("Background");
        }
        #endregion

        #region 其他方法
        //循环背景图
        public void CycleBackground()
        {
            //获取协程
            Coroutine coroutine = StartCoroutine(CycleBackgroundCoroutine());
            //存储协程，循环背景图仅仅会存在一份，所以直接以方法名为Key
            CoroutineDic["CycleBackground"] = coroutine;
        }
        //移动背景图
        public void MoveBackground()
        {
            //获取协程
            Coroutine coroutine = StartCoroutine(MoveBackgroundCoroutine());
            //存储
            CoroutineDic["MoveBackground"] = coroutine;
        }
        //切换背景图
        public void SwitchBackground(Scene location)
        {
            //获取协程
            Coroutine coroutine = StartCoroutine(SwitchBackgroundCoroutine(location));
            //存储协程，同理仅存储一份
            CoroutineDic["SwitchBackground"] = coroutine;
        }
        #endregion

        #region 协程方法
        //背景图循环方法本体
        private IEnumerator CycleBackgroundCoroutine()
        {
            //首先获取可用地点
            List<Scene> locations = GameScene.LocationDb.Datas.ToList();

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
                    Scene location = locations[Random.Range(0, locations.Count)];

                    //然后切换背景图
                    SwitchBackground(location);
                    //开启新一轮背景图移动
                    MoveBackground();

                    //更新持续时间
                    Duration = 60f;
                }
            }
        }

        //移动背景图
        private IEnumerator MoveBackgroundCoroutine()
        {
            //首先，更改背景图位置偏移为Y轴20%
            Background.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(20, LengthUnit.Percent)));
            //然后等待15s
            yield return new WaitForSeconds(45f);

            //然后，偏移更改为0
            Background.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(0)));
            //再等待15s
            yield return new WaitForSeconds(45f);

            //然后，偏移更改为-20%
            Background.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(-20, LengthUnit.Percent)));
            //再等待15s
            yield return new WaitForSeconds(45f);

            //最后，偏移更改回0%
            Background.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(0, LengthUnit.Percent)));
            //不等待，协程到此结束
        }

        //切换背景图本体
        private IEnumerator SwitchBackgroundCoroutine(Scene location)
        {
            //更改背景图不透明度为0
            Background.style.opacity = 0;
            //协程等待1s(动画用时)
            yield return new WaitForSeconds(1.0f);

            //更换背景图
            Background.style.backgroundImage = new StyleBackground(location.Background);
            //更新主面板
            GameUI.MainTitleInterface.LocationLabel.text = location.Name;
            //协程等待到当前帧结束
            yield return new WaitForEndOfFrame();

            //修改不透明度为1
            Background.style.opacity = 1;
            //协程等待1s(动画用时)
            yield return new WaitForSeconds(1.0f);
        }
        #endregion
    }
}
