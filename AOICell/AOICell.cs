

// AOI单元(宫格)
using System.Linq;

namespace AOICell
{
    public class AOICell
    {
        public int xIndex; // x索引
        public int zIndex; // z索引
        public AOIManager aoiManager;

        public AOICell[] aroundCells = null; // 周围的宫格
        public bool isCalculateBoundaries = false; // 是否已经计算了边界

        public UpdateItem cellUpdateItem; // Cell所有的更新

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
    }

    public enum CellOperationEnum
    {
        EntityEnter,
        EntityMove,
        EntityExit
    }
}
