using System;
using System.Diagnostics;
using UnityEngine;

namespace THLL
{
    //操作计时器，使用using块来实现计时操作
    public class ExecutionTimer : IDisposable
    {
        //计时器
        private readonly Stopwatch _stopwatch;
        //操作名称
        private readonly string _operationName;

        //接口实现
        public void Dispose()
        {
            if (_stopwatch != null)
            {
                //计时颜色
                string hexColor = ColorUtility.ToHtmlStringRGBA(ChineseColor.Blue_瀑布蓝);
                _stopwatch.Stop();
                UnityEngine.Debug.Log($"{_operationName}完成，耗时<color=#{hexColor}>{_stopwatch.ElapsedMilliseconds}</color>ms");
            }
        }

        //构建函数
        public ExecutionTimer(string operationName)
        {
            _operationName = operationName;
            _stopwatch = Stopwatch.StartNew();
        }
    }
}
