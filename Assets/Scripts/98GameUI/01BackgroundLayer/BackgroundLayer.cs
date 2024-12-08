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
        //主界面背景
        public VisualElement MainTitleBackground { get; private set; }
        //场景背景
        public VisualElement SceneBackground { get; private set; }


        //背景图相关
        //当前背景所剩时间        
        public float Duration { get; private set; } = -1f;
        #endregion

        #region 初始化与相关方法
        //初始化
        protected override void Start()
        {
            //父类Start方法
            base.Start();

            //将协程启用添加到资源加载完成事件中并手动启用一次移动操作
            GameAssetsManager.Instance.OnAllResourcesLoaded += () => CycleMainTitleBackground();
        }
        //获取视觉元素
        protected override void GetVisualElements()
        {
            MainTitleBackground = Document.rootVisualElement.Q<VisualElement>("MainTitleBackground");
            SceneBackground = Document.rootVisualElement.Q<VisualElement>("SceneBackground");
        }
        #endregion

        #region 主界面背景图相关方法
        //循环主界面背景图
        public void CycleMainTitleBackground()
        {
            //场景背景图隐藏，主界面背景图显示，伴随不透明度更改
            SceneBackground.style.display = DisplayStyle.None;
            SceneBackground.style.opacity = 0;
            MainTitleBackground.style.display = DisplayStyle.Flex;
            MainTitleBackground.style.opacity = 1;
            //开始背景图循环协程
            StartCoroutine(CycleMainTitleBackgroundCoroutine());
            //开始背景图移动协程
            StartCoroutine(MoveMainTitleBackgroundCoroutine());
        }
        //停止循环背景图
        public void StopCycleMainTitleBackground()
        {
            //场景背景图显示，主界面背景图隐藏，伴随不透明度更改
            SceneBackground.style.display = DisplayStyle.Flex;
            SceneBackground.style.opacity = 1;
            MainTitleBackground.style.display = DisplayStyle.None;
            MainTitleBackground.style.opacity = 0;
            //停止协程
            StopCoroutine(CycleMainTitleBackgroundCoroutine());
            //停止移动协程
            StopCoroutine(MoveMainTitleBackgroundCoroutine());
            //复原背景偏移
            MainTitleBackground.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(0)));
        }
        //切换主界面背景图
        private void SwitchMainTitleBackground(Scene location)
        {
            //开始背景图切换协程
            StartCoroutine(SwitchMainTitleBackgroundCoroutine(location));
        }
        //切换主界面背景图协程
        private IEnumerator SwitchMainTitleBackgroundCoroutine(Scene scene)
        {
            //更改背景图不透明度为0
            MainTitleBackground.style.opacity = 0;
            //协程等待1s(动画用时)
            yield return new WaitForSeconds(1.0f);

            //更换背景图
            MainTitleBackground.style.backgroundImage = new StyleBackground(scene.BackgroundsDict.Values.ToList()[Random.Range(0, scene.BackgroundsDict.Count)]);
            //更新主面板
            GameUI.MainTitleInterface.LocationLabel.text = scene.Name;
            //协程等待到当前帧结束
            yield return new WaitForEndOfFrame();

            //修改不透明度为1
            MainTitleBackground.style.opacity = 1;
            //协程等待1s(动画用时)
            yield return new WaitForSeconds(1.0f);
        }
        //主界面背景图循环方法本体
        private IEnumerator CycleMainTitleBackgroundCoroutine()
        {
            //首先获取可用地点，此处为主界面循环，仅循环根场景
            List<Scene> locations = GameScene.SceneDB.RootScenesStorage.Values.ToList();

            //不设置终止条件，除非协程被强制终止，否则一直循环
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
                    //若小于0，更换背景图
                    //首先获取目标地点
                    Scene location = locations[Random.Range(0, locations.Count)];

                    //然后切换背景图
                    SwitchMainTitleBackground(location);

                    //更新持续时间
                    Duration = 180f;
                }
            }
        }
        //移动主界面背景图协程
        private IEnumerator MoveMainTitleBackgroundCoroutine()
        {
            //不设置终止条件，除非协程被强制终止，否则一直循环
            while (true)
            {
                //首先，更改背景图位置偏移为Y轴20%
                MainTitleBackground.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(20, LengthUnit.Percent)));
                //然后等待45s
                yield return new WaitForSeconds(45f);

                //然后，偏移更改为0
                MainTitleBackground.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(0)));
                //再等待45s
                yield return new WaitForSeconds(45f);

                //然后，偏移更改为-20%
                MainTitleBackground.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(-20, LengthUnit.Percent)));
                //再等待45s
                yield return new WaitForSeconds(45f);

                //最后，偏移更改回0%
                MainTitleBackground.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(0, LengthUnit.Percent)));
                //再等待45s
                yield return new WaitForSeconds(45f);
            }
        }
        #endregion

        #region 场景背景图相关方法
        //切换场景背景图
        public void SwitchSceneBackground(Scene scene)
        {
            //开始背景图切换协程
            StartCoroutine(SwitchSceneBackgroundCoroutine(scene));
        }
        //切换场景背景图协程
        private IEnumerator SwitchSceneBackgroundCoroutine(Scene scene)
        {
            //更改背景图不透明度为0
            SceneBackground.style.opacity = 0;
            //协程等待1s(动画用时)
            yield return new WaitForSeconds(1.0f);

            //更换背景图
            SceneBackground.style.backgroundImage = new StyleBackground(scene.BackgroundsDict.Values.ToList()[Random.Range(0, scene.BackgroundsDict.Count)]);
            //协程等待到当前帧结束
            yield return new WaitForEndOfFrame();

            //修改不透明度为1
            SceneBackground.style.opacity = 1;
            //协程等待1s(动画用时)
            yield return new WaitForSeconds(1.0f);
        }
        #endregion
    }
}
