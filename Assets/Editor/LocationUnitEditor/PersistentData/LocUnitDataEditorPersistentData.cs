using System;
using System.Collections.Generic;

namespace THLL.GameEditor
{
    [Serializable]
    public class LocUnitDataEditorPersistentData
    {
        //要保存的数据
        public string Package;
        public string Category;
        public string Author;
        public List<int> ExpandedState;
    }
}
