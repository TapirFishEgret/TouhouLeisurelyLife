using System.Collections.Generic;
using System.IO;
using THLL.CharacterSystem;
using THLL.SceneSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.EditorSystem
{
    public static class GameEditor
    {
        #region ��Ϸ���ݴ洢
        //��ɫ���ݴ洢
        public static Dictionary<string, CharacterData> CharacterDataDict { get; } = new();
        //�������ݴ洢
        public static Dictionary<string, SceneData> SceneDataDict { get; } = new();
        #endregion

        #region �ļ���ط���
        //ȷ��Ŀ��·������
        public static bool MakeSureFolderPathExist(string folderPath, bool isUnityEditorFolder = false)
        {
            //���·������
            if (isUnityEditorFolder)
            {
                //����Unity�༭��·����ʹ����ط��������ļ���
                //���·���Ƿ����
                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    //�������ڣ����������
                    //�ָ�·��
                    string[] folders = folderPath.Split("\\");
                    string currentPath = string.Empty;

                    //�𼶼�鲢�����ļ���
                    for (int i = 0; i < folders.Length; i++)
                    {
                        //��ȡ�ļ���
                        string folder = folders[i];
                        //�ж��Ƿ�ֱ���ڸ�Ŀ¼��
                        if (i == 0 && folder == "Assets")
                        {
                            //���ǣ�ָ����ǰ·��Ϊ��Ŀ¼
                            currentPath = folder;
                            //�������˴�ѭ��
                            continue;
                        }
                        //������·��
                        string newPath = Path.Combine(currentPath, folder);
                        //�ж���·���Ƿ����
                        if (!AssetDatabase.IsValidFolder(newPath))
                        {
                            //�������ڣ��򴴽�
                            AssetDatabase.CreateFolder(currentPath, folder);
                        }
                        //ָ����ǰ·��Ϊ��·��
                        currentPath = newPath;
                    }
                    //�������󱣴�
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    //����������������ļ��б��������ڣ�����false
                    return false;
                }
                else
                {
                    //���Ѵ��ڸ��ļ��У��򷵻�true
                    return true;
                }
            }
            else
            {
                //������ͨ·������ֱ���ж�
                if (!Directory.Exists(folderPath))
                {
                    //�������ڣ��򴴽�
                    Directory.CreateDirectory(folderPath);
                    //�������
                    AssetDatabase.SaveAssets();
                    //��������ˢ��
                    AssetDatabase.Refresh();
                    //����������������ļ��б��������ڣ�����false
                    return false;
                }
                else
                {
                    //���Ѵ��ڸ��ļ��У��򷵻�true
                    return true;
                }
            }
        }
        //ͨ���ݹ���ȫɾ���ļ���
        public static void DeleteFolder(string folderPath, bool isUnityEditorFolder = false)
        {
            //���·������
            if (isUnityEditorFolder)
            {
                //����Unity�༭��·����ʹ����ط���ɾ���ļ���
                //��ȡ�ļ����е������ļ������ļ���GUID
                string[] asseGUIDs = AssetDatabase.FindAssets("", new[] { folderPath });

                //��Ի�ȡ������·��
                foreach (string assetGUID in asseGUIDs)
                {
                    //ȷ��·��
                    string path = AssetDatabase.GUIDToAssetPath(assetGUID);

                    if (AssetDatabase.IsValidFolder(path))
                    {
                        //�����ļ��У�����еݹ�ɾ��
                        DeleteFolder(path);
                    }
                    else
                    {
                        //�����ļ�����ɾ��
                        AssetDatabase.DeleteAsset(path);
                    }

                    //���ɾ�����ļ���
                    AssetDatabase.DeleteAsset(folderPath);
                }

                //�������
                AssetDatabase.SaveAssets();
                //ˢ����Դ��ͼ
                AssetDatabase.Refresh();
            }
            else
            {
                //�����ǣ���ȡĿ¼����
                string folderName = Path.GetFileName(folderPath);
                //ɾ��meta�ļ�����Ҳ��֪��ΪʲôUnityҪ��StreamingAssets��Ҳ����Meta�ļ�
                File.Delete(Path.Combine(Path.GetDirectoryName(folderPath), folderName + ".meta"));
                //ɾ����Ŀ¼
                Directory.Delete(folderPath, true);
                //�������
                AssetDatabase.SaveAssets();
                //ˢ����Դ��ͼ
                AssetDatabase.Refresh();
            }
        }
        //����ռλ�ı��ĵ�
        public static void GeneratePlaceHolderTextFile(string directoryPath)
        {
            //ȷ��·������
            MakeSureFolderPathExist(directoryPath);
            //�����ļ�
            string filePath = Path.Combine(directoryPath, "PlaceHolder.txt");
            File.WriteAllText(filePath,
                "����һ��ռλ�ļ���" +
                "\n��Ҫ��Ϊ�˷�ֹUnity�ڹ����������Զ�ɾ��StreamingAssets�еĿ��ļ���" +
                "\nThis file is a placeholder." +
                "\nIt is mainly used to prevent Unity from automatically deleting empty folders in StreamingAssets during the build process.");
            //�������
            AssetDatabase.SaveAssets();
            //ˢ����Դ��ͼ
            AssetDatabase.Refresh();
        }
        #endregion

        #region �Ӿ�Ԫ�ع�����չ
        //Label�Ӿ�Ԫ��ʵ�������Զ�����
        public static void SingleLineLabelAdjustFontSizeToFit(Label label)
        {
            //���ȼ�⴫��
            if (label == null
                || label.text == null
                || label.style.display == DisplayStyle.None
                || float.IsNaN(label.resolvedStyle.width)
                || float.IsNaN(label.resolvedStyle.height))
            {
                //���޴�����ߴ���Label�����ı���Labelѹ������û��ʾ��ߺͿ�ΪNaN���򷵻�
                return;
            }

            //��¼ѭ������
            int count = 0;
            //Ȼ������ı�����
            Vector2 textSize = label.MeasureTextSize(
                label.text,
                float.MaxValue,
                VisualElement.MeasureMode.Undefined,
                float.MaxValue,
                VisualElement.MeasureMode.Undefined);
            //��ȡLabel���
            float labelWidth = label.resolvedStyle.width;
            float labelHeight = label.resolvedStyle.height;
            //ѭ�������Ե��������С
            while (true)
            {
                //��ǰ�����С
                float currentFontSize = label.resolvedStyle.fontSize;
                //�����������С
                float adjustedFontSize = currentFontSize;
                //����ı������Label��ȹ�ϵ
                if (textSize.x > labelWidth)
                {
                    //�����ڣ�����С
                    adjustedFontSize = currentFontSize - 0.1f;
                }
                else if (textSize.x < labelWidth && textSize.y < labelHeight)
                {
                    //���ı����С�ڵ�ǰ��ȣ����ı��߶�ͬ��С�ڣ���Ŵ�
                    adjustedFontSize = currentFontSize + 0.1f;
                }
                //���������С
                label.style.fontSize = adjustedFontSize;
                //���²�������
                textSize = label.MeasureTextSize(
                    label.text,
                    float.MaxValue,
                    VisualElement.MeasureMode.Undefined,
                    float.MaxValue,
                    VisualElement.MeasureMode.Undefined);
                //����Ƿ���������ѭ������
                //�������Σ���Ƚ��ƶ��߶�С�����С���߶Ƚ���
                if ((Mathf.Abs(textSize.x - labelWidth) < 4f && textSize.y < labelHeight)
                    || (Mathf.Abs(textSize.y - labelHeight) < 4f && textSize.x < labelWidth))
                {
                    //����������ѭ��������������
                    break;
                }
                //ѭ����������
                count++;
                //���
                if (count > 100)
                {
                    //��ѭ���������࣬��������
                    break;
                }
            }
        }
        #endregion
    }
}
