using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.BaseSystem
{
    public static class GameHistory
    {
        #region 数据
        //循环缓冲区，用于存储历史记录消息
        public static CircularBuffer<Label> Logs { get; } = new(10000);
        #endregion

        #region 记录方法
        //记录信息
        public static void Log(string message, Color color)
        {
            //创建一条Label，并设置其信息与颜色
            Label label = new()
            {
                text = message,
                style =
                {
                    color = color
                },
            };
            //添加到循环缓冲区中
            Logs.Add(label);
            //顺便让Debug也显示一下
            if (color == Color.red)
            {
                Debug.LogError(message);
            }
            else if (color == Color.yellow)
            {
                Debug.LogWarning(message);
            }
            else
            {
                Debug.Log(message);
            }
        }
        //记录一条普通信息
        public static void LogNormal(string message)
        {
            Log(message, Color.white);
        }
        //记录一条警告信息
        public static void LogWarning(string message)
        {
            Log(message, Color.yellow);
        }
        //记录一条错误信息
        public static void LogError(string message)
        {
            Log(message, Color.red);
        }
        #endregion

        #region 其他方法
        //重设历史记录大小
        public static void ResizeCapacity(int capacity)
        {
            if (capacity > 1)
            {
                Logs.Resize(capacity);
            }
        }
        #endregion
    }
}
