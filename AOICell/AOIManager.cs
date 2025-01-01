using System;
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
namespace AOICellSpace
{
    public class AOIConfig
    {
        public string mapName = ""; // 地图名称

        public int cellSize = 20; // 单元格大小
        public int initCount = 200; // 初始化数量

        // 各个更新的数量
        public int updateEnterAmount = 10;
        public int updateMoveAmount = 50;
        public int updateExitAmount = 10;
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

        public Action<AOIEntity, UpdateItem> OnEntityCellViewChange; // 实体视野变化回调
        public Action<AOICell, UpdateItem> OnCellViewEntityOperationCombination; // 宫格内视野中实体变化操作合并的回调

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
            AOIEntity aoiEntity = new AOIEntity(entityID, this);
            aoiEntity.UpdatePosition(x, z, EntityOperationEnum.TransferEnter);
            aoiEntityList.Add(aoiEntity);
            return aoiEntity;
        }
        /// <summary>
        /// 更新实体Entity位置
        /// </summary>
        public void UpdateEntityPosition(AOIEntity aoiEntity, float x, float z)
        {
            aoiEntity.UpdatePosition(x, z);
        }
        /// <summary>
        /// 实体Entity离开AOI宫格
        /// </summary>
        public void ExitCell(AOIEntity aoiEntity)
        {
            if (aoiCellDic.TryGetValue(aoiEntity.cellKey, out AOICell cell))
            {
                cell.ExitCell(aoiEntity);
            }
            else
            {
                this.LogYellow($"aoiCellDic cannot find cell: {aoiEntity.cellKey}!");
            }

            if (!aoiEntityList.Remove(aoiEntity))
            {
                this.LogYellow($"aoiEntityList cannot find entity: {aoiEntity.entityID}");
            }
        }

