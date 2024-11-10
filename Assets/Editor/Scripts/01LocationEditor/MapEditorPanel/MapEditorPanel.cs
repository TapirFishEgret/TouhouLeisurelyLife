using System;
using System.Collections.Generic;
using System.Linq;
using THLL.SceneSystem;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.EditorSystem.SceneEditor
{
    public class MapEditorPanel : VisualElement
    {
        #region ������
        //�����
        public MainWindow MainWindow { get; private set; }

        //��ʾ�ĳ���
        public SceneData ShowedScene
        {
            get
            {
                //�ж��Ƿ������ݱ�ѡ��
                if (MainWindow.DataTreeView.ActiveSelection == null)
                {
                    return null;
                }
                //��ȡѡ������
                return MainWindow.DataTreeView.ActiveSelection.Data;
            }
        }

        //�������
        private VisualElement MapEditorRootPanel { get; set; }
        //ȫ��
        private Label FullNameLabel { get; set; }
        //�������������
        private IntegerField ColCountIntegerField { get; set; }
        //�������������
        private IntegerField RowCountIntegerField { get; set; }
        //��ˢ����
        private VisualElement BrushContainer { get; set; }
        //��ͼ����
        private VisualElement MapContainer { get; set; }
        //��ˢ���������
        private TextField BrushTextField { get; set; }
        //��ˢ��ɫѡ����
        private ColorField BrushColorField { get; set; }
        //������ͼ��ť
        private Button CreateMapButton { get; set; }
        //ɾ����ͼ��ť
        private Button DeleteMapButton { get; set; }
        #endregion

        #region ����
        //�Ƿ��ڻ滭
        private bool IsPainting { get; set; }
        #endregion

        #region ���ݱ༭���ĳ�ʼ���Լ����ݸ���
        //��������
        public MapEditorPanel(VisualTreeAsset visualTree, MainWindow mainWindow)
        {
            //��������Ϊ����չ������
            style.flexGrow = 1;
            style.display = DisplayStyle.None;

            //��ȡ���
            visualTree.CloneTree(this);

            //ָ��������
            MainWindow = mainWindow;

            //��ʼ��
            Init();
        }
        //��ʼ��
        private void Init()
        {
            //��ʱ
            using ExecutionTimer timer = new("��ͼ�༭����ʼ��", MainWindow.TimerDebugLogToggle.value);

            //��ȡUI�ؼ�
            //�������
            MapEditorRootPanel = this.Q<VisualElement>("MapEditorRootPanel");
            //ȫ��
            FullNameLabel = MapEditorRootPanel.Q<Label>("FullNameLabel");
            //�������������
            ColCountIntegerField = MapEditorRootPanel.Q<IntegerField>("ColCountIntegerField");
            //�������������
            RowCountIntegerField = MapEditorRootPanel.Q<IntegerField>("RowCountIntegerField");
            //��ˢ����
            BrushContainer = MapEditorRootPanel.Q<VisualElement>("BrushContainer");
            //��ͼ����
            MapContainer = MapEditorRootPanel.Q<VisualElement>("MapContainer");
            //��ˢ���������
            BrushTextField = MapEditorRootPanel.Q<TextField>("BrushTextField");
            //��ˢ��ɫѡ����
            BrushColorField = MapEditorRootPanel.Q<ColorField>("BrushColorField");
            //������ͼ��ť
            CreateMapButton = MapEditorRootPanel.Q<Button>("CreateMapButton");
            //ɾ����ͼ��ť
            DeleteMapButton = MapEditorRootPanel.Q<Button>("DeleteMapButton");

            //ע���¼�
            RegisterEvents();
        }
        //ˢ�����
        public void MRefresh()
        {
            //��ʱ
            using ExecutionTimer timer = new("��ͼ�༭���ˢ��", MainWindow.TimerDebugLogToggle.value);

            //ˢ��ǰ������Դ�ı���
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //����Ƿ������ݱ�ѡ��
            if (MainWindow.DataTreeView.ActiveSelection != null)
            {
                //����
                //����ȫ��
                SetFullName();
            }
        }
        //ע���¼�
        private void RegisterEvents()
        {
            //ע�Ἰ��ͼ�θı��¼�
            RegisterCallback<GeometryChangedEvent>(evt =>
            {

            });

            //ע�����������ı��¼�
            ColCountIntegerField.RegisterValueChangedCallback(evt =>
            {

            });
            //ע�����������ı��¼�
            RowCountIntegerField.RegisterValueChangedCallback(evt =>
            {

            });

            //Ϊÿ����ˢ��ťע�����¼�
            BrushContainer.Query<Button>().ForEach(button =>
            {
                //����Ƿ�Ϊ��ˢ
                if (button.name.StartsWith("Brush"))
                {
                    //���ǣ�ע�����¼�
                    button.clicked += () =>
                    {
                        //���õ�ǰ��ˢ����Ϊ��ť��ʾ�ı�
                        BrushTextField.value = button.text;
                        //���õ�ǰ��ˢ��ɫΪ��ť��ɫ
                        BrushColorField.value = button.resolvedStyle.color;
                    };
                }
            });
            //Ϊ��ͼ����������갴���¼�
            MapContainer.RegisterCallback<MouseDownEvent>(evt =>
            {
                //����Ƿ������ݱ�ѡ��
                if (ShowedScene == null)
                {
                    //��û�У�����
                    return;
                }
                //��ⰴ��
                if (evt.button == 0)
                {
                    //��Ϊ���������ˢԪ���Ƿ�����
                    if (string.IsNullOrEmpty(BrushTextField.value))
                    {
                        //��û�У�����
                        return;
                    }
                    //����ָ�밴�±�־
                    IsPainting = true;
                }
            });
            //Ϊ��ͼ����������ˢ�ƶ���ɫ����
            MapContainer.RegisterCallback<MouseMoveEvent>(evt =>
            {
                //����Ƿ��ڻ滭
                if (IsPainting)
                {

                }
            });
            //Ϊ��ͼ�����������̧���¼�
            MapContainer.RegisterCallback<MouseUpEvent>(evt =>
            {
                //��ⰴ��
                if (evt.button == 0)
                {
                    //��Ϊ�����ȡ���滭
                    IsPainting = false;
                }
            });
            //Ϊ��ͼ������������뿪�¼�
            MapContainer.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                //�����滭
                IsPainting = false;
            });

            //ע�ᴴ����ͼ��ť����¼�
            CreateMapButton.clicked += () =>
            {

            };
            //ע��ɾ����ͼ��ť����¼�
            DeleteMapButton.clicked += () =>
            {

            };
        }
        #endregion

        #region ��������
        //��ȡ������ȫ��
        private void SetFullName()
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
