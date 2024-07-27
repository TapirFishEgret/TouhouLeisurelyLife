using System.Collections.Generic;

namespace THLL.LocationSystem
{
    public static class GameLocation
    {
        #region 数据
        //游戏内所有地点实例数据库
        private static readonly LocUnitDb _locUnitDb = new();
        public static LocUnitDb LocUnitDb => _locUnitDb;
        //游戏内所有地点连接数据
        private static readonly Dictionary<LocUnit, Dictionary<LocUnit, int>> _locUnitConnDic;
        public static Dictionary<LocUnit, Dictionary<LocUnit, int>> LocUnitConnDic => _locUnitConnDic;
        #endregion

        #region 方法
        //初始化
        public static void Init()
        {
            //完成每个实例的初始化方法
            foreach (LocUnit locUnit in LocUnitDb)
            {
                locUnit.Init(LocUnitDb, LocUnitConnDic);
            }
        }
        #endregion
    }
}
