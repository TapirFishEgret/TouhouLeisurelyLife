using System.Collections.Generic;
using THLL.BaseSystem;
using THLL.LocationSystem.Tags;
using UnityEngine;

namespace THLL.LocationSystem
{
    public class GameLocationMgr : GameBehaviour
    {
        //测试对象
        public LocUnitData testTarget;

        protected override void OnMinuteChanged(int currentMinute)
        {
            base.OnMinuteChanged(currentMinute);
            //每分钟测试一次
            TestMethod();
        }

        //测试方法
        public void TestMethod()
        {
            if (testTarget != null)
            {
                //获取真正的测试对象
                LocUnit realTestTarget = GameLocation.LocUnitDb[testTarget];
                //输出对象ID
                Debug.Log(realTestTarget.ID);
                //输出对象包含的标签的名称
                foreach (LocUnitTag locUnitTag in realTestTarget.Tags)
                {
                    Debug.Log(locUnitTag.Name);
                }
                //输出对象所有链接数据
                foreach (KeyValuePair<LocUnit, int> keyValuePair in realTestTarget.Connections)
                {
                    Debug.Log($"从水下第一个生命的{realTestTarget.ID}开始，到{keyValuePair.Key.ID}时代，一共花费了{keyValuePair.Value}秒钟");
                }
            }
        }
    }
}