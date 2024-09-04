using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.UISystem
{
    public class AnimationLayer : BaseGameUI
    {
        #region 自身数据
        //UI数据
        //遮罩
        public VisualElement Cover { get; private set; }

        //其他数据
        //动画方向，True为向右，False为向左
        public bool AnimationDirection { get; private set; } = true;
        #endregion

        #region 初始化与相关方法
        //获取视觉元素
        protected override void GetVisualElements()
        {
            Cover = Document.rootVisualElement.Q<VisualElement>("Cover");
        }
        #endregion

        #region 其他方法
        //播放一次动画
        public void PlayOnce(Action method)
        {
            StartCoroutine(PlayOnceCoroutine(method));
        }
        //播放一次动画
        private IEnumerator PlayOnceCoroutine(Action method)
        {
            //首先判断方向
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
            yield return new WaitForSeconds(0.5f);

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
            yield return new WaitForSeconds(0.5f);
        }
        #endregion
    }
}
