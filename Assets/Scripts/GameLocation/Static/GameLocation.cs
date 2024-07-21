namespace THLL.LocationSystem
{
    public static class GameLocation
    {
        #region 数据
        //游戏内所有地点实例数据库
        public static readonly LocUnitDb LocUnitDb = new();
        //游戏内所有地点连接数据
        public static readonly LocUnitConnDb LocUnitConnDb = new();
        #endregion

        #region 方法
        //初始化
        public static void Init()
        {
            //完成每个实例的初始化方法
            foreach (LocUnit locUnit in LocUnitDb)
            {
                locUnit.Init(LocUnitDb, LocUnitConnDb);
            }
        }
        #endregion
    }
}
