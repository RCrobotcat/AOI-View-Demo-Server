using System.Collections.Generic;

/*
 * 特点:
 * 1. 宫格会根据场景地图自适应生成, 角色能走道德地方才会生成宫格
 * 2. 可以确保服务器内的角色相互可见或者相互不可见
 * 3. 边界移动了，宫格会自动更新(即视野会自动更新)
 * 4. 宫格的计算结果会合并以及复用，以减少计算开销 => 因为大部分时候角色没有穿越宫格边界，宫格内部的视野变化情况是不变的，这样可以优化序列数据量
 * 5. 会剔除无客户端关注的服务器AOI实体的数据变化 => 例如服务端驱动的AI怪物的服务端AOI实体变化，客户端不需要知道
 */

// AOI管理器
namespace AOICell
{
    public class AOIConfig
    {
        public string mapName = ""; // 地图名称

        public int cellSize = 20; // 单元格大小
        public int initCount = 200; // 初始化数量
    }

    public class AOIManager
    {
        private AOIConfig aoiCfg; // AOI配置
        public AOIConfig AOICfg { get { return aoiCfg; } private set => aoiCfg = value; }

        public string managerName; // 管理器名称
        private int cellSize; // 地图尺寸
        public int CellSize { get => cellSize; private set => cellSize = value; }

        private Dictionary<string, AOICell> aoiCellDic; // 所有的AOI宫格
        private List<AOIEntity> aoiEntityList; // 所有的AOI实体

        public AOIManager(AOIConfig config)
        {
            AOICfg = config;
            managerName = config.mapName;
            cellSize = config.cellSize;

            aoiCellDic = new Dictionary<string, AOICell>(config.initCount);
            aoiEntityList = new List<AOIEntity>();
        }

        /// <summary>
        /// 实体Entity进入AOI宫格
        /// 返回AOI实体
        /// </summary>
        /// <param name="entityID">实体ID</param>
        /// <param name="x">位置x</param>
        /// <param name="z">位置z</param>
        public AOIEntity EnterCell(uint entityID, float x, float z)
        {
            // TODO
            return null;
        }

        /// <summary>
        /// 更新实体Entity位置
        /// </summary>
        public void UpdateEntityPosition(AOIEntity aoiEntity, float x, float z)
        {
            // TODO
        }

        /// <summary>
        /// 实体Entity离开AOI宫格
        /// </summary>
        public void ExitCell(AOIEntity aoiEntity)
        {
            // TODO
        }

        /// <summary>
        /// 驱动整体AOI更新
        /// </summary>
        public void CalculateAOIUpdate()
        {

        }

        /// <summary>
        /// 移动穿越宫格(边界穿越)
        /// </summary>
        public void MoveCrossCell(AOIEntity aOIEntity)
        {
            AOICell cell = GetOrCreateCell(aOIEntity);
            if (!cell.isCalculateBoundaries)
            {
                CalculateCellBoundaries(cell);
            }

            cell.EnterCell(aOIEntity);
        }

        /// <summary>
        /// 在宫格内部移动
        /// </summary>
        public void MoveInsideCell(AOIEntity aOIEntity)
        {
            if (aoiCellDic.TryGetValue(aOIEntity.cellKey, out AOICell cell))
            {
                cell.MoveInsideCell(aOIEntity);
            }
            else
            {
                this.Error($"AOICell: {aOIEntity.cellKey} does not exist in aoiCellDic!");
            }
        }

        /// <summary>
        /// 获取或者创建宫格
        /// </summary>
        public AOICell GetOrCreateCell(AOIEntity aOIEntity)
        {
            AOICell cell;
            if (!aoiCellDic.TryGetValue(aOIEntity.cellKey, out cell))
            {
                cell = new AOICell(aOIEntity.xIndex, aOIEntity.zIndex, this);
                aoiCellDic.Add(aOIEntity.cellKey, cell);
            }

            return cell;
        }
        /// <summary>
        /// 计算宫格的边界
        /// 即周围的九个宫格
        /// </summary>
        void CalculateCellBoundaries(AOICell cell)
        {
            int xIndex = cell.xIndex;
            int zIndex = cell.zIndex;

            cell.aroundCells = new AOICell[9];

            int index = 0;
            for (int i = xIndex - 2; i <= xIndex + 2; i++)
            {
                for (int j = zIndex - 2; j <= zIndex + 2; j++)
                {
                    string key = $"{i}_{j}";
                    if (!aoiCellDic.TryGetValue(key, out AOICell ac))
                    {
                        ac = new AOICell(i, j, this);
                        aoiCellDic.Add(key, ac);
                    }

                    if (i > xIndex - 2
                        && i < xIndex + 2
                        && j > zIndex - 2
                        && j < zIndex + 2)
                    {
                        cell.aroundCells[index++] = ac;
                    }
                }
            }

            cell.isCalculateBoundaries = true;
        }
    }
}
