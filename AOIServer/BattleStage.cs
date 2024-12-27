using AOICell;
using AOIProtocol;

// 游戏关卡类
namespace AOIServer
{
    public class StageConfig
    {
        public int stageID;
        public string stageName;

        public int aOISize;
        public int initCount = 100;
    }

    public class BattleStage
    {
        public StageConfig stageConfig;

        public AOIManager aOIManager; // AOI管理器

        public void InitStage(int stageID)
        {
            stageConfig = new StageConfig
            {
                stageID = stageID,
                stageName = $"Stage_{stageID}",

                aOISize = RegularConfigs.aoiSize,
                initCount = RegularConfigs.aoiInitCount
            };

            // 根据关卡配置生成AOI配置
            AOIConfig aoiConfig = new AOIConfig
            {
                mapName = stageConfig.stageName,
                cellSize = stageConfig.aOISize,
                initCount = stageConfig.initCount
            };

            aOIManager = new AOIManager(aoiConfig);

            this.LogYellow($"Init Stage:{stageID} done!");
        }
        public void TickStage() { }
        public void UnInitStage() { }

        /// <summary>
        /// 进入关卡
        /// </summary>
        public void EnterStage(BattleEntity entity)
        {

        }

        /// <summary>
        /// 更新关卡
        /// </summary>
        public void UpdateStage(BattleEntity entity)
        {

        }

        /// <summary>
        /// 退出关卡
        /// </summary>
        public void ExitStage(BattleEntity entity)
        {

        }
    }
}
