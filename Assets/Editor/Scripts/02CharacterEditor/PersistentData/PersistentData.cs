using System;
using System.Collections.Generic;

namespace THLL.EditorSystem.CharacterDataEditor
{
    [Serializable]
    public class PersistentData
    {
        //要保存的数据
        public bool TimerDebugLogState = true;
        public List<int> ExpandedState = new();
    }
}
