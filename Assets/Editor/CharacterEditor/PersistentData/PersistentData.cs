using System;
using System.Collections.Generic;

namespace THLL.GameEditor.CharacterDataEditor
{
    [Serializable]
    public class PersistentData
    {
        //要保存的数据
        public string DefaultPackage = "";
        public string DefaultAuthor = "";
        public bool TimerDebugLogState = true;
        public List<int> ExpandedState = new();
    }
}
