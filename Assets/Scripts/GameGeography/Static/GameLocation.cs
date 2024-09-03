using System.Collections.Generic;

namespace THLL.GeographySystem
{
    public static class GameLocation
    {
        #region 数据
        //游戏内所有地点实例数据库
        public static LocationDb LocationDb { get; } = new LocationDb();
        //游戏内所有地点连接数据
        public static Dictionary<Location, Dictionary<Location, int>> LocationConnectionDb { get; } = new Dictionary<Location, Dictionary<Location, int>>();
        #endregion

        #region 方法
        //初始化
        public static void Init()
        {
            //完成每个实例的初始化方法
            foreach (Location location in LocationDb.Datas)
            {
                location.Init(LocationDb, LocationConnectionDb);
            }
        }
        //寻路，深度优先搜索获取所有路径
        public static List<List<Location>> FindAllPaths(Location start, Location end)
        {
            //创建深度优先搜索结果、过程数据
            List<List<Location>> allPaths = new();
            List<Location> currentPath = new();
            HashSet<Location> visited = new();

            //方法内部方法，优先深度搜索
            void DFS(Location current)
            {
                //判断是否已经查询过此节点
                if (visited.Contains(current))
                {
                    //若已经查询，则返回
                    return;
                }

                //将节点加入查询节点与路径过程数据中
                visited.Add(current);
                currentPath.Add(current);

                //检测是否到达终点
                if (current.Equals(end))
                {
                    //若是，将当前结果作为完整路径存入最终结果中
                    allPaths.Add(new List<Location>(currentPath));
                }
                //若不是，检查是否仍有下一个可行地点
                else if (LocationConnectionDb.ContainsKey(current))
                {
                    //若有，遍历
                    foreach (Location neighbor in LocationConnectionDb[current].Keys)
                    {
                        //继续搜索
                        DFS(neighbor);
                    }
                }

                //一条路径结束后，移除最后一个节点，并探索下一个路径
                currentPath.RemoveAt(currentPath.Count - 1);
                visited.Remove(current);
            }

            //开始查询
            DFS(start);
            //返回所有路径，若无可用路径，则返回空
            if (allPaths.Count > 0)
            {
                return allPaths;
            }
            return null;
        }
        //寻路，使用Dijkstra算法获取最短路径
        public static List<Location> FindShortestPath(Location start, Location end)
        {
            //存储从起点开始到每个节点的最短距离
            Dictionary<Location, int> distances = new();
            //前驱节点字典，用于重建路径
            Dictionary<Location, Location> previous = new();
            //优先队列，用于按最短距离有点处理节点，存储节点及其对应距离
            SortedSet<(int, Location)> priorityQueue = new(Comparer<(int, Location)>.Create((a, b) =>
            {
                //比较器，首先比较时间耗费
                int result = a.Item1.CompareTo(b.Item1);
                //若耗时相同，则比较哈希码，以保证元素唯一
                return result == 0 ? a.Item2.GetHashCode().CompareTo(b.Item2.GetHashCode()) : result;
            }));

            //初始化所有节点和前驱节点
            foreach (var loc in LocationConnectionDb.Keys)
            {
                //初始距离为无限大
                distances[loc] = int.MaxValue;
                //前驱节点为空
                previous[loc] = null;
                //将节点放入优先队列
                priorityQueue.Add((int.MaxValue, loc));
            }

            //将起点距离设置为0，并更新优先队列
            distances[start] = 0;
            priorityQueue.Add((0, start));

            //对优先队列进行查询
            while (priorityQueue.Count > 0)
            {
                //首先去除队列中距离最小的节点
                var (currentDistance, current) = priorityQueue.Min;
                priorityQueue.Remove(priorityQueue.Min);

                //判断当前节点是否为终点
                if (current.Equals(end))
                {
                    //若是，则新建路径
                    List<Location> path = new();
                    //从终点开始
                    while (current != null)
                    {
                        //一步步向最终路径的0号位插入
                        path.Insert(0, current);
                        //并一步步回溯，从终点向起点构建
                        current = previous[current];
                    }
                    //构建完成后返回
                    return path;
                }

                //判断当前节点是否有相邻节点
                if (!LocationConnectionDb.ContainsKey(current))
                {
                    //若没有，跳过
                    continue;
                }

                //若有，对相邻节点进行计算
                foreach (Location neighbor in LocationConnectionDb[current].Keys)
                {
                    //计算从起点到该节点的距离
                    int newDist = currentDistance + LocationConnectionDb[current][neighbor];
                    //检测距离长短
                    if (newDist < distances[neighbor])
                    {
                        //若新距离小于旧距离信息，则从优先队列中移除旧的距离
                        priorityQueue.Remove((distances[neighbor], neighbor));
                        //同时更新距离
                        distances[neighbor] = newDist;
                        //更新前驱节点
                        previous[neighbor] = current;
                        //将新的距离加入优先队列
                        priorityQueue.Add((newDist, neighbor));
                    }
                }
            }

            //若没有任何路径被查询到，返回空
            return null;
        }
        #endregion
    }
}
