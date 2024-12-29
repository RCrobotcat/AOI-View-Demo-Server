using System;

// AOI实体
namespace AOICell
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

        AOICell[] aroundAddCell = null; // 存量视野周围新增的宫格

        public AOIEntity(uint entityID, AOIManager aoiManager)
        {
            this.entityID = entityID;
            this.aoiManager = aoiManager;
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

                xIndex = _xIndex;
                zIndex = _zIndex;
                cellKey = _cellKey;

                if (operationEnum != EntityOperationEnum.TransferEnter && operationEnum != EntityOperationEnum.TransferOut)
                {
                    operationEnum = EntityOperationEnum.MoveCross;
                }

                // 进入新的宫格
                aoiManager.MoveCrossCell(this);
            }
            else
            {
                operationEnum = EntityOperationEnum.MoveInside;
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
    }
}
