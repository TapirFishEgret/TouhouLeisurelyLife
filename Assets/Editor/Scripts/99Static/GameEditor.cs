using System.Collections.Generic;
using System.IO;
using THLL.CharacterSystem;
using THLL.SceneSystem;
using UnityEditor;
using UnityEngine;

namespace THLL.EditorSystem
{
    public static class GameEditor
    {
        #region ��Ϸ���ݴ洢
        //���������Ƿ���Ҫ���ؽ�
        public static bool IsSceneDataDirty { get; set; } = true;
        //�������ݴ洢
        private static Dictionary<string, SceneData> _sceneDataDict;
        public static Dictionary<string, SceneData> SceneDataDict
        {
            get
            {
                //����Ƿ���Ҫ�ؽ�
                if (IsSceneDataDirty)
                {
                    //����Ҫ�����ؽ�
                    ReadSceneData();
                    //�ؽ���ɺ�������
                    IsSceneDataDirty = false;
                }
                //���س��������ֵ�
                return _sceneDataDict;
            }
        }
        //��ɫ�����Ƿ���Ҫ���ؽ�
        public static bool IsCharacterDataDirty { get; set; } = true;
        //��ɫ���ݴ洢
        private static Dictionary<string, CharacterData> _characterDataDict;
        public static Dictionary<string, CharacterData> CharacterDataDict
        {
            get
            {
                //����Ƿ���Ҫ�ؽ�
                if (IsCharacterDataDirty)
                {
                    //����Ҫ�����ؽ�
                    ReadCharacterData();
                    //�ؽ���ɺ�������
                    IsCharacterDataDirty = false;
                }
                //���ؽ�ɫ�����ֵ�
                return _characterDataDict;
            }
        }
        #endregion

        #region ���ݶ�ȡ��ط���
        //�������ݶ�ȡ
        private static void ReadSceneData()
        {
            //��ʼ�����������ֵ�
            _sceneDataDict = new Dictionary<string, SceneData>();

            //���ݴ洢·��
            string rootPath = Path.Combine(Application.streamingAssetsPath, "Scene");
            //ȷ��·������
            MakeSureFolderPathExist(rootPath);

            //��ȡ���г�������
            try
            {
                //��ȡ�����ļ�
                string[] filePaths = Directory.GetFiles(rootPath, "*.json", SearchOption.AllDirectories);
                //���������ļ�
                foreach (string filePath in filePaths)
                {
                    //����Ƿ�ΪĿ�������ļ�
                    if (Path.GetFileNameWithoutExtension(filePath).StartsWith("SceneData"))
                    {
                        //���ǣ���ȡ����
                        SceneData sceneData = SceneData.LoadFromJson<SceneData>(filePath);
                        //�趨��ȡ��ַ
                        sceneData.DataPath = filePath.Replace("\\", "/");
                        //��ӵ��ֵ�
                        _sceneDataDict[sceneData.ID] = sceneData;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("��ȡ��������ʧ�ܣ�" + e.Message);
            }
        }
        //��ɫ���ݶ�ȡ
        private static void ReadCharacterData()
        {
            //��ʼ����ɫ�����ֵ�
            _characterDataDict = new Dictionary<string, CharacterData>();

            //���ݴ洢·��
            string rootPath = Path.Combine(Application.streamingAssetsPath, "Character");
            //ȷ��·������
            MakeSureFolderPathExist(rootPath);

            //��ȡ���н�ɫ����
            try
            {
                //��ȡ�����ļ�
                string[] filePaths = Directory.GetFiles(rootPath, "*.json", SearchOption.AllDirectories);
                //���������ļ�
                foreach (string filePath in filePaths)
                {
                    //����Ƿ�ΪĿ�������ļ�
                    if (Path.GetFileNameWithoutExtension(filePath).StartsWith("CharacterData"))
                    {
                        //���ǣ���ȡ����
                        CharacterData characterData = CharacterData.LoadFromJson<CharacterData>(filePath);
                        //�趨��ȡ��ַ
                        characterData.DataPath = filePath.Replace("\\", "/");
                        //��ӵ��ֵ�
                        _characterDataDict[characterData.ID] = characterData;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("��ȡ��ɫ����ʧ�ܣ�" + e.Message);
            }
        }
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
    }
}
