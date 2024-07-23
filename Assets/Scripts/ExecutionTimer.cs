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
        //是否输出控制台信息
        private readonly bool _debugLogEnabled;
        //时间信息十六进制颜色
        private string _timeHexColor;

        //接口实现
        public void Dispose()
        {
            if (_stopwatch != null)
            {
                //停止计时
                _stopwatch.Stop();
                //决定是否输出
                if (_debugLogEnabled)
                {
                    //计时颜色
                    if (string.IsNullOrEmpty(_timeHexColor))
                    {
                        //若计时器颜色为空，默认为瀑布蓝
                        _timeHexColor = ColorUtility.ToHtmlStringRGBA(ChineseColor.Blue_瀑布蓝);
                    }
                    UnityEngine.Debug.Log($"{_operationName}完成，耗时<color=#{_timeHexColor}>{_stopwatch.ElapsedMilliseconds}</color>ms");
                }
            }
        }

        //构建函数
        public ExecutionTimer(string operationName, bool debugLog = true, string timeHexColor = null)
        {
            _operationName = operationName;
            _stopwatch = Stopwatch.StartNew();
            _debugLogEnabled = debugLog;
            _timeHexColor = timeHexColor;
        }
    }
}
