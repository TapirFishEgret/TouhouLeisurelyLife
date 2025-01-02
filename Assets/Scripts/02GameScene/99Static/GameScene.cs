using System.Collections.Generic;
using System.Linq;
using THLL.BaseSystem;
using UnityEngine;

namespace THLL.SceneSystem
{
    public static class GameScene
    {
        #region 游戏资源
        //默认背景图
        public static Sprite DefaultBackground => GameAssetsManager.Instance.DefaultBackground;
        #endregion

        #region 数据存储
        //当前使用的场景数据存储
        public static Dictionary<string, Scene> Storage { get; private set; } = new();
        //根场景数据存储
        public static Dictionary<string, Scene> RootScenes { get; private set; } = new();
        //完整场景数据存储
        private static Dictionary<string, Dictionary<string, Scene>> AllScenes { get; set; } = new();
        //场景连接存储
        private static Dictionary<Scene, Dictionary<Scene, int>> SceneConnections { get; set; } = new();
        //父级场景索引
        private static Dictionary<Scene, HashSet<Scene>> Index_ParentScene { get; set; } = new();
        //总计数
        public static int TotalCount { get; private set; } = 0;
        //重复场景计数
        public static int DuplicateSceneCount { get; set; } = 0;
        #endregion

        #region 内部操作方法
        //添加场景
        public static void AddScene(Scene scene)
        {
            //添加到当前使用的场景数据存储
            Storage[scene.ID] = scene;
            //添加计数
            TotalCount++;

            //添加到完整场景数据存储
            if (!AllScenes.ContainsKey(scene.ID))
            {
                //若ID不存在，创建新列表
                AllScenes[scene.ID] = new Dictionary<string, Scene>();
            }
            else
            {
                //若ID已存在，增加计数
                DuplicateSceneCount++;
            }
            string id = AllScenes[scene.ID].Count.ToString();
            AllScenes[scene.ID][id] = scene;

            //设定场景连接存储
            SceneConnections[scene] = scene.Connections;
            //设定父级场景索引
            Index_ParentScene[scene] = scene.ChildScenes;
        }
        //设置场景
        public static void SetScene(string sceneID, Scene scene)
        {
            //设置到当前使用的场景数据存储
            Storage[sceneID] = scene;
        }
        //初始化
        public static void Init()
        {
            //遍历当前使用的场景数据
            foreach (Scene scene in Storage.Values)
            {
                //检查场景父级
                if (!string.IsNullOrEmpty(scene.ParentSceneID))
                {
                    //若不为空，则获取父级
                    if (Storage.TryGetValue(scene.ParentSceneID, out Scene parentScene))
                    {
                        //若存在，指定父级
                        scene.ParentScene = parentScene;
                        //将自身添加到父级的子场景列表
                        parentScene.ChildScenes.Add(scene);
                    }
                    else
                    {
                        //若不存在，游戏内报错
                        GameHistory.LogError($" {scene.ID} 父级 {scene.ParentSceneID} 不存在，疑似为场景之一初始化出错。");
                    }
                }
                else
                {
                    //若为空，则添加到根场景数据存储
                    RootScenes[scene.ID] = scene;
                }

                //遍历该场景地图中存储的路径数据
                foreach (ScenePath path in scene.PathsInScene)
                {
                    //首先尝试获取场景A与B
                    if (Storage.TryGetValue(path.SceneAID, out Scene sceneA) && Storage.TryGetValue(path.SceneBID, out Scene sceneB))
                    {
                        //获取成功，再尝试获取A与B的连接数据
                        if (SceneConnections.TryGetValue(sceneA, out Dictionary<Scene, int> connectionsOfA) && SceneConnections.TryGetValue(sceneB, out Dictionary<Scene, int> connectionsOfB))
                        {
                            //若存在连接数据，则互相添加对方
                            connectionsOfA[sceneB] = path.Distance;
                            connectionsOfB[sceneA] = path.Distance;
                        }
                        else
                        {
                            //若不存在连接数据，则游戏内报错
                            GameHistory.LogError($" {sceneA.ID} 与 {sceneB.ID} 连接失败，疑似为场景之一初始化出错。");
                        }
                    }
                    else
                    {
                        //若获取失败，游戏内报错
                        GameHistory.LogError($"{path}生成失败，疑似为场景之一不存在。");
                    }
                }
            }
        }
        #endregion

