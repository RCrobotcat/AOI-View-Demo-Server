using System;

// AOI单元(宫格)
namespace AOICell
{
    public class AOICell
    {
        public int xIndex; // x索引
        public int zIndex; // z索引
        public AOIManager aoiManager;

        public AOICell[] aroundCells = null; // 周围的宫格
        public bool isCalculateBoundaries = false; // 是否已经计算了边界

        public AOICell(int xIndex, int zIndex, AOIManager aoiManager)
        {
            this.xIndex = xIndex;
            this.zIndex = zIndex;
            this.aoiManager = aoiManager;
        }

        /// <summary>
        /// 进入宫格
        /// 可能是：传送进入宫格或者移动进入宫格
        /// </summary>
        public void EnterCell(AOIEntity aOIEntity)
        {

        }

        /// <summary>
        /// 在宫格内部移动
        /// </summary>
        public void MoveInsideCell(AOIEntity aOIEntity)
        {

        }
    }
}
