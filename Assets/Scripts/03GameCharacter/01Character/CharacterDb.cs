using System.Collections.Generic;
using System.Linq;

namespace THLL.CharacterSystem
{
    public class CharacterDb : BaseGameEntityDb<CharacterData, Character>
    {
        #region 新增存储
        //系列索引存储
        private Dictionary<string, List<Character>> OriginatingSeriesDic { get; } = new();
        //组织索引存储
        private Dictionary<string, List<Character>> AffiliationDic { get; } = new();
        #endregion

        #region 操作方法
        //增添
        public override void Add(CharacterData key, Character value)
        {
            base.Add(key, value);

            //创建系列名称索引存储
            if (key.OriginatingSeries != null)
            {
                if (!OriginatingSeriesDic.ContainsKey(key.OriginatingSeries))
                {
                    OriginatingSeriesDic[key.OriginatingSeries] = new List<Character>();
                }
                OriginatingSeriesDic[key.OriginatingSeries].Add(value);
            }
            //创建组织索引存储
            if (key.Affiliation != null)
            {
                if (!AffiliationDic.ContainsKey(key.Affiliation))
                {
                    AffiliationDic[key.Affiliation] = new List<Character>();
                }
                AffiliationDic[key.Affiliation].Add(value);
            }
        }
        public override void Add(Character value)
        {
            Add(value.BaseData, value);
        }
        //移除
        public override bool Remove(CharacterData key)
        {
            //从系列中移除
            OriginatingSeriesDic[key.OriginatingSeries].Remove(Store[key]);
            //从组织中移除
            AffiliationDic[key.Affiliation].Remove(Store[key]);
            //从存储中移除
            return base.Remove(key);
        }
        public override bool Remove(Character value)
        {
            return Remove(value.BaseData);
        }
        public override bool Remove(string id)
        {
            return Remove(IDDic[id]);
        }
        //获取系列所有角色
        public IEnumerable<Character> GetCharacterSeries(string originatingSeriesDic)
        {
            return OriginatingSeriesDic.ContainsKey(originatingSeriesDic) ? OriginatingSeriesDic[originatingSeriesDic] : Enumerable.Empty<Character>();
        }
        //获取组织所有角色
        public IEnumerable<Character> GetCharacterByAffliation(string affiliation)
        {
            return AffiliationDic.ContainsKey(affiliation) ? AffiliationDic[affiliation] : Enumerable.Empty<Character>();
        }
        #endregion

        #region 其他方法
        //更新查询方式
        protected override void InitFilters()
        {
            base.InitFilters();
            //新增所属作品系列查询
            Filters[QueryKeywordEnum.C_OriginatingSeries] = (datas, queryValue) =>
            {
                //类型检查
                if (queryValue is string originatingSeries)
                {
                    return datas.Where(d => d.OriginatingSeries == originatingSeries);
                }
                return datas;
            };
            //新增作品所属组织查询
            Filters[QueryKeywordEnum.C_Affiliation] = (datas, queryValue) =>
            {
                //类型检查
                if (queryValue is string affiliation)
                {
                    return datas.Where(d => d.Affiliation == affiliation);
                }
                return datas;
            };
        }
        #endregion
    }
}
