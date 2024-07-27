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
        L_IsGateway,
    }

    //委托，用于表示过滤函数，可传入多种类型
    public delegate IEnumerable<TValue> FilterDelegate<TValue, TQueryValue>(IEnumerable<TValue> datas, TQueryValue queryValue);
}
