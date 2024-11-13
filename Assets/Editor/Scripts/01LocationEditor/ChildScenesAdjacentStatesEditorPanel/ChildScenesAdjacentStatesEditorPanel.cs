using System.Collections.Generic;
using System.Linq;
using THLL.SceneSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.EditorSystem.SceneEditor
{
    public class ChildScenesAdjacentStatesEditorPanel : VisualElement
    {
        #region ����
        //��ͼ�뾶
        private const float ChordDiagramRadius = 300f;
        #endregion

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
        private VisualElement ChildScenesAdjacentStatesEditorRootPanel { get; set; }
        //ȫ��
        private Label FullNameLabel { get; set; }
        //��ͼ���ݵ�����
        private VisualElement ChordDiagramDataPointContainer { get; set; }
        //��ͼ��������
        private VisualElement ChordDiagramLineContainer { get; set; }
        #endregion

        #region ����
        //��ͼ���ݵ��ֵ�
        private Dictionary<string, Label> ChordDiagramDataPoints { get; set; } = new();
        //�����ֵ�
        private Dictionary<(string, string), VisualElement> ChordDiagramLines { get; set; } = new();
        //��ǰ���ڻ��Ƶ�����
        private VisualElement CurrentDrawingLine { get; set; } = null;
        //��ǰ�������ߵ����
        private Vector2 StartDrawLinePoint { get; set; } = Vector2.zero;
        //�Ƿ����ڻ�������
        private bool IsDrawingLine { get; set; } = false;
        #endregion 

        #region ���ݱ༭���ĳ�ʼ���Լ����ݸ���
        //��������
        public ChildScenesAdjacentStatesEditorPanel(VisualTreeAsset visualTree, MainWindow mainWindow)
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
            using ExecutionTimer timer = new("�ӳ�������״̬�༭����ʼ��", MainWindow.TimerDebugLogToggle.value);

            //��ȡUI�ؼ�
            //�������
            ChildScenesAdjacentStatesEditorRootPanel = this.Q<VisualElement>("ChildScenesAdjacentStatesEditorRootPanel");
            //ȫ��
            FullNameLabel = ChildScenesAdjacentStatesEditorRootPanel.Q<Label>("FullNameLabel");
            //��ͼ����
            ChordDiagramDataPointContainer = ChildScenesAdjacentStatesEditorRootPanel.Q<VisualElement>("ChordDiagramDataPointContainer");
            //��ͼ��������
            ChordDiagramLineContainer = ChildScenesAdjacentStatesEditorRootPanel.Q<VisualElement>("ChordDiagramLineContainer");

            //ע���¼�
            RegisterEvents();
        }
        //ˢ�����
        public void CSASRefresh()
        {
            //��ʱ
            using ExecutionTimer timer = new("�ӳ�������״̬�༭����ʼ��", MainWindow.TimerDebugLogToggle.value);

            //ˢ��ǰ������Դ�ı���
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //����Ƿ������ݱ�ѡ��
            if (MainWindow.DataTreeView.ActiveSelection != null)
            {
                //���У�����ȫ��
                SetFullName();
                //������ͼ���ݵ�
                GenerateChordDiagramDataPoints();
                //������ͼ����
                GenerateChordDiagramLines();
            }
        }
        //ע���¼�
        private void RegisterEvents()
        {
            //Ϊ���ݲ㴴�������������ʼ�����¼�
            ChordDiagramDataPointContainer.RegisterCallback<MouseDownEvent>(evt =>
            {
                //�����갴��
                if (evt.button == 0)
                {
                    //��Ϊ����������겶��Ŀ���Ƿ�Ϊ���ݵ�
                    if (evt.target is Label label && ChordDiagramDataPoints.ContainsKey(label.userData.ToString()))
                    {
                        //���ǣ���ʼ��������
                        IsDrawingLine = true;
                        //���õ�ǰ���ڻ��Ƶ�����
                        CurrentDrawingLine = new VisualElement()
                        {
                            //����userData���ݴ���ʼ���ݵ�ID
                            userData = label.userData.ToString(),
                            //������ʽ
                            style =
                            {
                                //����λ��Ϊ����
                                position = Position.Absolute,
                                //����λ��ΪĿ�����������top
                                top = label.localBound.center.y,
                                //���λ��ΪĿ�����������left
                                left = label.localBound.center.x,
                                //������λ�����������Ͻ�
                                transformOrigin = new StyleTransformOrigin(new TransformOrigin(0,0)),
                                //���ÿ��Ϊ0
                                width = 0,
                                //���ø߶�Ϊ3
                                height = 3f,
                                //���ñ���ɫΪ���
                                backgroundColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)),
                            }
                        };
                        //��¼��ʼλ��
                        StartDrawLinePoint = new Vector2(label.localBound.center.x, label.localBound.center.y);
                        //��ӵ�������
                        ChordDiagramDataPointContainer.Add(CurrentDrawingLine);
                        //����ʼ����ǰ
                        label.BringToFront();
                    }
                }
            });
            //Ϊ���߲㴴������ƶ��¼�
            ChordDiagramDataPointContainer.RegisterCallback<MouseMoveEvent>(evt =>
            {
                //�����ڻ�������
                if (IsDrawingLine)
                {
                    //��ȡ���λ��
                    Vector2 mousePosition = evt.localMousePosition;
                    //���㳤��
                    float length = Vector2.Distance(StartDrawLinePoint, mousePosition);
                    //����Ƕ�
                    float angle = Mathf.Atan2(mousePosition.y - StartDrawLinePoint.y, mousePosition.x - StartDrawLinePoint.x);
                    //���õ�ǰ���ڻ��Ƶ����ߵĿ��
                    CurrentDrawingLine.style.width = length;
                    //���õ�ǰ���ڻ��Ƶ����ߵ���ת�Ƕ�
                    CurrentDrawingLine.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Radian)));
                }
            });
            //Ϊ���߲㴴�����̧���¼�
            ChordDiagramDataPointContainer.RegisterCallback<MouseUpEvent>(evt =>
            {
                //����Ƿ��ڻ�������
                if (IsDrawingLine)
                {
                    //���ǣ����Ի�ȡĿ�����ݵ�
                    if (evt.target is Label label && ChordDiagramDataPoints.ContainsKey(label.userData.ToString()))
                    {
                        //����ȡ�ɹ������Ի�ȡĿ�����ݵ��ID
                        string targetSceneID = label.userData.ToString();
                        //��ȡ����ID
                        (string, string) lineID = GetLineID(CurrentDrawingLine.userData.ToString(), targetSceneID);
                        //���ID�Ƿ���ͬ
                        if (CurrentDrawingLine.userData.ToString() == targetSceneID)
                        {
                            //����ͬ������ʾ
                            Debug.Log("�������ӵ��Լ�");
                            //�жϻ���
                            IsDrawingLine = false;
                        }
                        //����Ƿ��Ѵ��ڸ�����
                        else if (ChordDiagramLines.ContainsKey(lineID))
                        {
                            //���ǣ�����ʾ
                            Debug.Log("�������Ѵ���");
                            //�жϻ���
                            IsDrawingLine = false;
                        }
                        else
                        {
                            //����ͬ������������
                            GenerateChordDiagramLine(CurrentDrawingLine.userData.ToString(), targetSceneID, CurrentDrawingLine.style.backgroundColor.value);
                            //����������
                            CreateAdjacentState(CurrentDrawingLine.userData.ToString(), targetSceneID);
                        }
                    }
                    //�Ƴ���ǰ���ڻ��Ƶ�����
                    ChordDiagramDataPointContainer.Remove(CurrentDrawingLine);
                    CurrentDrawingLine = null;
                }
                else
                {
                    //�����ǣ����жϻ���
                    IsDrawingLine = false;
                    //�Ƴ���ǰ���ڻ��Ƶ�����
                    if (CurrentDrawingLine != null)
                    {
                        ChordDiagramDataPointContainer.Remove(CurrentDrawingLine);
                        CurrentDrawingLine = null;
                    }
                }
                //���û���״̬Ϊ��
                IsDrawingLine = false;
            });

            //Ϊ���߲㴴�����ڴ�С�����������¼�
            ChordDiagramDataPointContainer.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                //ˢ����ͼ����
                GenerateChordDiagramLines();
            });
        }
        #endregion

        #region ��������
        //�������ݵ�
        public void AddChordDiagramDataPoint(string sceneID)
        {
            //�������ݵ�
            GenerateChordDiagramDataPoint(sceneID);
            //������ͼ���ݵ�λ��
            UpdateChordDiagramDataPointsPosition();
        }
        //ɾ�����ݵ�
        public void RemoveChordDiagramDataPoint(string sceneID)
        {
            //�Ƴ�����
            ChordDiagramDataPoints.Remove(sceneID);
            //������ͼ���ݵ�λ��
            UpdateChordDiagramDataPointsPosition();
        }
        #endregion

        #region ��ͼ���ݵ�����
        //������ͼ���ݵ�
        private void GenerateChordDiagramDataPoints()
        {
            //��յ�ǰ��ͼ����
            ChordDiagramDataPointContainer.Clear();
            //������ݵ��б�
            ChordDiagramDataPoints.Clear();
            //��⵱ǰ�����Ƿ�Ϊ��
            if (ShowedScene == null)
            {
                //��Ϊ�գ����˳�
                return;
            }
            //����Ϊ�գ���ȡ���ӳ���ID����
            List<string> childSceneIDs = MainWindow.DataTreeView.ChildrenDicCache[ShowedScene.ID.GetHashCode()]
                .Select(x => x.data.Data.ID)
                .ToList();

            //�����ӳ���ID
            for (int i = 0; i < childSceneIDs.Count; i++)
            {
                //Ϊ����ID����Label��Ϊ���ݵ�
                GenerateChordDiagramDataPoint(childSceneIDs[i]);
            }
            //������ͼ���ݵ�λ��
            UpdateChordDiagramDataPointsPosition();
        }
        //�������ݵ�
        private Label GenerateChordDiagramDataPoint(string sceneID)
        {
            //�������ݵ��ǩ
            Label dataPoint = new()
            {
                //��������
                name = "ChordDiagramDataPoint" + sceneID,
                //�����ı�
                text = sceneID.Split("_").Last(),
                //��ȫ�ε�ID�����ݴ���userData��
                userData = sceneID,
                //������ʽ
                style =
                {
                    //����λ��Ϊ����
                    position = Position.Absolute,
                    //�����Ų�Ϊ����
                    unityTextAlign = TextAnchor.MiddleCenter,
                    //���������СΪ16
                    fontSize = 16,
                    //��������߾�Ϊ0
                    marginTop = 0,
                    marginBottom = 0,
                    marginLeft = 0,
                    marginRight = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
                    paddingLeft = 0,
                    paddingRight = 0,
                    //���ñ߿�Ϊ1
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    //���ñ���ɫΪ��
                    backgroundColor = Color.grey,
                    //����������ɫΪ��ɫ
                    color = Color.white,
                }
            };
            //ע���������¼�
            dataPoint.RegisterCallback<MouseEnterEvent>(evt =>
            {
                //�������ݵ�
                Highlight(dataPoint);
            });
            //ע������뿪�¼�
            dataPoint.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                //ȡ���������ݵ�
                UnHighlight(dataPoint);
            });
            //��ӵ�������
            ChordDiagramDataPointContainer.Add(dataPoint);
            //��ӵ����ݵ��б�
            ChordDiagramDataPoints[sceneID] = dataPoint;
            //�������ݵ��ǩ
            return dataPoint;
        }
        //������ͼ���ݵ��λ��
        private void UpdateChordDiagramDataPointsPosition()
        {
            //����ǶȲ���
            float angleStep = 360f / ChordDiagramDataPoints.Count;
            //������¼
            int count = 0;
            //�������ݵ�
            foreach (Label dataPoint in ChordDiagramDataPoints.Values)
            {
                //���㻡��
                float radian = angleStep * count * Mathf.Deg2Rad;
                //��������
                Vector2 position = new(ChordDiagramRadius * Mathf.Cos(radian), ChordDiagramRadius * Mathf.Sin(radian));
                //��������
                dataPoint.style.translate = new StyleTranslate(new Translate(position.x, position.y));
                //��������
                count++;
            }
        }
        #endregion

        #region ��ͼ��������
        //������ͼ����
        private void GenerateChordDiagramLines()
        {
            //��յ�ǰ����
            foreach (VisualElement line in ChordDiagramLines.Values)
            {
                if (ChordDiagramDataPointContainer.Contains(line))
                {
                    ChordDiagramDataPointContainer.Remove(line);
                }
            }
            //��������б�
            ChordDiagramLines.Clear();
            //��⵱ǰ�����Ƿ�Ϊ��
            if (ShowedScene == null)
            {
                //��Ϊ�գ����˳�
                return;
            }
            //����Ϊ�գ���ȡ�ӳ�������״̬����
            List<(string, string)> adjacentStates = MainWindow.DataTreeView.ItemDicCache[ShowedScene.ID.GetHashCode()]
                .data
                .Data
                .ChildScenesAdjacentStates
                .ToList();

            //��������״̬����
            foreach ((string fromSceneID, string toSceneID) in adjacentStates)
            {
                //��������
                GenerateChordDiagramLine(fromSceneID, toSceneID);
            }
        }
        //���������Ӿ�Ԫ��
        private void GenerateChordDiagramLine(string fromSceneID, string toSceneID, Color color = default)
        {
            //���Ի�ȡ���ݵ�
            if (ChordDiagramDataPoints.TryGetValue(fromSceneID, out Label fromDataPoint) && ChordDiagramDataPoints.TryGetValue(toSceneID, out Label toDataPoint))
            {
                //��ȡ��ʼ�����յ�λ��
                Vector2 fromPosition = fromDataPoint.localBound.center;
                Vector2 toPosition = toDataPoint.localBound.center;
                //�������߽Ƕ�
                float angle = Mathf.Atan2(toPosition.y - fromPosition.y, toPosition.x - fromPosition.x);
                //�������߳���
                float length = Vector2.Distance(fromPosition, toPosition);
                //���������Ӿ�Ԫ��
                VisualElement line = new()
                {
                    //��������
                    name = "ChordDiagramLine" + fromSceneID + toSceneID,
                    //������ʽ
                    style =
                    {
                        //����λ��Ϊ����
                        position = Position.Absolute,
                        //����λ��Ϊ��ʼ��y
                        top = fromPosition.y,
                        //���λ��Ϊ��ʼ��x
                        left = fromPosition.x,
                        //������λ�����������Ͻ�
                        transformOrigin = new StyleTransformOrigin(new TransformOrigin(0,0)),
                        //���ÿ��Ϊ���߳���
                        width = length,
                        //���ø߶�Ϊ3
                        height = 3f,
                        //�߿���Ϊ1
                        borderTopWidth = 1,
                        borderBottomWidth = 1,
                        borderLeftWidth = 1,
                        borderRightWidth = 1,
                    }
                };
                //ע���������¼�
                line.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    //��������
                    Highlight(line);
                });
                //ע������뿪�¼�
                line.RegisterCallback<MouseLeaveEvent>(evt =>
                {
                    //ȡ����������
                    UnHighlight(line);
                });
                //ע���Ҽ�ɾ���¼�
                line.RegisterCallback<MouseDownEvent>(evt =>
                {
                    if (evt.button == 1)
                    {
                        //ɾ������
                        RemoveAdjacentState(fromSceneID, toSceneID);
                        //�Ƴ�����
                        ChordDiagramDataPointContainer.Remove(line);
                        //�Ƴ������б�
                        ChordDiagramLines.Remove(GetLineID(fromSceneID, toSceneID));
                    }
                });
                //�����ɫ
                if (color == default)
                {
                    //��δָ����ɫ�����������
                    color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                }
                //���ñ���ɫ
                line.style.backgroundColor = color;
                //������ת�Ƕ�
                line.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Radian)));
                //��ӵ�������
                ChordDiagramDataPointContainer.Add(line);
                //��ӵ������б�
                ChordDiagramLines[GetLineID(fromSceneID, toSceneID)] = line;
                //�����ݵ���ǰ
                fromDataPoint.BringToFront();
                toDataPoint.BringToFront();
            }
        }
        #endregion

        #region ���ݸ���
        //��������״̬
        private void CreateAdjacentState(string fromSceneID, string toSceneID)
        {
            //��ȡ����ID
            (string, string) lineID = GetLineID(fromSceneID, toSceneID);
            //���Ի�ȡ����״̬����
            if (MainWindow.DataTreeView.ItemDicCache[ShowedScene.ID.GetHashCode()].data.Data.ChildScenesAdjacentStates.Contains(lineID))
            {
                //�����ڣ����˳�
                return;
            }
            //�������ڣ��򴴽�����״̬����
            MainWindow.DataTreeView.ItemDicCache[ShowedScene.ID.GetHashCode()].data.Data.ChildScenesAdjacentStates.Add(lineID);
        }
        //ɾ������״̬
        private void RemoveAdjacentState(string fromSceneID, string toSceneID)
        {
            //��ȡ����ID
            (string, string) lineID = GetLineID(fromSceneID, toSceneID);
            //���Ի�ȡ����״̬����
            if (MainWindow.DataTreeView.ItemDicCache[ShowedScene.ID.GetHashCode()].data.Data.ChildScenesAdjacentStates.Contains(lineID))
            {
                //�����ڣ���ɾ��
                MainWindow.DataTreeView.ItemDicCache[ShowedScene.ID.GetHashCode()].data.Data.ChildScenesAdjacentStates.Remove(lineID);
            }
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
        //��ȡ����ID
        private (string, string) GetLineID(string str1, string str2)
        {
            return str1.CompareTo(str2) < 0 ? (str1, str2) : (str2, str1);
        }
        //��������
        private void Highlight(VisualElement ve)
        {
            //��������ʽΪ��ʾ��ɫ�߿�
            ve.style.borderTopColor = Color.white;
            ve.style.borderBottomColor = Color.white;
            ve.style.borderLeftColor = Color.white;
            ve.style.borderRightColor = Color.white;
        }
        //ȡ����������
        private void UnHighlight(VisualElement ve)
        {
            //��������ʽΪ��ʾ͸���߿�
            ve.style.borderTopColor = Color.clear;
            ve.style.borderBottomColor = Color.clear;
            ve.style.borderLeftColor = Color.clear;
            ve.style.borderRightColor = Color.clear;
        }
        #endregion
    }
}