        /// <summary>
        /// 驱动整体AOI更新
        /// </summary>
        public void CalculateAOIUpdate()
        {
            // 计算实体视野变化
            for (int i = 0; i < aoiEntityList.Count; i++)
            {
                aoiEntityList[i].CalculateEntityCellViewChange();
            }

            // 计算宫格内部视野中实体变化操作合并
            foreach (var item in aoiCellDic)
            {
                AOICell cell = item.Value;

                if (cell.aOIEntitiesEnterSet.Count > 0)
                {
                    cell.aOIEntitiesSet.UnionWith(cell.aOIEntitiesEnterSet);
                    cell.aOIEntitiesEnterSet.Clear();
                }

                cell.CalculateCellViewEntityOperationCombination();
            }
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

            // 上移操作
            {
                cell.UpCellArr = new AOICell[12];

                // 要离开(视野退出)的3个宫格(Exit)
                cell.UpCellArr[0] = aoiCellDic[$"{xIndex - 1}_{zIndex - 2}"];
                cell.UpCellArr[1] = aoiCellDic[$"{xIndex}_{zIndex - 2}"];
                cell.UpCellArr[2] = aoiCellDic[$"{xIndex + 1}_{zIndex - 2}"];

                // 要进入(视野进入)的3个宫格(Enter)
                cell.UpCellArr[3] = aoiCellDic[$"{xIndex - 1}_{zIndex + 1}"];
                cell.UpCellArr[4] = aoiCellDic[$"{xIndex}_{zIndex + 1}"];
                cell.UpCellArr[5] = aoiCellDic[$"{xIndex + 1}_{zIndex + 1}"];

                // 在中间的6个宫格内移动: 不会有视野变化, 只有实体的移动
                cell.UpCellArr[6] = aoiCellDic[$"{xIndex - 1}_{zIndex}"];
                cell.UpCellArr[7] = aoiCellDic[$"{xIndex}_{zIndex}"];
                cell.UpCellArr[8] = aoiCellDic[$"{xIndex + 1}_{zIndex}"];
                cell.UpCellArr[9] = aoiCellDic[$"{xIndex - 1}_{zIndex - 1}"];
                cell.UpCellArr[10] = aoiCellDic[$"{xIndex}_{zIndex - 1}"];
                cell.UpCellArr[11] = aoiCellDic[$"{xIndex + 1}_{zIndex - 1}"];
            }

            //下移操作：3:exit，3:enter，6:move
            {
                cell.DownCellArr = new AOICell[12];

                cell.DownCellArr[0] = aoiCellDic[$"{xIndex - 1},{zIndex + 2}"];
                cell.DownCellArr[1] = aoiCellDic[$"{xIndex},{zIndex + 2}"];
                cell.DownCellArr[2] = aoiCellDic[$"{xIndex + 1},{zIndex + 2}"];

                cell.DownCellArr[3] = aoiCellDic[$"{xIndex - 1},{zIndex - 1}"];
                cell.DownCellArr[4] = aoiCellDic[$"{xIndex},{zIndex - 1}"];
                cell.DownCellArr[5] = aoiCellDic[$"{xIndex + 1},{zIndex - 1}"];

                cell.DownCellArr[6] = aoiCellDic[$"{xIndex - 1},{zIndex + 1}"];
                cell.DownCellArr[7] = aoiCellDic[$"{xIndex},{zIndex + 1}"];
                cell.DownCellArr[8] = aoiCellDic[$"{xIndex + 1},{zIndex + 1}"];
                cell.DownCellArr[9] = aoiCellDic[$"{xIndex - 1},{zIndex}"];
                cell.DownCellArr[10] = aoiCellDic[$"{xIndex},{zIndex}"];
                cell.DownCellArr[11] = aoiCellDic[$"{xIndex + 1},{zIndex}"];
            }

            //左移操作：3:exit，3:enter，6:move
            {
                cell.LeftCellArr = new AOICell[12];

                cell.LeftCellArr[0] = aoiCellDic[$"{xIndex + 2},{zIndex + 1}"];
                cell.LeftCellArr[1] = aoiCellDic[$"{xIndex + 2},{zIndex}"];
                cell.LeftCellArr[2] = aoiCellDic[$"{xIndex + 2},{zIndex - 1}"];

                cell.LeftCellArr[3] = aoiCellDic[$"{xIndex - 1},{zIndex + 1}"];
                cell.LeftCellArr[4] = aoiCellDic[$"{xIndex - 1},{zIndex}"];
                cell.LeftCellArr[5] = aoiCellDic[$"{xIndex - 1},{zIndex - 1}"];

                cell.LeftCellArr[6] = aoiCellDic[$"{xIndex},{zIndex + 1}"];
                cell.LeftCellArr[7] = aoiCellDic[$"{xIndex},{zIndex}"];
                cell.LeftCellArr[8] = aoiCellDic[$"{xIndex},{zIndex - 1}"];
                cell.LeftCellArr[9] = aoiCellDic[$"{xIndex + 1},{zIndex + 1}"];
                cell.LeftCellArr[10] = aoiCellDic[$"{xIndex + 1},{zIndex}"];
                cell.LeftCellArr[11] = aoiCellDic[$"{xIndex + 1},{zIndex - 1}"];
            }

            //右移操作：3:exit，3:enter，6:move
            {
                cell.RightCellArr = new AOICell[12];

                cell.RightCellArr[0] = aoiCellDic[$"{xIndex - 2},{zIndex + 1}"];
                cell.RightCellArr[1] = aoiCellDic[$"{xIndex - 2},{zIndex}"];
                cell.RightCellArr[2] = aoiCellDic[$"{xIndex - 2},{zIndex - 1}"];

                cell.RightCellArr[3] = aoiCellDic[$"{xIndex + 1},{zIndex + 1}"];
                cell.RightCellArr[4] = aoiCellDic[$"{xIndex + 1},{zIndex}"];
                cell.RightCellArr[5] = aoiCellDic[$"{xIndex + 1},{zIndex - 1}"];

                cell.RightCellArr[6] = aoiCellDic[$"{xIndex - 1},{zIndex + 1}"];
                cell.RightCellArr[7] = aoiCellDic[$"{xIndex - 1},{zIndex}"];
                cell.RightCellArr[8] = aoiCellDic[$"{xIndex - 1},{zIndex - 1}"];
                cell.RightCellArr[9] = aoiCellDic[$"{xIndex},{zIndex + 1}"];
                cell.RightCellArr[10] = aoiCellDic[$"{xIndex},{zIndex}"];
                cell.RightCellArr[11] = aoiCellDic[$"{xIndex},{zIndex - 1}"];
            }

            //左上操作：5:exit, 5:enter, 4:move
            {
                cell.upLeftCellArr = new AOICell[14];

                cell.upLeftCellArr[0] = aoiCellDic[$"{xIndex},{zIndex - 2}"];
                cell.upLeftCellArr[1] = aoiCellDic[$"{xIndex + 1},{zIndex - 2}"];
                cell.upLeftCellArr[2] = aoiCellDic[$"{xIndex + 2},{zIndex - 2}"];
                cell.upLeftCellArr[3] = aoiCellDic[$"{xIndex + 2},{zIndex - 1}"];
                cell.upLeftCellArr[4] = aoiCellDic[$"{xIndex + 2},{zIndex}"];

                cell.upLeftCellArr[5] = aoiCellDic[$"{xIndex - 1},{zIndex - 1}"];
                cell.upLeftCellArr[6] = aoiCellDic[$"{xIndex - 1},{zIndex}"];
                cell.upLeftCellArr[7] = aoiCellDic[$"{xIndex - 1},{zIndex + 1}"];
                cell.upLeftCellArr[8] = aoiCellDic[$"{xIndex},{zIndex + 1}"];
                cell.upLeftCellArr[9] = aoiCellDic[$"{xIndex + 1},{zIndex + 1}"];

                cell.upLeftCellArr[10] = aoiCellDic[$"{xIndex},{zIndex}"];
                cell.upLeftCellArr[11] = aoiCellDic[$"{xIndex + 1},{zIndex}"];
                cell.upLeftCellArr[12] = aoiCellDic[$"{xIndex},{zIndex - 1}"];
                cell.upLeftCellArr[13] = aoiCellDic[$"{xIndex + 1},{zIndex - 1}"];
            }

            //右上操作：5:exit, 5:enter, 4:move
            {
                cell.upRightCellArr = new AOICell[14];

                cell.upRightCellArr[0] = aoiCellDic[$"{xIndex - 2},{zIndex}"];
                cell.upRightCellArr[1] = aoiCellDic[$"{xIndex - 2},{zIndex - 1}"];
                cell.upRightCellArr[2] = aoiCellDic[$"{xIndex - 2},{zIndex - 2}"];
                cell.upRightCellArr[3] = aoiCellDic[$"{xIndex - 1},{zIndex - 2}"];
                cell.upRightCellArr[4] = aoiCellDic[$"{xIndex},{zIndex - 2}"];

                cell.upRightCellArr[5] = aoiCellDic[$"{xIndex - 1},{zIndex + 1}"];
                cell.upRightCellArr[6] = aoiCellDic[$"{xIndex},{zIndex + 1}"];
                cell.upRightCellArr[7] = aoiCellDic[$"{xIndex + 1},{zIndex + 1}"];
                cell.upRightCellArr[8] = aoiCellDic[$"{xIndex + 1},{zIndex}"];
                cell.upRightCellArr[9] = aoiCellDic[$"{xIndex + 1},{zIndex - 1}"];

                cell.upRightCellArr[10] = aoiCellDic[$"{xIndex - 1},{zIndex}"];
                cell.upRightCellArr[11] = aoiCellDic[$"{xIndex},{zIndex}"];
                cell.upRightCellArr[12] = aoiCellDic[$"{xIndex - 1},{zIndex - 1}"];
                cell.upRightCellArr[13] = aoiCellDic[$"{xIndex},{zIndex - 1}"];
            }

            //左下操作：5:exit, 5:enter, 4:move
            {
                cell.downLeftCellArr = new AOICell[14];

                cell.downLeftCellArr[0] = aoiCellDic[$"{xIndex},{zIndex + 2}"];
                cell.downLeftCellArr[1] = aoiCellDic[$"{xIndex + 1},{zIndex + 2}"];
                cell.downLeftCellArr[2] = aoiCellDic[$"{xIndex + 2},{zIndex + 2}"];
                cell.downLeftCellArr[3] = aoiCellDic[$"{xIndex + 2},{zIndex + 1}"];
                cell.downLeftCellArr[4] = aoiCellDic[$"{xIndex + 2},{zIndex}"];

                cell.downLeftCellArr[5] = aoiCellDic[$"{xIndex - 1},{zIndex + 1}"];
                cell.downLeftCellArr[6] = aoiCellDic[$"{xIndex - 1},{zIndex}"];
                cell.downLeftCellArr[7] = aoiCellDic[$"{xIndex - 1},{zIndex - 1}"];
                cell.downLeftCellArr[8] = aoiCellDic[$"{xIndex},{zIndex - 1}"];
                cell.downLeftCellArr[9] = aoiCellDic[$"{xIndex + 1},{zIndex - 1}"];

                cell.downLeftCellArr[10] = aoiCellDic[$"{xIndex},{zIndex + 1}"];
                cell.downLeftCellArr[11] = aoiCellDic[$"{xIndex + 1},{zIndex + 1}"];
                cell.downLeftCellArr[12] = aoiCellDic[$"{xIndex},{zIndex}"];
                cell.downLeftCellArr[13] = aoiCellDic[$"{xIndex + 1},{zIndex}"];
            }

            //右下操作：5:exit, 5:enter, 4:move
            {
                cell.downRightCellArr = new AOICell[14];

                cell.downRightCellArr[0] = aoiCellDic[$"{xIndex - 2},{zIndex + 2}"];
                cell.downRightCellArr[1] = aoiCellDic[$"{xIndex - 2},{zIndex + 1}"];
                cell.downRightCellArr[2] = aoiCellDic[$"{xIndex - 2},{zIndex}"];
                cell.downRightCellArr[3] = aoiCellDic[$"{xIndex - 1},{zIndex + 2}"];
                cell.downRightCellArr[4] = aoiCellDic[$"{xIndex},{zIndex + 2}"];

                cell.downRightCellArr[5] = aoiCellDic[$"{xIndex - 1},{zIndex - 1}"];
                cell.downRightCellArr[6] = aoiCellDic[$"{xIndex},{zIndex - 1}"];
                cell.downRightCellArr[7] = aoiCellDic[$"{xIndex + 1},{zIndex - 1}"];
                cell.downRightCellArr[8] = aoiCellDic[$"{xIndex + 1},{zIndex}"];
                cell.downRightCellArr[9] = aoiCellDic[$"{xIndex + 1},{zIndex + 1}"];

                cell.downRightCellArr[10] = aoiCellDic[$"{xIndex - 1},{zIndex + 1}"];
                cell.downRightCellArr[11] = aoiCellDic[$"{xIndex},{zIndex + 1}"];
                cell.downRightCellArr[12] = aoiCellDic[$"{xIndex - 1},{zIndex}"];
                cell.downRightCellArr[13] = aoiCellDic[$"{xIndex},{zIndex}"];
            }


            cell.isCalculateBoundaries = true;
        }
    }
}
