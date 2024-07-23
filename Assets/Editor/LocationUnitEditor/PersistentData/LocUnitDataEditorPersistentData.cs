using System;
using System.Collections.Generic;

namespace THLL.GameEditor
{
    [Serializable]
    public class LocUnitDataEditorPersistentData
    {
        //要保存的数据
        public string DefaultPackage;
        public string DefaultAuthor;
        public bool TimerDebugLogState;
        public bool IsDataEditorPanelOpen;
        public List<int> ExpandedState;
    }
}
