using System;

// AOI单元(宫格)
namespace AOICell
{
    public class AOICell
    {
        public int xIndex; // x索引
        public int zIndex; // z索引
        public AOIManager aoiManager;

        public AOICell(int xIndex, int zIndex, AOIManager aoiManager)
        {
            this.xIndex = xIndex;
            this.zIndex = zIndex;
            this.aoiManager = aoiManager;
        }
    }
}
