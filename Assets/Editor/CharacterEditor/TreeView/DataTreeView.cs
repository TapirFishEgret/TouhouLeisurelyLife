using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace THLL.GameEditor.CharacterEditor
{
    public class DataTreeView : TreeView
    {
        #region 构成
        //主面板
        public MainWindow MainWindow { get; private set; }
        #endregion

        #region 构造函数与刷新
        public DataTreeView(MainWindow window)
        {
            MainWindow = window;
        }
        #endregion
    }
}
