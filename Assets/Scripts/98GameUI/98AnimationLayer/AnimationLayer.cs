using System;
using System.Collections;
using THLL.BaseSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.UISystem
{
    public class AnimationLayer : BaseGameInterface
    {
        #region 自身数据
        //UI数据
        //遮罩
        public VisualElement Cover { get; private set; }
        //加载画面
        public VisualElement LoadingScreen { get; private set; }
        //少女祈祷中
        public Label MaidenPrayingLabel { get; private set; }

        //其他数据
        //动画方向，True为向右，False为向左
        public bool AnimationDirection { get; private set; } = true;
        #endregion

        #region 初始化与相关方法
        //初始化
        protected override void Start()
        {
            //父类Start方法
            base.Start();

            //将隐藏加载界面添加到资源加载事件中
            GameAssetsManager.Instance.OnAllResourcesLoaded += HideLoadingScreen;
        }
        //获取视觉元素
        protected override void GetVisualElements()
        {
            Cover = Document.rootVisualElement.Q<VisualElement>("Cover");
            LoadingScreen = Document.rootVisualElement.Q<VisualElement>("LoadingScreen");
            MaidenPrayingLabel = Document.rootVisualElement.Q<Label>("MaidenPrayingLabel");
        }
        #endregion

        #region 其他方法
        //播放一次遮盖动画
        public void CoverOnce(Action method)
        {
            //获取协程
            Coroutine coroutine = StartCoroutine(CoverOnceCoroutine(method, GameUI.DefaultUIAnimationDuration));
            //存储协程
            CoroutineDic["CoverOnce" + method.GetHashCode()] = coroutine;
        }
        //显示加载界面
        public void ShowLoadingScreen()
        {
            //首先将加载界面不透明度设为1
            LoadingScreen.style.opacity = 1;
            //然后开始祈祷
            Coroutine coroutine = StartCoroutine(MaidenPrayCoroutine(GameUI.DefaultUIAnimationDuration));
            //并存储协程，考虑到祈祷动画唯一，不设置额外Key
            CoroutineDic["MaidenPray"] = coroutine;
        }
        //隐藏加载界面
        public void HideLoadingScreen()
        {
            //结束少女祈祷协程
            StopCoroutine(CoroutineDic["MaidenPray"]);
            //将标签不透明度设为1
            MaidenPrayingLabel.style.opacity = 1;
            //遮盖一次，并将加载界面隐藏
            CoverOnce(() => LoadingScreen.style.opacity = 0);
        }
        #endregion

        #region 协程方法
        //播放一次遮盖动画
        private IEnumerator CoverOnceCoroutine(Action method, float animationDuration)
        {
            //为动画设置间隔时间
            GameUI.SetVisualElementAllTransitionAnimationDuration(Cover, animationDuration);

            //然后开始判断方向
            if (AnimationDirection)
            {
                //若向右，则更改Right为0
                Cover.style.right = new StyleLength(new Length(0, LengthUnit.Percent));
            }
            else
            {
                //反之则修改Left
                Cover.style.left = new StyleLength(new Length(0, LengthUnit.Percent));
            }
            //间隔0.5s
            yield return new WaitForSeconds(animationDuration);

            //执行方法
            method();
            //间隔至当前帧结束
            yield return new WaitForEndOfFrame();

            //再次判断方向
            if (AnimationDirection)
            {
                //若向右，则更改Left为100并反向
                Cover.style.left = new StyleLength(new Length(100, LengthUnit.Percent));
                AnimationDirection = !AnimationDirection;
            }
            else
            {
                //反之则修改Right
                Cover.style.right = new StyleLength(new Length(100, LengthUnit.Percent));
                AnimationDirection = !AnimationDirection;
            }
            //间隔0.5s
            yield return new WaitForSeconds(animationDuration);
        }
        //少女祈祷中
        private IEnumerator MaidenPrayCoroutine(float animationDuration)
        {
            //为动画设置间隔时间
            GameUI.SetVisualElementAllTransitionAnimationDuration(MaidenPrayingLabel, animationDuration);

            //没什么事的话就一直运行吧
            while (true)
            {
                //隐藏一下
                MaidenPrayingLabel.style.opacity = 0;
                //等0.3s
                yield return new WaitForSeconds(animationDuration);

                //显示一下
                MaidenPrayingLabel.style.opacity = 1;
                //等0.3s
                yield return new WaitForSeconds(animationDuration);
            }
        }
        #endregion
    }
}
