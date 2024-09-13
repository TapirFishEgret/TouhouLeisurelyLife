using System.Collections.Generic;

namespace THLL.BaseSystem
{
    //委托，用于表示过滤函数
    public delegate IEnumerable<T> FilterDelegate<T>(IEnumerable<T> items, string filterValue);
}
