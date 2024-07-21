using UnityEngine;

namespace THLL.BaseSystem
{
    /// <summary>
    /// 基于GameBehaviour的单例模式
    /// 不包含加载时不删除功能
    /// </summary>
    public class Singleton<T> : GameBehaviour where T : Singleton<T>
    {
        //单例模式
        protected static T instance;
        public static T Instance
        {
            get
            {
                //判断是否存在
                if (instance == null)
                {
                    //若没有，尝试查找现有的实例
                    instance = FindFirstObjectByType<T>();
                    //再次判断是否存在
                    if (instance == null)
                    {
                        //若仍不存在，生成包含该脚本的游戏物体
                        GameObject go = new("Singleton");
                        instance = go.AddComponent<T>();
                    }
                }
                //返回
                return instance;
            }
        }

        //周期函数
        protected virtual void Awake()
        {
            //单例模式的设置
            if (instance == null)
            {
                //若为空，则指定当前物体为单例
                instance = this as T;
            }
            else if (instance != this)
            {
                //若单例不为当前物体，销毁
                Destroy(gameObject);
            }
        }
    }
}
