using System;
using System.Collections.Generic;

// AOI实体
namespace AOICellSpace
{
    public enum EntityOperationEnum
    {
        None,
        TransferEnter, // 传送进入
        TransferOut, // 传送离开

        MoveCross, // 移动进入
        MoveInside, // 在宫格内部移动
        MoveOut, // 移动离开
    }
    public enum MoveCrossCellDirEnum
    {
        None,
        Up,
        Down,
        Left,
        Right,
        LeftUp,
        RightUp,
        RightDown,
        LeftDown
    }
    public enum EntityDriverEnum
    {
        None,
        Client, // 客户端驱动, 需要计数(计数指有多少个客户端在关注这个宫格)
        Server // 服务器驱动, 不需要计数
    }

    public class AOIEntity
    {
        public uint entityID; // 实体ID
        public AOIManager aoiManager;

        // 当前实体所在的宫格的xz索引
        public int xIndex = 0;
        public int zIndex = 0;

        // 当前实体的历史宫格(上一个宫格)的xz索引
        public int oldXIndex = 0;
        public int oldZIndex = 0;

        // 当前实体所在的宫格的Key
        public string cellKey = "";
        // 当前实体的历史宫格(上一个宫格)的Key
        public string oldCellKey = "";

        // 当前实体的位置
        private float posX = 0;
        public float PoxX { get => posX; }
        private float posZ = 0;
        public float PosZ { get => posZ; }

        private EntityOperationEnum operationEnum;
        public EntityOperationEnum EntityOperation { get => operationEnum; }

        private MoveCrossCellDirEnum moveCrossCellDirEnum;
        public MoveCrossCellDirEnum MoveCrossCellDir { get => moveCrossCellDirEnum; }

        private EntityDriverEnum entityDriverEnum;
        public EntityDriverEnum EntityDriver { get => entityDriverEnum; }

        AOICell[] aroundAddCell = null; // 存量视野周围新增的宫格

        List<AOICell> singleCellToBeRemovedList = new List<AOICell>(5); // 存量视野中要移除的单个宫格列表
        List<AOICell> singleCellToBeAddedList = new List<AOICell>(5); // 存量视野中要增加的单个宫格列表

        private UpdateItem entityUpdateItem; // 更新项

        public AOIEntity(uint entityID, AOIManager aoiManager, EntityDriverEnum driverEnum)
        {
            this.entityID = entityID;
            this.aoiManager = aoiManager;
            this.entityDriverEnum = driverEnum;

            entityUpdateItem = new UpdateItem(aoiManager.AOICfg.updateEnterAmount,
                aoiManager.AOICfg.updateMoveAmount,
                aoiManager.AOICfg.updateExitAmount);
        }

