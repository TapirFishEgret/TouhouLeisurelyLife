using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.UISystem
{
    public static class GameUIExtensions
    {
        #region 渐变切换图片内容显示
        //渐变式切换图片
        public static void GradientSwitchImage(this Image image, Sprite sprite, float animationDuration, Action onSwitchHappened = null)
        {
            //检测传入图片与当前图片是否相同
            if (image.sprite == sprite)
            {
                //若是，返回
                return;
            }
            else
            {
                //若不同，检测是否有协程在运行
                if (GameUIManager.Instance.IsRunningCoroutine(image))
                {
                    //若是，直接显示当前图片
                    image.DirectlySwitchImage(onSwitchHappened);
                }
                //添加协程
                GameUIManager.Instance.AddCoroutine(image, GradientSwitchImageCoroutine(image, sprite, animationDuration, onSwitchHappened));
            }
        }
        //渐变式切换图片协程
        private static IEnumerator GradientSwitchImageCoroutine(Image image, Sprite sprite, float animationDuration, Action onSwitchHappened = null)
        {
            //将图片移动至userData暂存
            image.userData = sprite;

            //确定执行协程后，更改容器动画时长
            GameUI.SetVisualElementAllTransitionAnimationDuration(image, animationDuration);

            //首先，隐藏当前Label，更改不透明度，这货是个0-1的浮点数值
            image.style.opacity = 0f;
            //等待
            yield return new WaitForSeconds(animationDuration);

            //然后更改值
            image.sprite = image.userData as Sprite;
            //触发回调
            onSwitchHappened?.Invoke();
            //等待到当前帧结束
            yield return new WaitForEndOfFrame();

            //然后显示
            image.style.opacity = 1f;
            //等待
            yield return new WaitForSeconds(animationDuration);

            //协程完成后移除自身
            GameUIManager.Instance.RemoveCoroutine(image);
        }
        //直接切换图片
        public static void DirectlySwitchImage(this Image image, Action onSwitchHappened = null)
        {
            //检测并停止，移除协程
            if (GameUIManager.Instance.IsRunningCoroutine(image))
            {
                GameUIManager.Instance.RemoveCoroutine(image);
            }
            //检测暂存元素是否为空
            if (image.userData == null)
            {
                //若是，直接返回
                return;
            }

            //更改元素动画时长为0
            GameUI.SetVisualElementAllTransitionAnimationDuration(image, 0f);
            //更改元素不透明度
            image.style.opacity = 1f;

            //显示所有图片
            image.sprite = image.userData as Sprite;
            //触发回调
            onSwitchHappened?.Invoke();
        }
        #endregion

        #region Label文本显示相关
        //渐变式显示文本
        public static void GradientDisplayText(this Label label, string text, float animationDuration, Action onSwitchHappened = null)
        {
            //检测传入文本与当前文本是否相同
            if (label.userData as string == text)
            {
                //若是，返回
                return;
            }
            else
            {
                //若不同，检测是否有协程在运行
                if (GameUIManager.Instance.IsRunningCoroutine(label))
                {
                    //若是，直接显示当前所有文本
                    label.DirectlyDisplayText(onSwitchHappened);
                }
                //添加协程
                GameUIManager.Instance.AddCoroutine(label, GradientDisplayTextCoroutine(label, text, animationDuration, onSwitchHappened));
            }
        }
        //渐变式显示文本协程
        private static IEnumerator GradientDisplayTextCoroutine(Label label, string text, float animationDuration, Action onSwitchHappened = null)
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
            //触发回调
            onSwitchHappened?.Invoke();
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
        public static void ProgressiveDisplayText(this Label label, string text, float animationDuration, Action onSwitchHappened = null)
        {
            //检测传入文本与当前文本是否相同
            if (label.userData as string == text)
            {
                //若是，返回
                return;
            }
            else
            {
                //若不同，检测是否有协程在运行
                if (GameUIManager.Instance.IsRunningCoroutine(label))
                {
                    //若是，直接显示当前所有文本
                    label.DirectlyDisplayText(onSwitchHappened);
                }
                //添加协程
                GameUIManager.Instance.AddCoroutine(label, ProgressiveDisplayTextCoroutine(label, text, animationDuration, onSwitchHappened));
            }
        }
        //逐字式显示文本协程
        private static IEnumerator ProgressiveDisplayTextCoroutine(Label label, string text, float animationDuration, Action onSwitchHappened = null)
        {
            //将文本移入userData暂存
            label.userData = text;

            //计算字体显示时间间隔
            float fontDisplayInterval = animationDuration / text.Length;

            //清空当前文本
            label.text = string.Empty;
            //触发回调
            onSwitchHappened?.Invoke();

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
        public static void DirectlyDisplayText(this Label label, Action onSwitchHappened = null)
        {
            //检测并停止，移除协程
            if (GameUIManager.Instance.IsRunningCoroutine(label))
            {
                GameUIManager.Instance.RemoveCoroutine(label);
            }
            //检测暂存元素是否为空
            if (label.userData == null)
            {
                //若是，直接返回
                return;
            }

            //更改元素动画时长为0
            GameUI.SetVisualElementAllTransitionAnimationDuration(label, 0f);
            //更改元素不透明度
            label.style.opacity = 1f;

            //显示所有文本
            label.text = label.userData as string;
            //触发回调
            onSwitchHappened?.Invoke();
        }
        #endregion
    }
}
