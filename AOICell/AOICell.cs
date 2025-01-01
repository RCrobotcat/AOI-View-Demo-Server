﻿using System.Collections.Generic;

// AOI单元(宫格)
namespace AOICellSpace
{
    public class AOICell
    {
        public int xIndex; // x索引
        public int zIndex; // z索引
        public AOIManager aoiManager;

        public AOICell[] aroundCells = null; // 周围的宫格
        public bool isCalculateBoundaries = false; // 是否已经计算了边界

        public AOICell[] UpCellArr = null; // 上边宫格
        public AOICell[] DownCellArr = null; // 下边宫格
        public AOICell[] LeftCellArr = null; // 左边宫格
        public AOICell[] RightCellArr = null; // 右边宫格
        public AOICell[] upLeftCellArr = null; // 左上宫格
        public AOICell[] upRightCellArr = null; // 右上宫格
        public AOICell[] downRightCellArr = null; // 右下宫格
        public AOICell[] downLeftCellArr = null; // 左下宫格

        public UpdateItem cellUpdateItem; // Cell所有的更新

        public HashSet<AOIEntity> aOIEntitiesSet = new HashSet<AOIEntity>(); // 当前宫格内的所有实体
        public HashSet<AOIEntity> aOIEntitiesEnterSet = new HashSet<AOIEntity>(); // 当前宫格内的所有新进入的实体(缓存)

        public AOICell(int xIndex, int zIndex, AOIManager aoiManager)
        {
            this.xIndex = xIndex;
            this.zIndex = zIndex;
            this.aoiManager = aoiManager;

            cellUpdateItem = new UpdateItem(aoiManager.AOICfg.updateEnterAmount,
                aoiManager.AOICfg.updateMoveAmount,
                aoiManager.AOICfg.updateExitAmount);
        }

        /// <summary>
        /// 进入宫格
        /// 可能是：传送进入宫格或者移动进入宫格
        /// </summary>
        public void EnterCell(AOIEntity aOIEntity)
        {
            if (!aOIEntitiesEnterSet.Add(aOIEntity))
            {
                this.Error($"AOICell: {aOIEntity.cellKey} EnterCell error! Entity: {aOIEntity.entityID} already enter cell!");
                return;
            }

            if (aOIEntity.EntityOperation == EntityOperationEnum.TransferEnter)
            {
                aOIEntity.AddAroundCellView(aroundCells);

                for (int i = 0; i < aroundCells.Length; i++)
                {
                    aroundCells[i].AddCellOperation(CellOperationEnum.EntityEnter, aOIEntity);
                }
            }
            else if (aOIEntity.EntityOperation == EntityOperationEnum.MoveCross)
            {
                switch (aOIEntity.MoveCrossCellDir)
                {
                    case MoveCrossCellDirEnum.Up:
                        break;
                    case MoveCrossCellDirEnum.Down:
                        break;
                    case MoveCrossCellDirEnum.Left:
                        break;
                    case MoveCrossCellDirEnum.Right:
                        break;
                    case MoveCrossCellDirEnum.LeftUp:
                        break;
                    case MoveCrossCellDirEnum.RightUp:
                        break;
                    case MoveCrossCellDirEnum.RightDown:
                        break;
                    case MoveCrossCellDirEnum.LeftDown:
                        break;
                    case MoveCrossCellDirEnum.None:
                        break;
                    default:
                        break;
                }
            }
            else
            {
                this.Error($"AOICell: {aOIEntity.cellKey} EnterCell error! EntityOperation: {aOIEntity.EntityOperation} error!");
            }
        }
        /// <summary>
        /// 在宫格内部移动
        /// </summary>
        public void MoveInsideCell(AOIEntity aOIEntity)
        {

        }

        /// <summary>
        /// 退出宫格
        /// </summary>
        public void ExitCell(AOIEntity aoiEntity)
        {
            for (int i = 0; i < aroundCells.Length; i++)
            {
                aroundCells[i].AddCellOperation(CellOperationEnum.EntityExit, aoiEntity);
            }
        }

        /// <summary>
        /// 写入AOICell操作
        /// 合并(叠加)所有相关操作
        /// </summary>
        /// <param name="aoiEntity">当前这个操作是哪个AOI实体写入的</param>
        public void AddCellOperation(CellOperationEnum cellOperation, AOIEntity aoiEntity)
        {
            switch (cellOperation)
            {
                case CellOperationEnum.EntityEnter:
                    cellUpdateItem.enterItemsList.Add(new EnterItem
                    {
                        id = aoiEntity.entityID,
                        x = aoiEntity.PoxX,
                        z = aoiEntity.PosZ
                    });
                    break;
                case CellOperationEnum.EntityMove:
                    cellUpdateItem.moveItemsList.Add(new MoveItem
                    {
                        id = aoiEntity.entityID,
                        x = aoiEntity.PoxX,
                        z = aoiEntity.PosZ
                    });
                    break;
                case CellOperationEnum.EntityExit:
                    cellUpdateItem.exitItemsList.Add(new ExitItem
                    {
                        id = aoiEntity.entityID
                    });
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 计算AOICell内所有实体的操作合并
        /// </summary>
        public void CalculateCellViewEntityOperationCombination()
        {
            if (!cellUpdateItem.IsEmpty)
            {
                aoiManager.OnCellViewEntityOperationCombination?.Invoke(this, cellUpdateItem);
                cellUpdateItem.Reset();
            }
        }
    }

    public enum CellOperationEnum
    {
        EntityEnter,
        EntityMove,
        EntityExit
    }
}
