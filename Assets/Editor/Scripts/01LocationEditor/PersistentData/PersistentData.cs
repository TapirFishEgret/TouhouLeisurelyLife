using System;
using System.Collections.Generic;

namespace THLL.EditorSystem.SceneEditor
{
    [Serializable]
    public class PersistentData
    {
        //要保存的数据
        public bool TimerDebugLogState;
        public List<int> ExpandedState;
    }
}
