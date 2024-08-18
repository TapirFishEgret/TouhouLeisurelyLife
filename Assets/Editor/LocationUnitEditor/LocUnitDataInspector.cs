using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using THLL.LocationSystem;
using UnityEditor;
using UnityEngine;

namespace THLL.GameEditor.LocUnitDataEditor
{
    [CustomEditor(typeof(LocUnitData))]
    public class LocUnitDataInspector : Editor
    {
        //覆写Inspector的UI界面，使元素只读，并添加按钮打开编辑窗口
        public override void OnInspectorGUI()
        {
            //开始禁用GUI
            EditorGUI.BeginDisabledGroup(true);
            //绘制默认GUI
            DrawDefaultInspector();
            //结束禁用
            EditorGUI.EndDisabledGroup();

            //添加空行
            EditorGUILayout.Space();

            //添加按钮
            if (GUILayout.Button("Open Editor Window"))
            {
                MainWindow.ShowWindow();
            }
        }
    }
}
