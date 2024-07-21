using System;

namespace THLL.LocationSystem
{
    [Serializable]
    public struct LocUnitDataConn
    {
        //另一处地点
        public LocUnitData otherLocUnit;
        //通行时间
        public int duration;
    }
}
