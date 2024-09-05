using UnityEngine;

namespace THLL.BaseSystem
{
    /// <summary>
    /// 基于GameBehaviour的单例模式
    /// 不包含加载时不删除功能
    /// </summary>
    public class Singleton<T> : GameBehaviour where T : Singleton<T>
    {
        #region 数据
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
        #endregion

        #region 周期函数
        //Awake
        protected override void Awake()
        {
            //父类Awake方法
            base.Awake();

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

            //把自己添加到静态存储中
            TouhouLeisurelyLife.Managers.Add(this);

            //将自身设置为不启用
            enabled = false;
        }
        //Start
        protected virtual void Start()
        {
            //初始化
            Init();
        }
        #endregion

        #region 其他函数
        //初始化
        protected virtual void Init()
        {

        }
        #endregion
    }
}
