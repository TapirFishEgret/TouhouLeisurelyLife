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
        public List<int> ExpandedState;
    }
}
