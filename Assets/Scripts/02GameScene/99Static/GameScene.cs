using System.Collections.Generic;
using System.Linq;
using THLL.BaseSystem;
using UnityEngine;

namespace THLL.SceneSystem
{
    public static class GameScene
    {
        #region ��Ϸ��Դ
        //Ĭ�ϱ���ͼ
        public static Sprite DefaultBackground => GameAssetsManager.Instance.DefaultBackground;
        #endregion

        #region ���ݴ洢
        //��ǰʹ�õĳ������ݴ洢
        public static Dictionary<string, Scene> Storage { get; private set; } = new();
        //���������ݴ洢
        public static Dictionary<string, Scene> RootScenes { get; private set; } = new();
        //�����������ݴ洢
        private static Dictionary<string, Dictionary<string, Scene>> AllScenes { get; set; } = new();
        //�������Ӵ洢
        private static Dictionary<Scene, Dictionary<Scene, int>> SceneConnections { get; set; } = new();
        //������������
        private static Dictionary<Scene, HashSet<Scene>> Index_ParentScene { get; set; } = new();
        //�ܼ���
        public static int TotalCount { get; private set; } = 0;
        //�ظ���������
        public static int DuplicateSceneCount { get; set; } = 0;
        #endregion

        #region �ڲ���������
        //��ӳ���
        public static void AddScene(Scene scene)
        {
            //��ӵ���ǰʹ�õĳ������ݴ洢
            Storage[scene.ID] = scene;
            //��Ӽ���
            TotalCount++;

            //��ӵ������������ݴ洢
            if (!AllScenes.ContainsKey(scene.ID))
            {
                //��ID�����ڣ��������б�
                AllScenes[scene.ID] = new Dictionary<string, Scene>();
            }
            else
            {
                //��ID�Ѵ��ڣ����Ӽ���
                DuplicateSceneCount++;
            }
            string id = AllScenes[scene.ID].Count.ToString();
            AllScenes[scene.ID][id] = scene;

            //�趨�������Ӵ洢
            SceneConnections[scene] = scene.Connections;
            //�趨������������
            Index_ParentScene[scene] = scene.ChildScenes;
        }
        //���ó���
        public static void SetScene(string sceneID, Scene scene)
        {
            //���õ���ǰʹ�õĳ������ݴ洢
            Storage[sceneID] = scene;
        }
        //��ʼ��
        public static void Init()
        {
            //������ǰʹ�õĳ�������
            foreach (Scene scene in Storage.Values)
            {
                //��鳡������
                if (!string.IsNullOrEmpty(scene.ParentSceneID))
                {
                    //����Ϊ�գ����ȡ����
                    if (Storage.TryGetValue(scene.ParentSceneID, out Scene parentScene))
                    {
                        //�����ڣ�ָ������
                        scene.ParentScene = parentScene;
                        //��������ӵ��������ӳ����б�
                        parentScene.ChildScenes.Add(scene);
                    }
                    else
                    {
                        //�������ڣ���Ϸ�ڱ���
                        GameHistory.LogError($" {scene.ID} ���� {scene.ParentSceneID} �����ڣ�����Ϊ����֮һ��ʼ������");
                    }
                }
                else
                {
                    //��Ϊ�գ�����ӵ����������ݴ洢
                    RootScenes[scene.ID] = scene;
                }

                //�����ó�����ͼ�д洢��·������
                foreach (ScenePath path in scene.PathsInScene)
                {
                    //���ȳ��Ի�ȡ����A��B
                    if (Storage.TryGetValue(path.SceneAID, out Scene sceneA) && Storage.TryGetValue(path.SceneBID, out Scene sceneB))
                    {
                        //��ȡ�ɹ����ٳ��Ի�ȡA��B����������
                        if (SceneConnections.TryGetValue(sceneA, out Dictionary<Scene, int> connectionsOfA) && SceneConnections.TryGetValue(sceneB, out Dictionary<Scene, int> connectionsOfB))
                        {
                            //�������������ݣ�������ӶԷ�
                            connectionsOfA[sceneB] = path.Distance;
                            connectionsOfB[sceneA] = path.Distance;
                        }
                        else
                        {
                            //���������������ݣ�����Ϸ�ڱ���
                            GameHistory.LogError($" {sceneA.ID} �� {sceneB.ID} ����ʧ�ܣ�����Ϊ����֮һ��ʼ������");
                        }
                    }
                    else
                    {
                        //����ȡʧ�ܣ���Ϸ�ڱ���
                        GameHistory.LogError($"{path}����ʧ�ܣ�����Ϊ����֮һ�����ڡ�");
                    }
                }
            }
        }
        #endregion

