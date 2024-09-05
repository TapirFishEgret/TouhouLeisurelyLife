using System.Collections;
using System.Collections.Generic;
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
        public static GamePlaySettings GamePlaySettingsInterface { get; set; }
        //游戏补丁设定界面
        public static GamePatchesSettings GamePatchesSettingsInterface { get; set; }

        //游玩相关界面
        //游玩界面
        public static Play PlayInterface { get; set; }

        //辅助界面
        //背景图层
        public static BackgroundLayer BackgroundLayer { get; set; }
        //动画图层
        public static AnimationLayer AnimationLayer { get; set; }

        //界面显示状态存储，主要用于返回功能
        public static List<BaseGameUI> ShowedInterfaces { get; } = new();
        #endregion

        #region 静态方法
        //显示界面
        public static void ShowInterface(BaseGameUI @interface, bool needsAnimation = true)
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
        private static void ShowInterface(BaseGameUI @interface)
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
        //渐变式显示文本
        public static void GradientDisplayText(BaseGameUI @interface, Label conatiner, string text, float animationDuration)
        {
            //TODO:暂时这么解决吧，停止该界面其他协程
            @interface.StopAllCoroutines();
            //让传入的界面运行协程
            @interface.StartCoroutine(GradientDisplayText(conatiner, text, animationDuration));
        }
        //渐变式显示文本本体
        private static IEnumerator GradientDisplayText(Label container, string text, float animationDuration)
        {
            //检测文本
            if (container.text == text)
            {
                //若文本内容未发生实际更改，则直接返回
                yield break;
            }

            //首先，隐藏当前Label，更改不透明度，这货是个0-1的浮点数值
            container.style.opacity = 0f;
            //等待
            yield return new WaitForSeconds(animationDuration);

            //然后更改值
            container.text = text;
            //等待到当前帧结束
            yield return new WaitForEndOfFrame();

            //然后显示
            container.style.opacity = 1f;
            //等待
            yield return new WaitForSeconds(animationDuration);
        }
        //逐字式显示文本
        public static void ProgressiveDisplayText(BaseGameUI @interface, Label container, string text, float animationDuration)
        {
            //TODO:暂时这么解决吧，停止该界面其他协程
            @interface.StopAllCoroutines();
            //让界面运行协程
            @interface.StartCoroutine(ProgressiveDisplayText(container, text, animationDuration));
        }
        //逐字式显示文本本体
        private static IEnumerator ProgressiveDisplayText(Label container, string text, float animationDuration)
        {
            //检测文本
            if (container.text == text)
            {
                //若文本内容未发生实际更改，则直接返回
                yield break;
            }

            //计算字体显示间隔
            float fontDisplayInterval = animationDuration / text.Length;

            //清空当前文本
            container.text = string.Empty;

            //逐字显示
            foreach (char letter in text)
            {
                //添加文本
                container.text += letter;
                //等待指定时间
                yield return new WaitForSeconds(fontDisplayInterval);
            }
        }
        #endregion
    }
}
