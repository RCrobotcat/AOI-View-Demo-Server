using System.Collections.Generic;

// AOI单元(宫格)
namespace AOICellSpace
{
    public enum CellOperationEnum
    {
        EntityEnter,
        EntityMove,
        EntityExit
    }

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
        public HashSet<AOIEntity> aOIEntitiesMoveSet = new HashSet<AOIEntity>(); // 当前宫格内的所有移动的实体(缓存)
        public HashSet<AOIEntity> aOIEntitiesExitSet = new HashSet<AOIEntity>(); // 当前宫格内的所有离开的实体(缓存)

        public int clientEntityConcernCount = 0; // 客户端中关注这个宫格的实体数量
        public int serverEntityConcernCount = 0; // 服务器中关注这个宫格的实体数量

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
                        StraightMove(UpCellArr, aOIEntity);
                        break;
                    case MoveCrossCellDirEnum.Down:
                        StraightMove(DownCellArr, aOIEntity);
                        break;
                    case MoveCrossCellDirEnum.Left:
                        StraightMove(LeftCellArr, aOIEntity);
                        break;
                    case MoveCrossCellDirEnum.Right:
                        StraightMove(RightCellArr, aOIEntity);
                        break;
                    case MoveCrossCellDirEnum.LeftUp:
                        SkewMove(upLeftCellArr, aOIEntity);
                        break;
                    case MoveCrossCellDirEnum.RightUp:
                        SkewMove(upRightCellArr, aOIEntity);
                        break;
                    case MoveCrossCellDirEnum.RightDown:
                        SkewMove(downRightCellArr, aOIEntity);
                        break;
                    case MoveCrossCellDirEnum.LeftDown:
                        SkewMove(downLeftCellArr, aOIEntity);
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
            aOIEntitiesMoveSet.Add(aOIEntity);
            for (int i = 0; i < aroundCells.Length; i++)
            {
                aroundCells[i].AddCellOperation(CellOperationEnum.EntityMove, aOIEntity);
            }
        }

        /// <summary>
        /// 退出宫格
        /// </summary>
        public void ExitCell(AOIEntity aoiEntity)
        {
            aOIEntitiesExitSet.Add(aoiEntity);
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
                    if (aoiEntity.EntityDriver == EntityDriverEnum.Client)
                    {
                        clientEntityConcernCount++;
                    }
                    else
                    {
                        serverEntityConcernCount++;
                    }

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
                    if (aoiEntity.EntityDriver == EntityDriverEnum.Client)
                    {
                        clientEntityConcernCount--;
                    }
                    else
                    {
                        serverEntityConcernCount--;
                    }

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
                if (clientEntityConcernCount > 0 && aOIEntitiesSet.Count > 0)
                    aoiManager.OnCellViewEntityOperationCombination?.Invoke(this, cellUpdateItem);
                cellUpdateItem.Reset();
            }
        }

        /// <summary>
        /// 直线移动
        /// </summary>
        /// <param name="cellArr">受影响的宫格</param>
        /// <param name="aOIEntity">哪个实体写入的操作</param>
        void StraightMove(AOICell[] cellArr, AOIEntity aOIEntity)
        {
            for (int i = 0; i < cellArr.Length; i++)
            {
                if (i < 3) // 移除视野
                {
                    aOIEntity.RemoveCellView(cellArr[i]);
                    cellArr[i].AddCellOperation(CellOperationEnum.EntityExit, aOIEntity);
                }
                else if (i >= 3 && i < 6) // 增加视野
                {
                    aOIEntity.AddCellView(cellArr[i]);
                    cellArr[i].AddCellOperation(CellOperationEnum.EntityEnter, aOIEntity);
                }
                else // 移动
                {
                    cellArr[i].AddCellOperation(CellOperationEnum.EntityMove, aOIEntity);
                }
            }
        }
        /// <summary>
        /// 斜线移动
        /// </summary>
        /// <param name="arr">受影响的宫格</param>
        /// <param name="entity">哪个实体写入的操作</param>
        void SkewMove(AOICell[] arr, AOIEntity entity)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (i < 5)
                {
                    entity.RemoveCellView(arr[i]);
                    arr[i].AddCellOperation(CellOperationEnum.EntityExit, entity);
                }
                else if (i >= 5 && i < 10)
                {
                    entity.AddCellView(arr[i]);
                    arr[i].AddCellOperation(CellOperationEnum.EntityEnter, entity);
                }
                else
                {
                    arr[i].AddCellOperation(CellOperationEnum.EntityMove, entity);
                }
            }
        }

        public override string ToString()
        {
            return $"CellName:{xIndex},{zIndex} ExistEntity:{aOIEntitiesSet.Count} ClientConcernEntity:{clientEntityConcernCount} ServerConcernEntity:{serverEntityConcernCount}";
        }

    }
}
