using System;

namespace THLL.BaseSystem
{
    /// <summary>
    /// 简单的标记属性，用于字段上添加一个字符串标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class EasyMarkAttribute : Attribute
    {
        //Mark
        public string Mark { get; private set; }

        //构造函数
        public EasyMarkAttribute(string mark)
        {
            Mark = mark;
        }
    }
}
