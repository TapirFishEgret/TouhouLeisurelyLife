using System.Collections.Generic;
using System.Linq;
using THLL.BaseSystem;

namespace THLL.CharacterSystem
{
    public class CharacterDatabase : BaseGameEntityDatabase<CharacterData, Character>
    {
        #region 新增索引与存储
        //系列索引
        public Dictionary<string, HashSet<Character>> SeriesIndex { get; private set; } = new();
        //组织索引
        public Dictionary<string, HashSet<Character>> GroupIndex { get; private set; } = new();
        #endregion

        #region 初始化相关
        //筛选器初始化
        protected override void InitFilter()
        {
            //系列筛选器
            FilterStorage.Add(QueryKeyWordEnum.C_Series, (items, filterValue) =>
            {
                //判断传入items个数与总基础存储个数是否相同
                if (items.Count() == Count)
                {
                    //若是，说明尚未开始筛选，直接返回索引
                    return SeriesIndex[filterValue];
                }
                //若不是，返回对items筛选后的列表
                return items.Where(item => item.Series == filterValue);
            });
            //组织筛选器
            FilterStorage.Add(QueryKeyWordEnum.C_Group, (items, filterValue) =>
            {
                //判断传入items个数与总基础存储个数是否相同
                if (items.Count() == Count)
                {
                    //若是，说明尚未开始筛选，直接返回索引
                    return GroupIndex[filterValue];
                }
                //若不是，返回对items筛选后的列表
                return items.Where(item => item.Group == filterValue);
            });
        }
        #endregion
    }
}
