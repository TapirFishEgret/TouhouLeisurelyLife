using System.Collections;
using System.Collections.Generic;
using System.Linq;
using THLL.UISystem.Settings;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.UISystem
{
    public static class GameUI
    {
        #region 数据
        //系统相关界面
        //主标题界面
        public static MainTitle MainTitleInterface { get; set; }
        //新游戏界面
        public static NewGame NewGameInterface { get; set; }
        //读取与加载界面
        public static SaveAndLoadGame SaveAndLoadGameInterface { get; set; }
        //设置界面
        public static GameSettings GameSettingsInterface { get; set; }
        //系统设定界面
        public static GameSystemSettings GameSystemSettingsInterface { get; set; }
        //游戏玩法设定界面
        public static GameplaySettings GameplaySettingsInterface { get; set; }
        //游戏补丁设定界面
        public static GamePatchesSettings GamePatchesSettingsInterface { get; set; }

        //游玩相关界面
        //游玩界面
        public static BasicPlay BasicPlayInterface { get; set; }

        //辅助界面
        //背景图层
        public static BackgroundLayer BackgroundLayer { get; set; }
        //动画图层
        public static AnimationLayer AnimationLayer { get; set; }

        //界面显示状态存储，主要用于返回功能
        public static List<BaseGameInterface> ShowedInterfaces { get; } = new();
        #endregion

        #region 静态方法
        //显示界面
        public static void ShowInterface(BaseGameInterface @interface, bool needsAnimation = true)
        {
            //检测是否需要动画
            if (needsAnimation)
            {
                //若需要，执行动画图层方法
                AnimationLayer.CoverOnce(() => ShowInterface(@interface));
            }
            else
            {
                //若不需要，直接执行方法
                ShowInterface(@interface);
            }
        }
        //显示界面本体
        private static void ShowInterface(BaseGameInterface @interface)
        {
            //正常执行，首先让存储中的界面归于底层
            ShowedInterfaces.ForEach(i => i.Document.sortingOrder = -1);
            //然后让要显示的界面浮于表面
            @interface.Document.sortingOrder = 1;
            //将它加入存储中
            ShowedInterfaces.Add(@interface);
        }

        //返回上一层界面
        public static void ReturnInterface(bool needsAnimation = true)
        {
            //检测是否需要动画
            if (needsAnimation)
            {
                //若需要，借助动画图层执行方法
                AnimationLayer.CoverOnce(ReturnInterface);
            }
            else
            {
                //若不需要，直接执行方法
                ReturnInterface();
            }
        }
        //返回上一层界面本体
        private static void ReturnInterface()
        {
            //检测数量
            if (ShowedInterfaces.Count > 1)
            {
                //大于1时，继续
                //让存储中的界面归于底层
                ShowedInterfaces.ForEach(i => i.Document.sortingOrder = -1);
                //剔除最后一个
                ShowedInterfaces.Remove(ShowedInterfaces[^1]);
                //让接下来的最后一个浮于表面
                ShowedInterfaces[^1].Document.sortingOrder = 1;
            }
        }
        #endregion

        #region 文本显示
        //渐变式显示文本
        public static void GradientDisplayText(BaseGameInterface @interface, Label container, string text, float animationDuration)
        {
            //检测是否有相同协程正在运行及传入文本与目标文本是否相同
            if (@interface.CoroutineDic.ContainsKey("GradientDisplayText" + container.GetHashCode() + text.GetHashCode()) || container.tooltip == text)
            {
                //若是，返回
                return;
            }

            //若不是，则停止显示当前文本(表现为直接全部显示)
            DirectlyDisplayText(@interface, container);
            //让传入的界面运行协程并获取
            Coroutine coroutine = @interface.StartCoroutine(GradientDisplayTextCoroutine(@interface, container, text, animationDuration));
            //存储在界面的协程字典下，Key为协程主名称+容器哈希值
            @interface.CoroutineDic["GradientDisplayText" + container.GetHashCode() + text.GetHashCode()] = coroutine;
        }
        //渐变式显示文本本体
        private static IEnumerator GradientDisplayTextCoroutine(BaseGameInterface @interface, Label container, string text, float animationDuration)
        {
            //将文本移动至tooltip暂存
            container.tooltip = text;

            //确定执行协程后，更改容器动画时长
            SetVisualElementAllTransitionAnimationDuration(container, animationDuration);

            //首先，隐藏当前Label，更改不透明度，这货是个0-1的浮点数值
            container.style.opacity = 0f;
            //等待
            yield return new WaitForSeconds(animationDuration);

            //然后更改值
            container.text = container.tooltip;
            //等待到当前帧结束
            yield return new WaitForEndOfFrame();

            //然后显示
            container.style.opacity = 1f;
            //等待
            yield return new WaitForSeconds(animationDuration);

            //协程完成后移除自身
            @interface.CoroutineDic.Remove("GradientDisplayText" + container.GetHashCode() + text.GetHashCode());
        }

        //逐字式显示文本
        public static void ProgressiveDisplayText(BaseGameInterface @interface, Label container, string text, float animationDuration)
        {
            //检测是否有相同协程正在运行及传入文本与目标文本是否相同
            if (@interface.CoroutineDic.ContainsKey("ProgressiveDisplayText" + container.GetHashCode() + text.GetHashCode()) || container.tooltip == text)
            {
                //若是，返回
                return;
            }

            //若不是，则停止显示当前文本(表现为直接全部显示)
            DirectlyDisplayText(@interface, container);
            //获取协程
            Coroutine coroutine = @interface.StartCoroutine(ProgressiveDisplayTextCoroutine(@interface, container, text, animationDuration));
            //存储在界面的协程字典下，Key为协程主名称+容器哈希值+文本哈希值
            @interface.CoroutineDic["ProgressiveDisplayText" + container.GetHashCode() + text.GetHashCode()] = coroutine;
        }
        //逐字式显示文本本体
        private static IEnumerator ProgressiveDisplayTextCoroutine(BaseGameInterface @interface, Label container, string text, float animationDuration)
        {
            //将文本移入tooltip暂存
            container.tooltip = text;

            //计算字体显示时间间隔
            float fontDisplayInterval = animationDuration / text.Length;

            //清空当前文本
            container.text = string.Empty;

            //逐字显示
            foreach (char letter in container.tooltip)
            {
                //添加文本
                container.text += letter;
                //等待指定时间
                yield return new WaitForSeconds(fontDisplayInterval);
            }

            //协程完成后移除自身
            @interface.CoroutineDic.Remove("ProgressiveDisplayText" + container.GetHashCode() + text.GetHashCode());
        }

        //直接显示所有文本
        public static void DirectlyDisplayText(BaseGameInterface @interface, Label container)
        {
            //检测并停止，移除协程
            //获取需要停止并移除的协程
            List<string> keysToRemoved = @interface.CoroutineDic.Where(
                kv => kv.Key.StartsWith("GradientDisplayText" + container.GetHashCode()) ||
                kv.Key.StartsWith("ProgressiveDisplayText" + container.GetHashCode()))
                .Select(kv => kv.Key)
                .ToList();
            //移除并停止
            foreach (string key in keysToRemoved)
            {
                @interface.CoroutineDic.Remove(key, out Coroutine coroutine);
                @interface.StopCoroutine(coroutine);
            }

            //更改元素动画时长为0
            SetVisualElementAllTransitionAnimationDuration(container, 0f);
            //更改元素不透明度
            container.style.opacity = 1f;

            //显示所有文本
            container.text = container.tooltip;
        }
        #endregion

        #region 辅助方法
        //设定视觉元素动画间隔时间
        public static void SetVisualElementAllTransitionAnimationDuration(VisualElement visualElement, float animationDuration)
        {
            //需要设置的动画的个数
            int count = visualElement.resolvedStyle.transitionDuration.Count();
            //创建新列表
            StyleList<TimeValue> list = Enumerable.Repeat(new TimeValue(animationDuration, TimeUnit.Second), count).ToList();
            //赋值
            visualElement.style.transitionDuration = list;
        }
        #endregion
    }
}
