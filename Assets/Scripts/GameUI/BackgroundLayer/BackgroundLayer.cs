using System.Collections;
using System.Collections.Generic;
using System.Linq;
using THLL.BaseSystem;
using THLL.GeographySystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.UISystem
{
    public class BackgroundLayer : BaseGameUI
    {
        #region 自身数据
        //UI相关
        //背景
        public VisualElement Background { get; private set; }
        //遮罩
        public VisualElement Cover { get; private set; }

        //背景图相关
        //当前背景所剩时间
        public float Duration { get; private set; } = 0;
        //动画方向，True为向下，False为向上
        public bool AnimationDirection { get; private set; } = true;
        #endregion

        #region 初始化与相关方法
        //初始化
        protected override void Init()
        {
            //父类初始化函数
            base.Init();

            //将协程启用添加到资源加载完成事件中
            GameAssetsManager.Instance.OnAllResourcesLoaded += () => StartCoroutine(CycleBackground());
        }
        //获取视觉元素
        protected override void GetVisualElements()
        {
            Background = Document.rootVisualElement.Q<VisualElement>("Background");
            Cover = Document.rootVisualElement.Q<VisualElement>("Cover");
        }
        #endregion

        #region 其他方法
        //背景图循环方法
        public IEnumerator CycleBackground()
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

                    //然后遮盖背景，判断动画方向
                    if (AnimationDirection)
                    {
                        //若为向下，则设置遮盖的Bottom数值
                        Cover.style.bottom = new StyleLength(new Length(0, LengthUnit.Percent));
                    }
                    else
                    {
                        //反之设置Top
                        Cover.style.top = new StyleLength(new Length(0, LengthUnit.Percent));
                    }
                    //协程等待0.5s(动画用时)
                    yield return new WaitForSeconds(0.5f);

                    //更换背景图
                    Background.style.backgroundImage = new StyleBackground(location.Background);
                    //更新主面板
                    GameUI.MainTitleInterface.LocationLabel.text = location.Name;
                    //协程等待到当前帧结束
                    yield return new WaitForEndOfFrame();

                    //取消遮盖背景，同样判断动画方向
                    if (AnimationDirection)
                    {
                        //若为向下，则设置遮盖的Top数值
                        Cover.style.top = new StyleLength(new Length(100, LengthUnit.Percent));
                        //并反向
                        AnimationDirection = !AnimationDirection;
                    }
                    else
                    {
                        //反之设置Bottom
                        Cover.style.bottom = new StyleLength(new Length(100, LengthUnit.Percent));
                        //并反向
                        AnimationDirection = !AnimationDirection;
                    }
                    //协程等待0.5s(动画用时)
                    yield return new WaitForSeconds(0.5f);

                    //更新持续时间，测试期间固定5s
                    Duration = 5f;
                }
            }
        }
        #endregion
    }
}
