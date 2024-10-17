using System.Collections.Generic;
using System.IO;
using THLL.SceneSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.EditorSystem.SceneEditor
{
    public class MapEditorPanel : Tab
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
        private IntegerField ColumnCountIntegerField { get; set; }
        //��ͼ����
        private VisualElement MapContainer { get; set; }
        #endregion

        #region ���ݱ༭���ĳ�ʼ���Լ����ݸ���
        //��������
        public MapEditorPanel(VisualTreeAsset visualTree, MainWindow mainWindow)
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
            using ExecutionTimer timer = new("��ͼ�༭����ʼ��", MainWindow.TimerDebugLogToggle.value);

            //�趨��������ʽ
            label = "��ͼ�༭���";
            style.flexGrow = 1;
            contentContainer.style.flexGrow = 1;
            contentContainer.style.backgroundColor = Color.gray;

            //��ȡUI�ؼ�
            //�������
            MapEditorRootPanel = this.Q<VisualElement>("MapEditorRootPanel");
            //ȫ��
            FullNameLabel = MapEditorRootPanel.Q<Label>("FullNameLabel");
            //�������������
            RowCountIntegerField = MapEditorRootPanel.Q<IntegerField>("RowCountIntegerField");
            //�������������
            ColumnCountIntegerField = MapEditorRootPanel.Q<IntegerField>("ColumnCountIntegerField");
            //��ͼ����
            MapContainer = MapEditorRootPanel.Q<VisualElement>("MapContainer");

            //ע���¼�
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<MouseDownEvent>(evt => Debug.Log(evt.target.ToString()));
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
                //��յ�ͼ����
                MapContainer.Clear();
                //���ɵ�ͼ
                ShowedScene.Map = new();
                //��ȡ��ͼ
                MapContainer.Add(ShowedScene.Map.GetMap());
            }
        }
        //����ͼ�θı�ʱ�ֶ�������С
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            
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
