using System.Collections.Generic;

namespace THLL.BaseSystem
{
    //查询关键字枚举
    public enum QueryKeywordEnum
    {
        B_Package,
        B_Category,
        B_Author,
        B_Name,
        L_ParentName,
        LT_IsInherited
    }

    //委托，用于表示过滤函数
    public delegate IEnumerable<T> FilterDelegate<T>(IEnumerable<T> datas, string queryValue);
}
