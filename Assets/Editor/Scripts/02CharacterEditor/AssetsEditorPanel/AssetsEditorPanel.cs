using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using THLL.CharacterSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.EditorSystem.CharacterEditor
{
    public class AssetsEditorPanel : Tab
    {
        #region ������
        //�����
        public MainWindow MainWindow { get; private set; }

        //��ʾ�Ľ�ɫ
        public CharacterData ShowedCharacter { get { return MainWindow.DataTreeView.ActiveSelection.Data; } }

        //����
        private VisualElement AssetsEditorRootPanel { get; set; }
        //��Ϣ��ʾ
        private Label FullInfoLabel { get; set; }
        //���ͷ��ť
        private Button AddAvatarButton { get; set; }
        //ͷ���������
        private ScrollView AvatarContainerScrollView { get; set; }
        //����-ͷ�������ֵ�
        private Dictionary<string, VisualElement> NameAvatarContainerDict { get; set; } = new();
        //������水ť
        private Button AddPortraitButton { get; set; }
        //�����������
        private ScrollView PortraitContainerScrollView { get; set; }
        //����-���������ֵ�
        private Dictionary<string, VisualElement> NamePortraitContainerDict { get; set; } = new();
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

            //���ñ�ǩҳ��������չ
            style.flexGrow = 1;
            contentContainer.style.flexGrow = 1;

            //�趨����
            label = "��Դ�༭���";

            //��ȡUI�ؼ�
            //�������
            AssetsEditorRootPanel = this.Q<VisualElement>("AssetsEditorRootPanel");
            //ȫ��
            FullInfoLabel = AssetsEditorRootPanel.Q<Label>("FullInfoLabel");
            //���ͷ��ť
            AddAvatarButton = AssetsEditorRootPanel.Q<Button>("AddAvatarButton");
            //ͷ���������
            AvatarContainerScrollView = AssetsEditorRootPanel.Q<ScrollView>("AvatarContainerScrollView");
            //������水ť
            AddPortraitButton = AssetsEditorRootPanel.Q<Button>("AddPortraitButton");
            //�����������
            PortraitContainerScrollView = AssetsEditorRootPanel.Q<ScrollView>("PortraitContainerScrollView");

            //ע���¼�
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            //���ͷ��ť����¼�
            AddAvatarButton.clicked += AddAvatar;
            //������水ť����¼�
            AddPortraitButton.clicked += AddPortrait;
        }
        //ˢ�����
        public async Task ARefresh()
        {
            //��ʱ
            using ExecutionTimer timer = new("��Դ�༭���ˢ��", MainWindow.TimerDebugLogToggle.value);

            //ˢ��ǰ������Դ�ı���
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //ɾ�������������ֵ�����
            AvatarContainerScrollView.Clear();
            NameAvatarContainerDict.Clear();
            PortraitContainerScrollView.Clear();
            NamePortraitContainerDict.Clear();

            //����Ƿ������ݱ�ѡ��
            if (MainWindow.DataTreeView.ActiveSelection != null)
            {
                //����
                //����ȫ��
                SetFullInfo();
                //��ȡͷ����Դ
                await ShowedCharacter.LoadAvatarsAsync(Path.GetDirectoryName(ShowedCharacter.SavePath), (name, avatar) => ShowAvatar(name, avatar));
                //��ȡ������Դ
                await ShowedCharacter.LoadPortraitsAsync(Path.GetDirectoryName(ShowedCharacter.SavePath), (name, portrait) => ShowPortrait(name, portrait));
            }
        }
        //����ͼ�θı�ʱ�ֶ�������С
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            //ͷ�񱣳ֳ���һ��
            NameAvatarContainerDict.Values.ToList().ForEach(avatarContainer =>
                avatarContainer.Q<VisualElement>("Container").style.height = avatarContainer.resolvedStyle.width);
            //���治������
        }
        #endregion

        #region ͷ����Դ����ɾ
        //��ʾͷ��
        public void ShowAvatar(string name, Sprite avatar)
        {
            //���Ի�ȡͷ������
            if (!NameAvatarContainerDict.TryGetValue(name, out VisualElement avatarContainer))
            {
                //���ޣ��򴴽�������Ԫ��
                avatarContainer = MainWindow.SpriteVisualElementTemplate.CloneTree();
                //�������������Image�ؼ�
                avatarContainer.Q<VisualElement>("Container").Add(new Image() { scaleMode = ScaleMode.ScaleToFit });
                //��ӵ�����������
                AvatarContainerScrollView.Add(avatarContainer);
                //��ӵ��ֵ���
                NameAvatarContainerDict.Add(name, avatarContainer);
            }
            //�޸�Ԫ��
            avatarContainer.Q<Image>().sprite = avatar;
            //��������
            avatarContainer.Q<Label>("NameLabel").text = name;
            //����ť���Ƴ��¼�
            avatarContainer.Q<Button>("RemoveButton").clicked += () => RemoveAvatar(name);
        }
        //����ͷ����Դ
        public void HideAvatar(string name)
        {
            //���Ի�ȡͷ������
            if (NameAvatarContainerDict.TryGetValue(name, out VisualElement avatarContainer))
            {
                //��������Ƴ�
                AvatarContainerScrollView.Remove(avatarContainer);
            }
        }
        //���ͷ����Դ
        public void AddAvatar()
        {
            //��ʾ���봰��
            TextInputWindow.ShowWindow(async avatarName =>
            {
                //�������������Ƿ��Ѵ��ڻ�Ϊ��
                if (string.IsNullOrEmpty(avatarName) || NameAvatarContainerDict.ContainsKey(avatarName))
                {
                    EditorUtility.DisplayDialog("Error", "Avatar Name is already exists or is empty!", "OK");
                    return;
                }

                //ѡ��Ŀ���ļ�
                string sourceFilePath = EditorUtility.OpenFilePanel("Select Avatar Image", "", "png,jpg,jpeg,bmp,webp,tiff,tif");
                //�ж�ѡ�����
                if (!string.IsNullOrEmpty(sourceFilePath))
                {
                    //����ѡ�У�������ָ��·��
                    string targetFilePath = Path.Combine(Path.GetDirectoryName(ShowedCharacter.SavePath), "Avatars", avatarName + ".png");
                    //�����ļ�
                    File.Copy(sourceFilePath, targetFilePath, true);

                    //������ֱ��ˢ�����
                    await ARefresh();
                }
            },
            "Add New Avatar",
            "Please Input New Avatar Name",
            "New Avatar Name",
            "New Name",
            EditorWindow.focusedWindow
            );
        }
        //�Ƴ�ͷ����Դ
        public void RemoveAvatar(string name)
        {
            //���ȣ�����ͷ��
            HideAvatar(name);
            //Ȼ�󣬻�ȡ���ͷ���Ŀ¼����Ϣ
            DirectoryInfo directory = new(Path.Combine(Path.GetDirectoryName(ShowedCharacter.SavePath), "Avatars"));
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

        #region ������Դ����ɾ
        //��ʾ����
        public void ShowPortrait(string name, Sprite portrait)
        {
            //���Ի�ȡ��������
            if (!NamePortraitContainerDict.TryGetValue(name, out VisualElement portraitContainer))
            {
                //���ޣ��򴴽�������Ԫ��
                portraitContainer = MainWindow.SpriteVisualElementTemplate.CloneTree();
                //�������������Image�ؼ�
                portraitContainer.Q<VisualElement>("Container").Add(new Image() { scaleMode = ScaleMode.ScaleToFit });
                //��ӵ�����������
                PortraitContainerScrollView.Add(portraitContainer);
                //��ӵ��ֵ���
                NamePortraitContainerDict.Add(name, portraitContainer);
            }
            //�޸�Ԫ��
            portraitContainer.Q<Image>().sprite = portrait;
            //��������
            portraitContainer.Q<Label>("NameLabel").text = name;
            //����ť���Ƴ��¼�
            portraitContainer.Q<Button>("RemoveButton").clicked += () => RemovePortrait(name);
        }
        //����������Դ
        public void HidePortrait(string name)
        {
            //���Ի�ȡ��������
            if (NamePortraitContainerDict.TryGetValue(name, out VisualElement portraitContainer))
            {
                //��������Ƴ�
                PortraitContainerScrollView.Remove(portraitContainer);
            }
        }
        //���������Դ
        public void AddPortrait()
        {
            //��ʾ���봰��
            TextInputWindow.ShowWindow(async portraitName =>
            {
                //�������������Ƿ��Ѵ��ڻ�Ϊ��
                if (string.IsNullOrEmpty(portraitName) || NamePortraitContainerDict.ContainsKey(portraitName))
                {
                    EditorUtility.DisplayDialog("Error", "Portrait Name is already exists or is empty!", "OK");
                    return;
                }

                //ѡ��Ŀ���ļ�
                string sourceFilePath = EditorUtility.OpenFilePanel("Select Portrait Image", "", "png,jpg,jpeg,bmp,webp,tiff,tif");
                //�ж�ѡ�����
                if (!string.IsNullOrEmpty(sourceFilePath))
                {
                    //����ѡ�У�������ָ��·��
                    string targetFilePath = Path.Combine(Path.GetDirectoryName(ShowedCharacter.SavePath), "Portraits", portraitName + ".png");
                    //�����ļ�
                    File.Copy(sourceFilePath, targetFilePath, true);

                    //������ֱ��ˢ�����
                    await ARefresh();
                }
            },
            "Add New Portrait",
            "Please Input New Portrait Name",
            "New Portrait Name",
            "New Name",
            EditorWindow.focusedWindow
            );
        }
        //�Ƴ�������Դ
        public void RemovePortrait(string name)
        {
            //���ȣ���������
            HidePortrait(name);
            //Ȼ�󣬻�ȡ��������Ŀ¼����Ϣ
            DirectoryInfo directory = new(Path.Combine(Path.GetDirectoryName(ShowedCharacter.SavePath), "Portraits"));
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
        public void SetFullInfo()
        {
            if (ShowedCharacter == null)
            {
                return;
            }
            //��/�ָ���ֶ�
            FullInfoLabel.text = string.Join("/", new string[] {
                ShowedCharacter.Series,
                ShowedCharacter.Group,
                ShowedCharacter.Chara,
                ShowedCharacter.Version
            });
            //������ɫ
            FullInfoLabel.style.color = ShowedCharacter.Color;
        }
        #endregion
    }
}
