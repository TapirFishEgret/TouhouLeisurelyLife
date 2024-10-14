using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using THLL.SceneSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.EditorSystem.SceneEditor
{
    public class AssetsEditorPanel : Tab
    {
        #region ������
        //�����
        public MainWindow MainWindow { get; private set; }

        //��ʾ�ĳ���
        public SceneData ShowedScene { get => MainWindow.DataTreeView.ActiveSelection.Data; }

        //�������
        private VisualElement AssetsEditorRootPanel { get; set; }
        //ȫ��
        private Label FullNameLabel { get; set; }
        //��ӱ���ͼ��ť
        private Button AddBackgroundButton { get; set; }
        //����ͼ��������
        private ScrollView AssetsContainerScrollView { get; set; }
        //����-����ͼ�ֵ�
        private Dictionary<string, VisualElement> NameBackgroundContainerDict { get; set; } = new();
        #endregion

        #region ���ݱ༭���ĳ�ʼ���Լ����ݸ���
        //��������
        public AssetsEditorPanel(VisualTreeAsset visualTree, MainWindow mainWindow)
        {
            //��ȡ���
            VisualElement panel = visualTree.CloneTree();
            Add(panel);

            //ָ��������
            MainWindow = mainWindow;

            //��ʼ��
            Init();
        }
        //��ʼ��
        private void Init()
        {
            //��ʱ
            using ExecutionTimer timer = new("��Դ�༭����ʼ��", MainWindow.TimerDebugLogToggle.value);

            //�趨����
            label = "��Դ�༭���";

            //��ȡUI�ؼ�
            //�������
            AssetsEditorRootPanel = this.Q<VisualElement>("AssetsEditorRootPanel");
            //ȫ��
            FullNameLabel = AssetsEditorRootPanel.Q<Label>("FullNameLabel");
            //��ӱ���ͼ��ť
            AddBackgroundButton = AssetsEditorRootPanel.Q<Button>("AddBackgroundButton");
            //����ͼ��������
            AssetsContainerScrollView = AssetsEditorRootPanel.Q<ScrollView>("AssetsContainerScrollView");

            //ע���¼�
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            AddBackgroundButton.clicked += AddBackground;
        }
        //ˢ�����
        public async Task ARefresh()
        {
            //��ʱ
            using ExecutionTimer timer = new("��Դ�༭���ˢ��", MainWindow.TimerDebugLogToggle.value);

            //ˢ��ǰ������Դ�ı���
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //ɾ�����б���ͼ����
            AssetsContainerScrollView.Clear();
            NameBackgroundContainerDict.Clear();

            //����Ƿ������ݱ�ѡ��
            if (MainWindow.DataTreeView.ActiveSelection != null)
            {
                //����
                //����ȫ��
                SetFullName();
                //���ز���ʾ����ͼ
                await ShowedScene.LoadBackgroundsAsync(Path.GetDirectoryName(ShowedScene.SavePath), (name, background) => ShowBackground(name, background));
            }
        }
        //����ͼ�θı�ʱ�ֶ�������С
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            //�������������߶�
            NameBackgroundContainerDict.Values.ToList().ForEach(backgroundContainer =>
                backgroundContainer.Q<VisualElement>("Container").style.height = backgroundContainer.resolvedStyle.width * 9 / 16);
        }
        #endregion

        #region ����ͼ��Դ����ɾ
        //��ʾ����ͼ��Դ
        public void ShowBackground(string name, Sprite background)
        {
            //���Ի�ȡ����ͼ����
            if (!NameBackgroundContainerDict.TryGetValue(name, out VisualElement backgroundContainer))
            {
                //���ޣ��򴴽�������Ԫ��
                backgroundContainer = MainWindow.BackgroundAssetContainerVisualTree.CloneTree();
                //������ʽ
                backgroundContainer.style.width = new StyleLength(new Length(50, LengthUnit.Percent));
                //�������������Image�ؼ�
                backgroundContainer.Q<VisualElement>("Container").Add(new Image() { scaleMode = ScaleMode.ScaleAndCrop });
                //��ӵ�����������
                AssetsContainerScrollView.Add(backgroundContainer);
                //��ӵ��ֵ���
                NameBackgroundContainerDict.Add(name, backgroundContainer);
            }
            //�޸�Ԫ��
            backgroundContainer.Q<Image>().sprite = background;
            //��������
            backgroundContainer.Q<Label>("NameLabel").text = name;
            //����ť���Ƴ��¼�
            backgroundContainer.Q<Button>("RemoveButton").clicked += () => RemoveBackground(name);
        }
        //���ر���ͼ��Դ
        public void HideBackground(string name)
        {
            //���Ի�ȡ����ͼ����
            if (NameBackgroundContainerDict.TryGetValue(name, out VisualElement backgroundContainer))
            {
                //���������Ƴ�
                AssetsContainerScrollView.Remove(backgroundContainer);
            }
        }
        //��ӱ���ͼ��Դ
        public void AddBackground()
        {
            //��ʾ���봰��
            TextInputWindow.ShowWindow(async backgroundName =>
            {
                //�������������Ƿ��Ѵ��ڻ�Ϊ��
                if (string.IsNullOrEmpty(backgroundName) || NameBackgroundContainerDict.ContainsKey(backgroundName))
                {
                    EditorUtility.DisplayDialog("Error", "Background Name is already exists or is empty!", "OK");
                    return;
                }

                //ѡ��Ŀ���ļ�
                string sourceFilePath = EditorUtility.OpenFilePanel("Select Background Image", "", "png,jpg,jpeg,bmp,webp,tiff,tif");
                //�ж�ѡ�����
                if (!string.IsNullOrEmpty(sourceFilePath))
                {
                    //����ѡ�У�������ָ��·��
                    string targetFilePath = Path.Combine(Path.GetDirectoryName(ShowedScene.SavePath), "Backgrounds", backgroundName + ".png");
                    //�����ļ�
                    File.Copy(sourceFilePath, targetFilePath, true);

                    //������ֱ��ˢ�����
                    await ARefresh();
                }
            },
            "Add New Background",
            "Please Input New Background Name",
            "New Background Name",
            "New Name",
            EditorWindow.focusedWindow
            );
        }
        //�Ƴ�����ͼ��Դ
        public void RemoveBackground(string name)
        {
            //���ȣ����ر���ͼ
            HideBackground(name);
            //Ȼ�󣬻�ȡ��ű���ͼ��Ŀ¼����Ϣ
            DirectoryInfo directory = new(Path.Combine(Path.GetDirectoryName(ShowedScene.SavePath), "Backgrounds"));
            //����Ŀ¼��ɾ���ļ�
            foreach (FileInfo file in directory.GetFiles())
            {
                if (Path.GetFileNameWithoutExtension(file.Name).Equals(name, System.StringComparison.OrdinalIgnoreCase))
                {
                    //ɾ��meta�ļ�
                    File.Delete(file.FullName + ".meta");
                    //ɾ���ļ�
                    file.Delete();
                }
            }
            //���沢ˢ����Դ
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        #endregion

        #region ��������
        //��ȡ������ȫ��
        public void SetFullName()
        {
            //ȫ���б�
            List<string> names = new();
            //����ǰ���Ʋ���
            names.Insert(0, ShowedScene.Name);
            //��ȡ����ID
            string parnetID = ShowedScene.ParentSceneID;
            //���и���
            while (!string.IsNullOrEmpty(parnetID))
            {
                //���Ի�ȡ��������
                if (MainWindow.DataTreeView.ItemDicCache.ContainsKey(parnetID.GetHashCode()))
                {
                    //��ȡ��������
                    SceneData parentData = MainWindow.DataTreeView.ItemDicCache[parnetID.GetHashCode()].data.Data;
                    //���������Ʋ���
                    names.Insert(0, parentData.Name);
                    //���¸���ID
                    parnetID = parentData.ParentSceneID;
                }
                else
                {
                    //�������ݣ����˳�ѭ��
                    break;
                }
            }
            //����ȫ����ʾ
            FullNameLabel.text = string.Join("/", names);
        }
        #endregion
    }
}
