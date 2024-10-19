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
        //��ˢ��ɫ������
        private TextField ColorSearcherField { get; set; }
        //�й�ɫѡ����
        private ListView ChineseColorSelector { get; set; }
        //������ͼ��ť
        private Button CreateMapButton { get; set; }
        //ɾ����ͼ��ť
        private Button DeleteMapButton { get; set; }
        #endregion

        #region ����
        //�Ƿ��ڻ滭
        private bool IsPainting { get; set; }
        //�й�ɫ�洢
        private (List<string>, Dictionary<string, Color>) ChineseColors { get; set; }
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
            //��ˢ��ɫ������
            ColorSearcherField = MapEditorRootPanel.Q<TextField>("ColorSearcherField");
            //�й�ɫѡ����
            ChineseColorSelector = MapEditorRootPanel.Q<ListView>("ChineseColorSelector");
            //������ͼ��ť
            CreateMapButton = MapEditorRootPanel.Q<Button>("CreateMapButton");
            //ɾ����ͼ��ť
            DeleteMapButton = MapEditorRootPanel.Q<Button>("DeleteMapButton");

            //�й�ɫѡ������ʼ�������Ȼ�ȡ������ɫ
            ChineseColors = ChineseColor.FindColors();
            //Ȼ���趨����Դ
            ChineseColorSelector.itemsSource = ChineseColors.Item1;
            //Ȼ���趨makeItem
            ChineseColorSelector.makeItem = () =>
            {
                //������ͨ��Label��Ϊѡ����
                Label item = new();
                //����Label
                return item;
            };
            //Ȼ���趨bindItem
            ChineseColorSelector.bindItem = (element, i) =>
            {
                //�趨Label���ı�Ϊ��ɫ����
                (element as Label).text = ChineseColorSelector.itemsSource[i] as string;
                //�趨Label����ɫΪ��ɫֵ
                (element as Label).style.color = ChineseColors.Item2[ChineseColors.Item1[i]];
            };

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
            }
        }
        //ע���¼�
        private void RegisterEvents()
        {
            //ע�Ἰ��ͼ�θı��¼�
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
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
                //�����С
                ShowedScene.MapData.Cols = newValue;
                //��ʾ�µ�ͼ
                ShowNewMap();
            });
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
                //���������С
                ShowedScene.MapData.Rows = newValue;
                //��ʾ��ȡ�µ�ͼ
                ShowNewMap();
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
                    //������һ�α�ˢ
                    BrushCell(evt.target as VisualElement);
                }
            });
            //Ϊ��ͼ����������ˢ�ƶ���ɫ����
            MapContainer.RegisterCallback<MouseMoveEvent>(evt =>
            {
                //����Ƿ��ڻ滭
                if (IsPainting)
                {
                    //���ǣ���ˢ��Ԫ��
                    BrushCell(evt.target as VisualElement);
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
            //Ϊ������ע�������¼�
            ColorSearcherField.RegisterValueChangedCallback(evt =>
            {
                //ɸѡ�����ɫ�б�
                List<string> filteredColors = ChineseColors.Item1.Where(color => color.Contains(evt.newValue)).ToList();
                //�趨ɸѡ�����ɫ�б�
                ChineseColorSelector.itemsSource = filteredColors;
                //ˢ����ʾ
                ChineseColorSelector.Rebuild();
            });
            //Ϊ��ɫѡ����ע��ѡ���¼�
            ChineseColorSelector.itemsChosen += (items) =>
            {
                //��ȡѡ�����ɫ����
                if (items.First() is string colorName)
                {
                    //����ȡ�ɹ��������ñ�ˢ��ɫ
                    BrushColorField.value = ChineseColors.Item2[colorName];
                }
            };
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
                ShowedScene.MapData = new MapData() { Cols = 10, Rows = 10 };
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
                ShowedScene.MapData = new();
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
            //�������ɵ�ͼ
            MapContainer.Clear();
            MapContainer.Add(ShowedScene.MapData.GetMap());
            //������֪ͨ������¸���������ʾ��ֵ
            ColCountIntegerField.SetValueWithoutNotify(ShowedScene.MapData.Cols);
            RowCountIntegerField.SetValueWithoutNotify(ShowedScene.MapData.Rows);
        }
        //��ˢ��Ԫ��
        private void BrushCell(VisualElement visualElement)
        {
            //�жϴ�������
            if (visualElement is Label label)
            {
                //����Label�����Ի�ȡuserData
                if (label.userData is ValueTuple<int, int> coords)
                {
                    //����userData�����ȡ��Ԫ��
                    int x = coords.Item1;
                    int y = coords.Item2;
                    //���ֵ��в��ҵ�Ԫ��
                    if (ShowedScene.MapData.Cells.TryGetValue((x, y), out MapCell cell))
                    {
                        //���ҵ���Ԫ�������õ�Ԫ������Ϊ��ˢ����
                        cell.Text = BrushTextField.value;
                        label.text = BrushTextField.value;
                        //���õ�Ԫ����ɫΪ��ˢ��ɫ
                        cell.TextColor = BrushColorField.value;
                        label.style.color = BrushColorField.value;
                        //��������Ԫ�������С
                        label.style.fontSize = new StyleLength(new Length(label.resolvedStyle.width / label.text.Length, LengthUnit.Pixel));
                    }
                }
            }
        }
        #endregion
    }
}
