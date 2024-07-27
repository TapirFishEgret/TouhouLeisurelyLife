using System;
using System.Collections.Generic;

namespace THLL.GameEditor.LocUnitDataEditor
{
    [Serializable]
    public class PersistentData
    {
        //要保存的数据
        public string DefaultPackage;
        public string DefaultAuthor;
        public bool TimerDebugLogState;
        public List<int> ExpandedState;
        public Dictionary<int, (float, float)> NodePositions;
    }
}