        /// <summary>
        /// 更新实体位置
        /// </summary>
        public void UpdatePosition(float x, float z, EntityOperationEnum operation = EntityOperationEnum.None)
        {
            posX = x;
            posZ = z;
            operationEnum = operation;

            int _xIndex = (int)(Math.Floor(posX / aoiManager.CellSize));
            int _zIndex = (int)(Math.Floor(posZ / aoiManager.CellSize));
            string _cellKey = $"{_xIndex}_{_zIndex}";

            if (_cellKey != cellKey)
            {
                oldXIndex = xIndex;
                oldZIndex = zIndex;
                oldCellKey = cellKey;

                if (cellKey != "")
                {
                    aoiManager.MarkEntityExitCell(this);
                }

                xIndex = _xIndex;
                zIndex = _zIndex;
                cellKey = _cellKey;

                if (operationEnum != EntityOperationEnum.TransferEnter && operationEnum != EntityOperationEnum.TransferOut)
                {
                    operationEnum = EntityOperationEnum.MoveCross;
                    if (xIndex < oldXIndex)
                    {
                        if (zIndex == oldZIndex)
                        {
                            moveCrossCellDirEnum = MoveCrossCellDirEnum.Left;
                        }
                        else if (zIndex < oldZIndex)
                        {
                            moveCrossCellDirEnum = MoveCrossCellDirEnum.LeftDown;
                        }
                        else
                        {
                            moveCrossCellDirEnum = MoveCrossCellDirEnum.LeftUp;
                        }
                    }
                    else if (xIndex > oldXIndex)
                    {
                        if (zIndex == oldZIndex)
                        {
                            moveCrossCellDirEnum = MoveCrossCellDirEnum.Right;
                        }
                        else if (zIndex < oldZIndex)
                        {
                            moveCrossCellDirEnum = MoveCrossCellDirEnum.RightDown;
                        }
                        else
                        {
                            moveCrossCellDirEnum = MoveCrossCellDirEnum.RightUp;
                        }
                    }
                    else
                    {
                        if (zIndex > oldZIndex)
                        {
                            moveCrossCellDirEnum = MoveCrossCellDirEnum.Up;
                        }
                        else
                        {
                            moveCrossCellDirEnum = MoveCrossCellDirEnum.Down;
                        }
                    }
                }

                // 移动进入新的宫格
                this.Log($"Entity: {entityID} Move Cross Cell: {cellKey}");
                aoiManager.MoveCrossCell(this);
            }
            else
            {
                operationEnum = EntityOperationEnum.MoveInside;
                moveCrossCellDirEnum = MoveCrossCellDirEnum.None;
                // this.Log($"Entity: {entityID} Move Inside Cell: {cellKey}");
                aoiManager.MoveInsideCell(this);
            }
        }

        /// <summary>
        /// 保存存量视野增加的九宫格
        /// </summary>
        public void AddAroundCellView(AOICell[] aroundCells)
        {
            this.aroundAddCell = aroundCells;
        }

        /// <summary>
        /// 计算实体视野变化
        /// 存量增删
        /// </summary>
        public void CalculateEntityCellViewChange()
        {
            AOICell cell = aoiManager.GetOrCreateCell(this);
            if (cell.clientEntityConcernCount > 0 && entityDriverEnum == EntityDriverEnum.Client)
            {
                if (aroundAddCell != null)
                {
                    for (int i = 0; i < aroundAddCell.Length; i++)
                    {
                        HashSet<AOIEntity> entities = aroundAddCell[i].aOIEntitiesSet;
                        foreach (var e in entities)
                        {
                            entityUpdateItem.enterItemsList.Add(new EnterItem(e.entityID, e.PoxX, e.PosZ));
                        }
                    }
                }

                for (int i = 0; i < singleCellToBeAddedList.Count; i++)
                {
                    HashSet<AOIEntity> set = singleCellToBeAddedList[i].aOIEntitiesSet;
                    foreach (var e in set)
                    {
                        entityUpdateItem.enterItemsList.Add(new EnterItem(e.entityID, e.posX, e.posZ));
                    }
                }
                for (int i = 0; i < singleCellToBeRemovedList.Count; i++)
                {
                    HashSet<AOIEntity> set = singleCellToBeRemovedList[i].aOIEntitiesSet;
                    foreach (var e in set)
                    {
                        entityUpdateItem.exitItemsList.Add(new ExitItem(e.entityID));
                    }
                }

                if (!entityUpdateItem.IsEmpty)
                {
                    aoiManager.OnEntityCellViewChange?.Invoke(this, entityUpdateItem);
                    entityUpdateItem.Reset();
                }
            }

            aroundAddCell = null;
            singleCellToBeAddedList.Clear();
            singleCellToBeRemovedList.Clear();
        }

        /// <summary>
        /// 移除宫格视野
        /// </summary>
        /// <param name="cell">视野中要移除的宫格</param>
        public void RemoveCellView(AOICell cell)
        {
            if (entityDriverEnum == EntityDriverEnum.Client)
                singleCellToBeRemovedList.Add(cell);
        }
        /// <summary>
        /// 增加宫格视野
        /// </summary>
        /// <param name="cell">视野中要增加的宫格</param>
        public void AddCellView(AOICell cell)
        {
            if (entityDriverEnum == EntityDriverEnum.Client)
                singleCellToBeAddedList.Add(cell);
        }
    }
}
