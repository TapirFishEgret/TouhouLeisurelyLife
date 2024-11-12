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
        //����ID�����
        private TextField SceneIDTextField { get; set; }
        //��ˢ��ɫѡ����
        private ColorField BrushColorField { get; set; }
        //�ӳ����б�
        private ListView ChildScenesListView { get; set; }
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
            //����ID�����
            SceneIDTextField = MapEditorRootPanel.Q<TextField>("SceneIDTextField");
            //��ˢ��ɫѡ����
            BrushColorField = MapEditorRootPanel.Q<ColorField>("BrushColorField");
            //�ӳ����б�
            ChildScenesListView = MapEditorRootPanel.Q<ListView>("ChildScenesListView");
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
                //���У�����ȫ��
                SetFullName();
                //�����Ӽ������б�
                SetChildScenesList();
                //��ʾ������ͼ
                ShowSceneMap();
            }
        }
        //ע���¼�
        private void RegisterEvents()
        {
            //ע�����������ı��¼�
            ColCountIntegerField.RegisterValueChangedCallback(evt =>
            {
                //����ͼ�Ƿ�Ϊ��
                if (ShowedScene.MapData.IsEmpty)
                {
                    //��Ϊ�գ��򲻴���
                    return;
                }
                //��������Ƿ���Ч
                if (evt.newValue < 1)
                {
                    //����Ч��������Ϊ1
                    ColCountIntegerField.value = 1;
                }
                //�ش�����ͼ
                ShowedScene.MapData.CreateMap(evt.newValue, RowCountIntegerField.value);
                //��ʾ��ͼ
                ShowSceneMap();
            });
            //ע�����������ı��¼�
            RowCountIntegerField.RegisterValueChangedCallback(evt =>
            {
                //����ͼ�Ƿ�Ϊ��
                if (ShowedScene.MapData.IsEmpty)
                {
                    //��Ϊ�գ��򲻴���
                    return;
                }
                //��������Ƿ���Ч
                if (evt.newValue < 1)
                {
                    //����Ч��������Ϊ1
                    RowCountIntegerField.value = 1;
                }
                //�ش�����ͼ
                ShowedScene.MapData.CreateMap(ColCountIntegerField.value, evt.newValue);
                //��ʾ��ͼ
                ShowSceneMap();
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
                    //����һ�λ���
                    BrushMap(evt.target as VisualElement);
                }
                else if (evt.button == 1)
                {
                    //��Ϊ�Ҽ�����ⳡ��IDԪ���Ƿ�����
                    if (string.IsNullOrEmpty(SceneIDTextField.value))
                    {
                        //��û�У�����
                        return;
                    }
                    //�������ã�����һ������
                    SetCellScene(evt.target as VisualElement);
                }
            });
            //Ϊ��ͼ����������ˢ�ƶ���ɫ����
            MapContainer.RegisterCallback<MouseMoveEvent>(evt =>
            {
                //����ˢԪ���Ƿ�����
                if (string.IsNullOrEmpty(BrushTextField.value))
                {
                    //��û�У�����
                    return;
                }
                //�ƶ�ʱ����
                BrushMap(evt.target as VisualElement);
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

            //���ӳ����б��������
            ChildScenesListView.makeItem = () =>
            {
                //�����ӳ�����Ա�ǩ��ʽ��ʾ
                Label label = new()
                {
                    //���ñ�ǩ����
                    name = "ChildSceneItem",
                    //���ñ�ǩ��ʽ
                    style =
                    {
                        //���������С
                        fontSize = 16,
                        //�����ı�����
                        unityTextAlign = TextAnchor.MiddleCenter,
                        //��������Ϊ������
                        whiteSpace = WhiteSpace.NoWrap,
                    }
                };
                //����Ա�ǩ���ı�ˢ��ѡ��
                label.RegisterCallback<MouseDownEvent>(evt =>
                {
                    //�����갴ť
                    if (evt.button == 0)
                    {
                        //��Ϊ�����������ó���ID�ı�Ϊ��ǩ����
                        SceneIDTextField.value = label.userData.ToString();
                        //���ñ�ˢ��ɫΪ��ɫ
                        BrushColorField.value = Color.white;
                    }
                });
                //���ر�ǩ
                return label;
            };
            ChildScenesListView.bindItem = (item, index) =>
            {
                //���ӳ���������ı�
                (item as Label).text = ChildScenesListView.itemsSource[index].ToString().Split("_").Last();
                (item as Label).userData = ChildScenesListView.itemsSource[index].ToString();
            };

            //ע�ᴴ����ͼ��ť����¼�
            CreateMapButton.clicked += () =>
            {
                //����Ƿ��е�ͼ
                if (ShowedScene.MapData.IsEmpty)
                {
                    //��û�У�����һ��5��8�еĵ�ͼ
                    ShowedScene.MapData.CreateMap(8, 5);
                    //��ʾ��ͼ
                    ShowSceneMap();
                }
            };
            //ע��ɾ����ͼ��ť����¼�
            DeleteMapButton.clicked += () =>
            {
                //����Ƿ��е�ͼ
                if (!ShowedScene.MapData.IsEmpty)
                {
                    //����
                    ShowedScene.MapData.CreateMap(0, 0);
                    //��ʾ��ͼ
                    ShowSceneMap();
                }
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
        //�����Ӽ������б�
        private void SetChildScenesList()
        {
            //������Դָ��Ϊ��
            ChildScenesListView.itemsSource = null;
            //��ȡ�Ӽ�����
            var childScenes = MainWindow.DataTreeView.ChildrenDicCache[ShowedScene.ID.GetHashCode()];
            //��ȡ�Ӽ�ID�ַ���
            List<string> childSceneIDs = childScenes.Select(item => item.data.Data.ID).ToList();
            //ָ������Դ
            ChildScenesListView.itemsSource = childSceneIDs;
            //ˢ���б�
            ChildScenesListView.Rebuild();
        }
        //��ʾ������ͼ
        private void ShowSceneMap()
        {
            //����ɾ����ǰ��ͼ
            MapContainer.Clear();
            //Ȼ���ж���û�е�ͼ����
            if (ShowedScene.MapData.IsEmpty)
            {
                //��û�У����������з�һ����ʾ��ǩ
                Label noMapLabel = new()
                {
                    text = "��ǰ����û�е�ͼ����",
                };
                MapContainer.Add(noMapLabel);
                //�������������������
                ColCountIntegerField.SetValueWithoutNotify(0);
                RowCountIntegerField.SetValueWithoutNotify(0);
            }
            else
            {
                //���У�����ʾ��ͼ
                MapContainer.Add(ShowedScene.MapData.GetMap());
                //����ȡ��ͼ������
                int colCount = ShowedScene.MapData.Cells.Keys.Max(item => item.Item1) + 1;
                int rowCount = ShowedScene.MapData.Cells.Keys.Max(item => item.Item2) + 1;
                //�������������������
                ColCountIntegerField.SetValueWithoutNotify(colCount);
                RowCountIntegerField.SetValueWithoutNotify(rowCount);
            }
        }
        //��ˢ��ͼ
        private void BrushMap(VisualElement visualElement)
        {
            //���Ŀ���Ƿ�ΪLabel
            if (visualElement is Label label)
            {
                //���ǣ������userData�Ƿ�Ϊ��Ԫ��
                if (label.userData is MapCell cell)
                {
                    //���ǣ�����Ƿ��ڻ滭״̬
                    if (IsPainting)
                    {
                        //���ڻ滭״̬�����÷�ˢ����
                        cell.Brush(BrushTextField.value, BrushColorField.value);
                    }
                    else
                    {
                        //�����ڻ滭״̬����Ԥ��
                        label.text = BrushTextField.value;
                        label.style.color = BrushColorField.value;
                    }
                }
            }
        }
        //�趨��Ԫ��������ĳ���
        private void SetCellScene(VisualElement visualElement)
        {
            //���Ŀ���Ƿ�ΪLabel
            if (visualElement is Label label)
            {
                //���ǣ������userData�Ƿ�Ϊ��Ԫ��
                if (label.userData is MapCell cell)
                {
                    //���ǣ���鵱ǰ�������Ƿ����иó���
                    if (ShowedScene.MapData.DisplayedScenes.TryGetValue(SceneIDTextField.value, out MapCell oldCell))
                    {
                        //�����У������ˢΪ��
                        oldCell.Brush("��", Color.clear);
                        //�趨��Ԫ��Ϊ�ǳ�����Ԫ��
                        oldCell.IsScene = false;
                    }
                    //�趨��Ԫ��������ĳ���
                    cell.Brush(SceneIDTextField.value, BrushColorField.value);
                    //������ӵ���ʾ�б���
                    ShowedScene.MapData.DisplayedScenes[SceneIDTextField.value] = cell;
                    //�趨��Ԫ��Ϊ������Ԫ��
                    cell.IsScene = true;
                }
            }
        }
        #endregion
    }
}
