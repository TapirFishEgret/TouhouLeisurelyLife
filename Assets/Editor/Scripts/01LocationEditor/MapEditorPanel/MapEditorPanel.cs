using System.Collections.Generic;
using System.IO;
using THLL.SceneSystem;
using UnityEditor;
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
        private IntegerField RowCountIntegerField { get; set; }
        //�������������
        private IntegerField ColCountIntegerField { get; set; }
        //��ͼ����
        private VisualElement MapContainer { get; set; }
        //������ͼ��ť
        private Button CreateMapButton { get; set; }
        //ɾ����ͼ��ť
        private Button DeleteMapButton { get; set; }
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
            RowCountIntegerField = MapEditorRootPanel.Q<IntegerField>("RowCountIntegerField");
            //�������������
            ColCountIntegerField = MapEditorRootPanel.Q<IntegerField>("ColCountIntegerField");
            //��ͼ����
            MapContainer = MapEditorRootPanel.Q<VisualElement>("MapContainer");
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
                //��ȡ��ͼ
                ShowNewMap();
                //������ֵ
                RowCountIntegerField.SetValueWithoutNotify(ShowedScene.Map.Rows);
                ColCountIntegerField.SetValueWithoutNotify(ShowedScene.Map.Cols);
            }
        }
        //ע���¼�
        private void RegisterEvents()
        {
            //ע�Ἰ��ͼ�θı��¼�
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            //ע�����������ı��¼�
            RowCountIntegerField.RegisterValueChangedCallback(evt =>
            {
                //����Ƿ������ݱ�ѡ��
                if (ShowedScene == null)
                {
                    //��û�У�����
                    return;
                }
                //��ȡ����ֵ
                int newValue = evt.newValue;
                //����Ƿ�Ϸ�
                if (newValue < 1)
                {
                    //�����Ϸ�������Ϊ1
                    RowCountIntegerField.value = 1;
                    //���������ƶ�����
                    return;
                }
                //������ֵ
                ShowedScene.Map.Rows = newValue;
                //��ʾ��ȡ�µ�ͼ
                ShowNewMap();
            });
            //ע�����������ı��¼�
            ColCountIntegerField.RegisterValueChangedCallback(evt =>
            {
                //����Ƿ������ݱ�ѡ��
                if (ShowedScene == null)
                {
                    //��û�У�����
                    return;
                }
                //��ȡ����ֵ
                int newValue = evt.newValue;
                //����Ƿ�Ϸ�
                if (newValue < 1)
                {
                    //�����Ϸ�������Ϊ1
                    ColCountIntegerField.value = 1;
                    //���������ƶ�����
                    return;
                }
                //������ֵ
                ShowedScene.Map.Cols = newValue;
                //��ʾ�µ�ͼ
                ShowNewMap();
            });
            //ע�ᴴ����ͼ��ť����¼�
            CreateMapButton.clicked += () =>
            {
                //����Ƿ������ݱ�ѡ��
                if (ShowedScene == null)
                {
                    //��û�У�����
                    return;
                }
                //���У����½���ͼ
                ShowedScene.Map = new Map(5, 9);
                //������֪ͨ������¸���������ʾ��ֵ
                RowCountIntegerField.SetValueWithoutNotify(ShowedScene.Map.Rows);
                ColCountIntegerField.SetValueWithoutNotify(ShowedScene.Map.Cols);
                //��ʾ�µ�ͼ
                ShowNewMap();
            };
            //ע��ɾ����ͼ��ť����¼�
            DeleteMapButton.clicked += () =>
            {
                //����Ƿ������ݱ�ѡ��
                if (ShowedScene == null)
                {
                    //��û�У�����
                    return;
                }
                //���У���ɾ����ͼ������Ϊ�½�ʵ��
                ShowedScene.Map = new();
                //������֪ͨ������¸���������ʾ��ֵ
                RowCountIntegerField.SetValueWithoutNotify(ShowedScene.Map.Rows);
                ColCountIntegerField.SetValueWithoutNotify(ShowedScene.Map.Cols);
                //��ʾ�µ�ͼ
                ShowNewMap();
            };
        }
        //����ͼ�θı�ʱ
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            //��ȡ�µ�ͼ(��ʵ��˳��ı䵥Ԫ���С)
            ShowNewMap();
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
        //��ʾ�µ�ͼ
        private void ShowNewMap()
        {
            //���ѡ�г����Ƿ�Ϊ��
            if (ShowedScene == null)
            {
                //���ǣ�����
                return;
            }
            //�������ɵ�ͼ����Ӧ���ڴ�С
            MapContainer.Clear();
            MapContainer.Add(ShowedScene.Map.GetMap(MapContainer));
        }
        #endregion
    }
}
