using System;
using UnityEditor;
using UnityEngine;

namespace THLL.GameEditor
{
    //文本输入窗口，用于弹出文本输入框
    public class TextInputWindow : EditorWindow
    {
        //窗口字段
        //描述
        private string wndDesc = string.Empty;
        //标签
        private string labelName = string.Empty;
        //确认事件
        private Action<string> onConfirm;
        //文本框是否需要被聚焦
        private bool isTextFieldNeedFocus = true;

        //结果字段
        //文本
        private string newText = string.Empty;
        //原窗口
        private EditorWindow originalWindow;

        //弹出窗口方法
        public static void ShowWindow(Action<string> onConfirm, string wndName, string wndDesc, string labelName, string newText, EditorWindow originalWindow)
        {
            TextInputWindow window = GetWindow<TextInputWindow>(wndName);
            window.onConfirm = onConfirm;
            window.wndDesc = wndDesc;
            window.labelName = labelName;
            window.newText = newText;
            window.originalWindow = originalWindow;
            //聚焦新窗口
            window.Focus();
        }

        //UI绘制
        private void OnGUI()
        {
            //创建标题与文本输入框
            GUILayout.Label(wndDesc, EditorStyles.boldLabel);
            GUI.SetNextControlName(labelName);
            newText = EditorGUILayout.TextField(labelName, newText);

            //初次打开时直接聚焦文本框
            if (isTextFieldNeedFocus)
            {
                EditorGUI.FocusTextInControl(labelName);
                isTextFieldNeedFocus = false;
            }

            //检测回车键的触发
            if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
            {
                //确认输入
                ConfirmInput();
                //阻断事件传递
                Event.current.Use();
            }

            //添加确认按钮
            if (GUILayout.Button("Confirm"))
            {
                //执行时确认输入
                ConfirmInput();
            }
        }

        //方法
        //确认输入
        private void ConfirmInput()
        {
            //检测名称是否为空
            if (!string.IsNullOrEmpty(newText))
            {
                //不为空，触发
                onConfirm?.Invoke(newText);
                //关闭窗口
                Close();
                //恢复窗口聚焦到原窗口
                originalWindow.Focus();
            }
            else
            {
                //为空，输出信息
                EditorUtility.DisplayDialog("无效文本", "文本不能为空。", "OK");
            }
        }
    }
}
