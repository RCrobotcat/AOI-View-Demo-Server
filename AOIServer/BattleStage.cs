using AOICell;
using AOIProtocol;
using System.Collections.Concurrent;

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

        private ConcurrentDictionary<uint, BattleEntity> entityDic = new ConcurrentDictionary<uint, BattleEntity>(); // 所有实体

        private ConcurrentQueue<BattleEntity> operationEnterQueue = new ConcurrentQueue<BattleEntity>(); // 发起进入关卡操作的队列
        private ConcurrentQueue<BattleEntity> operationExitQueue = new ConcurrentQueue<BattleEntity>(); // 发起退出关卡操作的队列
        private ConcurrentQueue<BattleEntity> operationMoveQueue = new ConcurrentQueue<BattleEntity>(); // 发起移动人物操作的队列

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
        public void TickStage()
        {
            // 处理退出关卡队列
            while (operationExitQueue.TryDequeue(out BattleEntity entity))
            {
                aOIManager.ExitCell(entity.aOIEntity);
                if (entityDic.TryRemove(entity.entityID, out BattleEntity e))
                {
                    e.OnExitStage();
                    e.aOIEntity = null;
                }
                else
                {
                    this.Error($"Entity:{entity.entityID} not exists in entityDic!");
                }
            }

            // 处理进入关卡队列
            while (operationEnterQueue.TryDequeue(out BattleEntity entity))
            {
                entity.aOIEntity = aOIManager.EnterCell(entity.entityID, entity.playerPos.X, entity.playerPos.Z);
                if (!entityDic.ContainsKey(entity.entityID))
                {
                    if (entityDic.TryAdd(entity.entityID, entity))
                    {
                        entity.OnEnterStage(this);
                    }
                    else
                    {
                        this.Error($"Entity:{entity.entityID} already exists in entityDic!");
                    }
                }
                else
                {
                    this.Error($"Entity:{entity.entityID} already exists in stage:{stageConfig.stageName}(stageID:{stageConfig.stageID})!");
                }
            }

            // 处理移动队列
            while (operationMoveQueue.TryDequeue(out BattleEntity entity))
            {
                aOIManager.UpdateEntityPosition(entity.aOIEntity, entity.playerPos.X, entity.playerPos.Z);
            }

            aOIManager.CalculateAOIUpdate(); // 驱动AOI
        }
        public void UnInitStage()
        {
            operationEnterQueue.Clear();
            operationExitQueue.Clear();
            operationMoveQueue.Clear();
        }

        /// <summary>
        /// 进入关卡
        /// </summary>
        public void EnterStage(BattleEntity entity)
        {
            this.Log($"Entity: {entity.entityID} is entering stage...");
            if (!entityDic.ContainsKey(entity.entityID))
            {
                operationEnterQueue.Enqueue(entity);
                this.LogGreen($"EnterStage entityID:{entity.entityID} success!");
            }
            else
            {
                this.Warn($"EnterStage entityID:{entity.entityID} already in stage:{stageConfig.stageName}(stageID:{stageConfig.stageID})!");
            }
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
            if (entityDic.ContainsKey(entity.entityID))
            {
                operationExitQueue.Enqueue(entity);
                this.LogYellow($"ExitStage entityID:{entity.entityID} success!");
            }
            else
            {
                this.Warn($"ExitStage entityID:{entity.entityID} not exists in stage:{stageConfig.stageName}(stageID:{stageConfig.stageID})!");
            }
        }
    }
}