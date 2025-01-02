using System.Collections.Generic;
using System.Linq;
using THLL.SceneSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.EditorSystem.SceneEditor
{
    public class PathsInSceneEditorPanel : VisualElement
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
        private VisualElement PathsInSceneEditorRootPanel { get; set; }
        //ȫ��
        private Label FullNameLabel { get; set; }
        //��ͼ���ݵ�����
        private VisualElement ChordDiagramContainer { get; set; }
        //���ݿ�����
        private VisualElement DataBoxContainer { get; set; }
        #endregion

        #region ����
        //·���ֵ�
        public Dictionary<(string, string), ScenePath> PathsInScene { get; set; } = new();
        //��ͼ���ݵ��ֵ�
        private Dictionary<string, Label> ChordDiagramDataPoints { get; set; } = new();
        //��ǰָ������ݵ�
        private Label CurrentPoint { get; set; } = null;
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
        public PathsInSceneEditorPanel(VisualTreeAsset visualTree, MainWindow mainWindow)
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
            using ExecutionTimer timer = new("����·���༭����ʼ��", MainWindow.TimerDebugLogToggle.value);

            //��ȡUI�ؼ�
            //�������
            PathsInSceneEditorRootPanel = this.Q<VisualElement>("PathsInSceneEditorRootPanel");
            //ȫ��
            FullNameLabel = PathsInSceneEditorRootPanel.Q<Label>("FullNameLabel");
            //��ͼ����
            ChordDiagramContainer = PathsInSceneEditorRootPanel.Q<VisualElement>("ChordDiagramContainer");
            //���ݿ�����
            DataBoxContainer = PathsInSceneEditorRootPanel.Q<VisualElement>("DataBoxContainer");

            //ע���¼�
            RegisterEvents();
        }
        //ˢ�����
        public void PISRefresh()
        {
            //��ʱ
            using ExecutionTimer timer = new("����·���༭����ʼ��", MainWindow.TimerDebugLogToggle.value);

            //ˢ��ǰ������Դ�ı���
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //����Ƿ������ݱ�ѡ��
            if (ShowedScene != null)
            {
                //���У�����ȫ��
                SetFullName();
                //����·���ֵ�
                PathsInScene.Clear();
                foreach (ScenePath scenePath in ShowedScene.MapData.PathsInScene)
                {
                    (string, string) pathID = GetPathID(scenePath.SceneAID, scenePath.SceneBID);
                    PathsInScene[pathID] = scenePath;
                }
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
            ChordDiagramContainer.RegisterCallback<MouseDownEvent>(evt =>
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
                        ChordDiagramContainer.Add(CurrentDrawingLine);
                        //����ʼ����ǰ
                        label.BringToFront();
                    }
                }
            });
            //Ϊ���߲㴴������ƶ��¼�
            ChordDiagramContainer.RegisterCallback<MouseMoveEvent>(evt =>
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
            ChordDiagramContainer.RegisterCallback<MouseUpEvent>(evt =>
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
                        (string, string) lineID = GetPathID(CurrentDrawingLine.userData.ToString(), targetSceneID);
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
                            CreatePath(CurrentDrawingLine.userData.ToString(), targetSceneID);
                        }
                    }
                    //�Ƴ���ǰ���ڻ��Ƶ�����
                    ChordDiagramContainer.Remove(CurrentDrawingLine);
                    CurrentDrawingLine = null;
                }
                else
                {
                    //�����ǣ����жϻ���
                    IsDrawingLine = false;
                    //�Ƴ���ǰ���ڻ��Ƶ�����
                    if (CurrentDrawingLine != null)
                    {
                        ChordDiagramContainer.Remove(CurrentDrawingLine);
                        CurrentDrawingLine = null;
                    }
                }
                //���û���״̬Ϊ��
                IsDrawingLine = false;
            });

            //ÿ֡��������
            EditorApplication.update += UpdateLines;
        }
        #endregion

        #region ���ݵ���ɾ����
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
            ChordDiagramContainer.Clear();
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
                //���õ�ǰָ������ݵ�
                CurrentPoint = dataPoint;
            });
            //ע������뿪�¼�
            dataPoint.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                //ȡ���������ݵ�
                UnHighlight(dataPoint);
            });
            //ע���Ҽ��˵�
            dataPoint.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("����Ϊ�����", action => SetAsEntrance());
            }));
            //��ӵ�������
            ChordDiagramContainer.Add(dataPoint);
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

        #region ��ͼ���߼���Ӧ���ݿ�����
        //������ͼ����
        private void GenerateChordDiagramLines()
        {
            //��յ�ǰ����
            foreach (VisualElement line in ChordDiagramLines.Values)
            {
                if (ChordDiagramContainer.Contains(line))
                {
                    ChordDiagramContainer.Remove(line);
                }
            }
            //��������б�
            ChordDiagramLines.Clear();
            //������ݿ�����
            DataBoxContainer.Clear();
            //��⵱ǰ�����Ƿ�Ϊ��
            if (ShowedScene == null)
            {
                //��Ϊ�գ����˳�
                return;
            }
            
            //����·������
            foreach ((string fromSceneID, string toSceneID) in PathsInScene.Keys)
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
                    //�ݴ�����
                    userData = fromSceneID + "+" + toSceneID,
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
                ChordDiagramContainer.Add(line);
                //��ӵ������б�
                ChordDiagramLines[GetPathID(fromSceneID, toSceneID)] = line;
                //�������ݿ�
                GenerateDataBox(fromSceneID, toSceneID);
                //�����ݵ���ǰ
                fromDataPoint.BringToFront();
                toDataPoint.BringToFront();
            }
            else
            {
                //�������ݵ㲻���ڣ��������ݵ�֮һΪ�����Ŀ�����
                if (fromSceneID == ShowedScene.ID || toSceneID == ShowedScene.ID)
                {
                    //���ǣ����������ݿ�
                    GenerateDataBox(fromSceneID, toSceneID, true);
                }
            }
        }
        //�������ݿ�
        private void GenerateDataBox(string fromSceneID, string toSceneID, bool isEntrance = false)
        {
            //��ȡ·��ID
            (string, string) pathID = GetPathID(fromSceneID, toSceneID);
            //��������
            VisualElement dataBox = new()
            {
                //������ʽ
                style =
                {
                    //������߾�
                    marginTop = 3,
                    marginBottom = 3,
                    marginLeft = 2,
                    marginRight = 2,
                    //�߿���Ϊ1
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                }
            };
            //����Ƿ�Ϊ�����
            if (isEntrance)
            {
                //���ǣ���������ʽ
                dataBox.style.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
            }
            //ע���������¼�
            dataBox.RegisterCallback<MouseEnterEvent>(evt =>
            {
                //������
                Highlight(dataBox);
                //���Ի�ȡ����
                if (ChordDiagramLines.TryGetValue(pathID, out VisualElement line))
                {
                    //�����ڣ����������
                    Highlight(line);
                }
            });
            //ע������뿪�¼�
            dataBox.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                //ȡ��������
                UnHighlight(dataBox);
                //���Ի�ȡ����
                if (ChordDiagramLines.TryGetValue(pathID, out VisualElement line))
                {
                    //�����ڣ���ȡ����������
                    UnHighlight(line);
                }
            });
            //��������A��ǩ
            Label sceneALabel = new()
            {
                //�����ı�
                text = fromSceneID.Split("_").Last(),
                //������ʽ
                style =
                {
                    //���������С
                    fontSize = 16,
                    //����������ɫ
                    color = Color.white,
                    //������߾�
                    marginTop = 1,
                    marginBottom = 1,
                    marginLeft = 1,
                    marginRight = 1,
                }
            };
            //��������B��ǩ
            Label sceneBLabel = new()
            {
                //�����ı�
                text = toSceneID.Split("_").Last(),
                //������ʽ
                style =
                {
                    //���������С
                    fontSize = 16,
                    //����������ɫ
                    color = Color.white,
                    //������߾�
                    marginTop = 1,
                    marginBottom = 1,
                    marginLeft = 1,
                    marginRight = 1,
                }
            };
            //�����������ÿ�
            IntegerField distanceField = new()
            {
                //���ñ�ǩ
                label = "����",
                //�ӳ���Ӧ
                isDelayed = true,
                //������ʽ
                style =
                {
                    //���������С
                    fontSize = 16,
                    //����������ɫ
                    color = Color.white,
                    //������߾�
                    marginTop = 1,
                    marginBottom = 1,
                    marginLeft = 1,
                    marginRight = 1,
                }
            };
            //�趨����ֵ
            if (PathsInScene.ContainsKey(pathID))
            {
                //������·�����ݣ������þ���ֵ
                distanceField.SetValueWithoutNotify(PathsInScene[pathID].Distance);
            }
            //ע���������¼�
            distanceField.RegisterValueChangedCallback(evt =>
            {
                //���������ֵ
                if (evt.newValue < 0)
                {
                    //��С���㣬ǿ�������0
                    distanceField.value = 0;
                }
                else
                {
                    //���Ի�ȡ·������
                    if (PathsInScene.ContainsKey(pathID))
                    {
                        //�����ڣ�����¾���
                        PathsInScene[pathID].Distance = evt.newValue;
                    }
                }
            });
            //����ɾ����ť
            Button deleteButton = new()
            {
                //�����ı�
                text = "ɾ��",
                //������ʽ
                style =
                {
                    //���������С
                    fontSize = 16,
                    //����������ɫ
                    color = Color.white,
                    //������߾�
                    marginTop = 1,
                    marginBottom = 1,
                    marginLeft = 1,
                    marginRight = 1,
                }
            };
            //ע��ɾ����ť�¼�
            deleteButton.clicked += () =>
            {
                //ɾ��·��
                RemovePath(fromSceneID, toSceneID);
                //�Ƴ����ݿ�
                DataBoxContainer.Remove(dataBox);
                //��������Ƿ����
                if (ChordDiagramLines.TryGetValue(pathID, out VisualElement line))
                {
                    //�����ڣ����Ƴ�����
                    ChordDiagramContainer.Remove(line);
                }
                //���б����Ƴ�����
                ChordDiagramLines.Remove(pathID);
            };
            //��ӵ�������
            dataBox.Add(sceneALabel);
            dataBox.Add(sceneBLabel);
            dataBox.Add(distanceField);
            dataBox.Add(deleteButton);
            //�����ݿ���ӵ�������
            DataBoxContainer.Add(dataBox);
        }
        //��������
        private void UpdateLines()
        {
            //������������
            foreach (VisualElement line in ChordDiagramLines.Values)
            {
                //��ȡ�洢����
                if (line.userData is string data)
                {
                    //��ȡ��ʼ�����յ�λ��
                    string fromSceneID = data.Split("+")[0];
                    string toSceneID = data.Split("+")[1];
                    if (ChordDiagramDataPoints.TryGetValue(fromSceneID, out Label fromDataPoint) && ChordDiagramDataPoints.TryGetValue(toSceneID, out Label toDataPoint))
                    {
                        //��ȡ��ʼ�����յ�λ��
                        Vector2 fromPosition = fromDataPoint.localBound.center;
                        Vector2 toPosition = toDataPoint.localBound.center;
                        //�������߽Ƕ�
                        float angle = Mathf.Atan2(toPosition.y - fromPosition.y, toPosition.x - fromPosition.x);
                        //�������߳���
                        float length = Vector2.Distance(fromPosition, toPosition);
                        //��������
                        line.style.top = fromPosition.y;
                        line.style.left = fromPosition.x;
                        line.style.width = length;
                        line.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Radian)));
                    }
                }
            }
        }
        #endregion

        #region ·����ɾ�������
        //��������·��
        private bool CreatePath(string fromSceneID, string toSceneID)
        {
            //��ȡ����ID
            (string, string) lineID = GetPathID(fromSceneID, toSceneID);
            //����·������
            if (PathsInScene.ContainsKey(lineID))
            {
                //�����ڣ��򷵻�false
                return false;
            }
            //�������ڣ��򴴽�·������
            ScenePath scenePath = new()
            {
                SceneAID = fromSceneID,
                SceneBID = toSceneID,
            };
            //��ӵ�����������
            ShowedScene.MapData.PathsInScene.Add(scenePath);
            //��ӵ�·�������б�
            PathsInScene[lineID] = scenePath;
            //���سɹ�
            return true;
        }
        //ɾ��·��
        private bool RemovePath(string fromSceneID, string toSceneID)
        {
            //��ȡ����ID
            (string, string) lineID = GetPathID(fromSceneID, toSceneID);
            //���Ի�ȡ·������
            if (PathsInScene.ContainsKey(lineID))
            {
                //�ӳ���������ɾ��
                ShowedScene.MapData.PathsInScene.Remove(PathsInScene[lineID]);
                //��·���ֵ����Ƴ�
                PathsInScene.Remove(lineID);
                //����true
                return true;
            }
            //�������ڣ��򷵻�false
            return false;
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
        private (string, string) GetPathID(string str1, string str2)
        {
            return str1.CompareTo(str2) < 0 ? (str1, str2) : (str2, str1);
        }
        //����Ϊ��������ڣ�����Ϊ��ѡ�г������丸������֮�䴴��·��
        private void SetAsEntrance()
        {
            //����Ƿ�Ϸ�
            if (CurrentPoint != null)
            {
                //�Ϸ���״̬�£�����·��
                if (CreatePath(ShowedScene.ID, CurrentPoint.userData.ToString()))
                {
                    //�������ݿ�
                    GenerateDataBox(ShowedScene.ID, CurrentPoint.userData.ToString(), true);
                }
            }
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