        #region �ⲿ��������
        //��ȡ����
        public static Scene GetScene(string sceneID)
        {
            if (Storage.ContainsKey(sceneID))
            {
                return Storage[sceneID];
            }
            else
            {
                return null;
            }
        }
        //���Ի�ȡ����
        public static bool TryGetScene(string sceneID, out Scene scene)
        {
            if (Storage.ContainsKey(sceneID))
            {
                scene = Storage[sceneID];
                return true;
            }
            else
            {
                scene = null;
                return false;
            }
        }
        //��ѯ�����Ƿ����
        public static bool HasScene(string sceneID)
        {
            return AllScenes.ContainsKey(sceneID);
        }
        //��ȡ���п���·��
        public static List<Queue<Scene>> GetAllPaths(Scene start, Scene end)
        {
            //�������ؽ��
            List<Queue<Scene>> result = new();
            //׷���ѷ��ʳ�����ֹѭ��
            HashSet<Scene> visited = new();
            //������ǰ·������
            Queue<Scene> currentPath = new();

            //�������������ڵݹ����·��
            void DFS(Scene current)
            {
                //����ǰ�������Ϊ�ѷ��ʲ����뵱ǰ·��
                visited.Add(current);
                currentPath.Enqueue(current);

                //����Ƿ񵽴��յ�
                if (current == end)
                {
                    //�������յ㣬�򽫵�ǰ·��������
                    result.Add(new Queue<Scene>(currentPath));
                }
                else if (SceneConnections.ContainsKey(current))
                {
                    //��δ�����յ㣬�������ǰ����������
                    foreach (Scene neighbor in SceneConnections[current].Keys)
                    {
                        //��δ���ʹ�����ݹ����·��
                        if (!visited.Contains(neighbor))
                        {
                            DFS(neighbor);
                        }
                    }
                }

                //����
                visited.Remove(current);
                currentPath.Dequeue();
            }

            //��ʼ����·��
            DFS(start);
            //���ؽ��
            return result;
        }
        //��ȡ���·��
        public static Queue<Scene> GetShorestPath(Scene start, Scene end)
        {
            //�洢��ÿ����������̾���
            Dictionary<Scene, int> distances = new();
            //�洢��ÿ��������ǰ��
            Dictionary<Scene, Scene> predecessors = new();
            //���ȶ��У�����������
            List<(Scene scene, int distance)> priorityQueue = new();
            //�Է��ʵĳ����ļ���
            HashSet<Scene> visited = new();

            //��ʼ������
            distances[start] = 0;
            priorityQueue.Add((start, 0));

            //��ʼ����
            while (priorityQueue.Count > 0)
            {
                //ȡ��������С�ĳ���
                priorityQueue.Sort((a, b) => a.distance.CompareTo(b.distance));
                Scene current = priorityQueue[0].scene;
                int currentDistance = priorityQueue[0].distance;
                priorityQueue.RemoveAt(0);

                //����Ƿ񵽴��յ�
                if (current == end)
                {
                    //�������յ㣬�����·��
                    Queue<Scene> path = new();
                    Scene scene = end;
                    while (scene != null)
                    {
                        path.Enqueue(scene);
                        predecessors.TryGetValue(scene, out scene);
                    }
                    //��ת������·��
                    return new Queue<Scene>(path.Reverse());
                }

                //����Ƿ��ѷ��ʹ�
                if (visited.Contains(current))
                {
                    continue;
                }
                visited.Add(current);

                //������������
                if (SceneConnections.TryGetValue(current, out Dictionary<Scene, int> connections))
                {
                    foreach (KeyValuePair<Scene, int> connection in connections)
                    {
                        Scene neighbor = connection.Key;
                        int distance = connection.Value;

                        //����Ƿ��ѷ��ʹ�
                        if (visited.Contains(neighbor))
                        {
                            continue;
                        }

                        //�����¾���
                        int newDistance = currentDistance + distance;

                        //���¾���
                        if (!distances.ContainsKey(neighbor) || newDistance < distances[neighbor])
                        {
                            distances[neighbor] = newDistance;
                            predecessors[neighbor] = current;
                            priorityQueue.Add((neighbor, newDistance));
                        }
                    }
                }
            }

            //��δ�ҵ�·�����򷵻ؿն���
            return new Queue<Scene>();
        }
        //����·������
        public static int CalculatePathLength(Queue<Scene> path)
        {
            //��⴫��·��
            if (path == null || path.Count < 2)
            {
                return 0;
            }

            //����·������
            int totalLength = 0;
            Scene[] scenes = path.ToArray();

            //����·���е����ڳ�����
            for (int i = 0; i < scenes.Length - 1; i++)
            {
                //��ȡ��ǰ��������һ����
                Scene current = scenes[i];
                Scene next = scenes[i + 1];

                //��鳡�������Ƿ����
                if (!SceneConnections.ContainsKey(current) || !SceneConnections[current].ContainsKey(next))
                {
                    //�������ڣ��򷵻�0
                    return 0;
                }

                //�ۼӾ���
                totalLength += SceneConnections[current][next];
            }

            //����·������
            return totalLength;
        }
        #endregion
    }
}
