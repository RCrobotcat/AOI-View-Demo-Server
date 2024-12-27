using System.Collections.Generic;

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
    }
}