        #region 外部操作方法
        //获取场景
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
        //尝试获取场景
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
        //查询场景是否存在
        public static bool HasScene(string sceneID)
        {
            return AllScenes.ContainsKey(sceneID);
        }
        //获取所有可能路径
        public static List<Queue<Scene>> GetAllPaths(Scene start, Scene end)
        {
            //创建返回结果
            List<Queue<Scene>> result = new();
            //追踪已访问场景防止循环
            HashSet<Scene> visited = new();
            //创建当前路径队列
            Queue<Scene> currentPath = new();

            //匿名方法，用于递归查找路径
            void DFS(Scene current)
            {
                //将当前场景标记为已访问并加入当前路径
                visited.Add(current);
                currentPath.Enqueue(current);

                //检测是否到达终点
                if (current == end)
                {
                    //若到达终点，则将当前路径加入结果
                    result.Add(new Queue<Scene>(currentPath));
                }
                else if (SceneConnections.ContainsKey(current))
                {
                    //若未到达终点，则遍历当前场景的连接
                    foreach (Scene neighbor in SceneConnections[current].Keys)
                    {
                        //若未访问过，则递归查找路径
                        if (!visited.Contains(neighbor))
                        {
                            DFS(neighbor);
                        }
                    }
                }

                //回溯
                visited.Remove(current);
                currentPath.Dequeue();
            }

            //开始查找路径
            DFS(start);
            //返回结果
            return result;
        }
        //获取最短路径
        public static Queue<Scene> GetShorestPath(Scene start, Scene end)
        {
            //存储到每个场景的最短距离
            Dictionary<Scene, int> distances = new();
            //存储到每个场景的前驱
            Dictionary<Scene, Scene> predecessors = new();
            //优先队列，按距离排序
            List<(Scene scene, int distance)> priorityQueue = new();
            //以访问的场景的集合
            HashSet<Scene> visited = new();

            //初始化距离
            distances[start] = 0;
            priorityQueue.Add((start, 0));

            //开始搜索
            while (priorityQueue.Count > 0)
            {
                //取出距离最小的场景
                priorityQueue.Sort((a, b) => a.distance.CompareTo(b.distance));
                Scene current = priorityQueue[0].scene;
                int currentDistance = priorityQueue[0].distance;
                priorityQueue.RemoveAt(0);

                //检测是否到达终点
                if (current == end)
                {
                    //若到达终点，则回溯路径
                    Queue<Scene> path = new();
                    Scene scene = end;
                    while (scene != null)
                    {
                        path.Enqueue(scene);
                        predecessors.TryGetValue(scene, out scene);
                    }
                    //翻转并返回路径
                    return new Queue<Scene>(path.Reverse());
                }

                //检测是否已访问过
                if (visited.Contains(current))
                {
                    continue;
                }
                visited.Add(current);

                //继续向下搜索
                if (SceneConnections.TryGetValue(current, out Dictionary<Scene, int> connections))
                {
                    foreach (KeyValuePair<Scene, int> connection in connections)
                    {
                        Scene neighbor = connection.Key;
                        int distance = connection.Value;

                        //检测是否已访问过
                        if (visited.Contains(neighbor))
                        {
                            continue;
                        }

                        //计算新距离
                        int newDistance = currentDistance + distance;

                        //更新距离
                        if (!distances.ContainsKey(neighbor) || newDistance < distances[neighbor])
                        {
                            distances[neighbor] = newDistance;
                            predecessors[neighbor] = current;
                            priorityQueue.Add((neighbor, newDistance));
                        }
                    }
                }
            }

            //若未找到路径，则返回空队列
            return new Queue<Scene>();
        }
        //计算路径长度
        public static int CalculatePathLength(Queue<Scene> path)
        {
            //检测传入路径
            if (path == null || path.Count < 2)
            {
                return 0;
            }

            //计算路径长度
            int totalLength = 0;
            Scene[] scenes = path.ToArray();

            //遍历路径中的相邻场景对
            for (int i = 0; i < scenes.Length - 1; i++)
            {
                //获取当前场景与下一场景
                Scene current = scenes[i];
                Scene next = scenes[i + 1];

                //检查场景连接是否存在
                if (!SceneConnections.ContainsKey(current) || !SceneConnections[current].ContainsKey(next))
                {
                    //若不存在，则返回0
                    return 0;
                }

                //累加距离
                totalLength += SceneConnections[current][next];
            }

            //返回路径长度
            return totalLength;
        }
        #endregion
    }
}
