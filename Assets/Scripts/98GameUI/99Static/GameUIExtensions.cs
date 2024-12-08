using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.UISystem
{
    public static class GameUIExtensions
    {
        #region 视觉元素渐变切换相关
        //渐变式切换元素
        public static void GradientSwitchElement(this VisualElement element, VisualElement otherElement, float animationDuration)
        {
            //检测原元素与目标元素是否相同或不为同一个父级或目标元素为空
            if (element == otherElement || (!element.parent.Children().Contains(otherElement)) || otherElement == null)
            {
                //若是，返回
                return;
            }

            //检测是否有协程在运行且目标元素相同
            if (GameUIManager.Instance.IsRunningCoroutine(element) && element.userData == otherElement)
            {
                //若是，返回
                return;
            }

            //若均不是，直接切换元素
            element.DirectSwitchElement(otherElement);
            //添加协程
            GameUIManager.Instance.AddCoroutine(element, GradientSwitchElementCoroutine(element, otherElement, animationDuration));
        }
        //渐变式切换元素协程
        private static IEnumerator GradientSwitchElementCoroutine(VisualElement element, VisualElement otherElement, float animationDuration)
        {
            //将目标元素移动至userData暂存
            element.userData = otherElement;

            //确定执行协程后，更改容器动画时长
            GameUI.SetVisualElementAllTransitionAnimationDuration(element, animationDuration);
            GameUI.SetVisualElementAllTransitionAnimationDuration(otherElement, animationDuration);

            //首先，隐藏当前元素，更改不透明度
            element.style.opacity = 0f;
            //等待
            yield return new WaitForSeconds(animationDuration);

            //然后更改值
            element.style.display = DisplayStyle.None;
            otherElement.style.display = DisplayStyle.Flex;
            //等待到当前帧结束
            yield return new WaitForEndOfFrame();

            //然后显示
            otherElement.style.opacity = 1f;
            //等待
            yield return new WaitForSeconds(animationDuration);

            //协程完成后移除自身
            GameUIManager.Instance.RemoveCoroutine(element);
        }
        //直接切换元素
        public static void DirectSwitchElement(this VisualElement element, VisualElement otherElement)
        {
            //检测并停止，移除协程
            if (GameUIManager.Instance.IsRunningCoroutine(element))
            {
                GameUIManager.Instance.RemoveCoroutine(element);
            }

            //更改元素动画时长为0
            GameUI.SetVisualElementAllTransitionAnimationDuration(element, 0f);
            GameUI.SetVisualElementAllTransitionAnimationDuration(otherElement, 0f);
            //更改元素不透明度
            element.style.opacity = 0f;
            otherElement.style.opacity = 1f;
            //更改元素显示
            element.style.display = DisplayStyle.None;
            otherElement.style.display = DisplayStyle.Flex;
        }
        #endregion

        #region Label文本显示相关
        //渐变式显示文本
        public static void GradientDisplayText(this Label label, string text, float animationDuration)
        {
            //检测是否有相同协程正在运行及传入文本与目标文本是否相同
            if (GameUIManager.Instance.IsRunningCoroutine(label) && label.userData as string == text)
            {
                //若是，返回
                return;
            }

            //若不是，则停止显示当前文本(表现为直接全部显示)
            label.DirectlyDisplayText();
            //添加协程
            GameUIManager.Instance.AddCoroutine(label, GradientDisplayTextCoroutine(label, text, animationDuration));
        }
        //渐变式显示文本协程
        private static IEnumerator GradientDisplayTextCoroutine(Label label, string text, float animationDuration)
        {
            //将文本移动至userData暂存
            label.userData = text;

            //确定执行协程后，更改容器动画时长
            GameUI.SetVisualElementAllTransitionAnimationDuration(label, animationDuration);

            //首先，隐藏当前Label，更改不透明度，这货是个0-1的浮点数值
            label.style.opacity = 0f;
            //等待
            yield return new WaitForSeconds(animationDuration);

            //然后更改值
            label.text = label.userData as string;
            //等待到当前帧结束
            yield return new WaitForEndOfFrame();

            //然后显示
            label.style.opacity = 1f;
            //等待
            yield return new WaitForSeconds(animationDuration);

            //协程完成后移除自身
            GameUIManager.Instance.RemoveCoroutine(label);
        }

        //逐字式显示文本
        public static void ProgressiveDisplayText(this Label label, string text, float animationDuration)
        {
            //检测是否有相同协程正在运行及传入文本与目标文本是否相同
            if (GameUIManager.Instance.IsRunningCoroutine(label) && label.userData as string == text)
            {
                //若是，返回
                return;
            }

            //若不是，则停止显示当前文本(表现为直接全部显示)
            label.DirectlyDisplayText();
            //添加协程
            GameUIManager.Instance.AddCoroutine(label, ProgressiveDisplayTextCoroutine(label, text, animationDuration));
        }
        //逐字式显示文本协程
        private static IEnumerator ProgressiveDisplayTextCoroutine(Label label, string text, float animationDuration)
        {
            //将文本移入userData暂存
            label.userData = text;

            //计算字体显示时间间隔
            float fontDisplayInterval = animationDuration / text.Length;

            //清空当前文本
            label.text = string.Empty;

            //逐字显示
            foreach (char letter in label.userData as string)
            {
                //添加文本
                label.text += letter;
                //等待指定时间
                yield return new WaitForSeconds(fontDisplayInterval);
            }

            //协程完成后移除自身
            GameUIManager.Instance.RemoveCoroutine(label);
        }

        //直接显示所有文本
        public static void DirectlyDisplayText(this Label label)
        {
            //检测并停止，移除协程
            if (GameUIManager.Instance.IsRunningCoroutine(label))
            {
                GameUIManager.Instance.RemoveCoroutine(label);
            }

            //更改元素动画时长为0
            GameUI.SetVisualElementAllTransitionAnimationDuration(label, 0f);
            //更改元素不透明度
            label.style.opacity = 1f;

            //显示所有文本
            label.text = label.userData as string;
        }
        #endregion
    }
}
