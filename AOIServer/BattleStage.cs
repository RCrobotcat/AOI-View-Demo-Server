using AOICellSpace;
using AOIProtocol;
using PENet;
using System.Collections.Concurrent;
using System.Numerics;

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

            aOIManager = new AOIManager(aoiConfig)
            {
                OnEntityCellViewChange = EntityCellViewChange,
                OnCellViewEntityOperationCombination = CellViewEntityOperationCombination,
#if DEBUG
                OnCreateNewCell = (xIndex, zIndex) =>
                {
                    Package pkg = new Package
                    {
                        cmd = CMD.NtfCell,
                        ntfCell = new NtfCell
                        {
                            xIndex = xIndex,
                            zIndex = zIndex
                        }
                    };

                    foreach (var item in entityDic)
                    {
                        item.Value.SendMsg(pkg);
                    }
                },
#endif
            };

            historyTime = DateTime.Now;

            this.LogYellow($"Init Stage:{stageID} done!");
        }
        public void TickStage()
        {
            if (CheckEntityDicHaveClient())
                RandomServerEntityAITest(); // 随机服务器实体AI用于测试

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
                entity.aOIEntity = aOIManager.EnterCell(entity.entityID, entity.playerInitPos.X, entity.playerInitPos.Z, entity.driverEnum);
                if (!entityDic.ContainsKey(entity.entityID))
                {
                    if (entityDic.TryAdd(entity.entityID, entity))
                    {
                        entity.OnEnterStage(this);

#if DEBUG
                        foreach (var item in aOIManager.GetAOICellDic())
                        {
                            Package pkg = new Package
                            {
                                cmd = CMD.NtfCell,
                                ntfCell = new NtfCell
                                {
                                    xIndex = item.Value.xIndex,
                                    zIndex = item.Value.zIndex
                                }
                            };

                            entity.SendMsg(pkg);
                        }
#endif
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
                aOIManager.UpdateEntityPosition(entity.aOIEntity, entity.playerTargetDirPos.X, entity.playerTargetDirPos.Z);
            }

            aOIManager.CalculateAOIUpdate(); // 驱动AOI
        }
        public void UnInitStage()
        {
            entityDic.Clear();
            operationEnterQueue.Clear();
            operationExitQueue.Clear();
            operationMoveQueue.Clear();

            stageConfig = null;
            aOIManager = null;
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
            if (entityDic.ContainsKey(entity.entityID))
            {
                operationMoveQueue.Enqueue(entity);
            }
            else
            {
                this.Warn($"UpdateStage entityID:{entity.entityID} not exists in stage:{stageConfig.stageName}(stageID:{stageConfig.stageID})!");
            }
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

        /// <summary>
        /// 应用层中AOI实体的视野变化回调
        /// 针对于一个实体而言
        /// </summary>
        void EntityCellViewChange(AOIEntity aOIEntity, UpdateItem item)
        {
            Package pkg = new Package
            {
                cmd = CMD.NtfAOIMsg,
                ntfAOIMsg = new NtfAOIMsg
                {
                    enterLst = new List<EnterMsg>(),
                    exitLst = new List<ExitMsg>()
                }
            };

            // 将Enter更新写入网络协议消息包
            if (item.enterItemsList.Count > 0)
            {
                for (int i = 0; i < item.enterItemsList.Count; i++)
                {
                    pkg.ntfAOIMsg.enterLst.Add(new EnterMsg
                    {
                        entityID = item.enterItemsList[i].id,
                        PosX = item.enterItemsList[i].x,
                        PosZ = item.enterItemsList[i].z
                    });

                    this.LogGreen($"AOIEntity: {item.enterItemsList[i].id} enter, x: {item.enterItemsList[i].x}, z: {item.enterItemsList[i].z}");
                }
            }

            // 将Exit更新写入网络协议消息包
            if (item.exitItemsList.Count > 0)
            {
                for (int i = 0; i < item.exitItemsList.Count; i++)
                {
                    pkg.ntfAOIMsg.exitLst.Add(new ExitMsg
                    {
                        entityID = item.exitItemsList[i].id,
                    });

                    this.LogYellow($"AOIEntity: {item.exitItemsList[i].id} exit.");
                }
            }

            if (entityDic.TryGetValue(aOIEntity.entityID, out BattleEntity battleEntity))
            {
                battleEntity.OnUpdateStage(pkg);
            }
        }

        /// <summary>
        /// 应用层中AOI宫格内实体操作合并回调
        /// 针对于一个宫格而言
        /// </summary>
        void CellViewEntityOperationCombination(AOICell cell, UpdateItem item)
        {
            Package pkg = new Package
            {
                cmd = CMD.NtfAOIMsg,
                ntfAOIMsg = new NtfAOIMsg
                {
                    enterLst = new List<EnterMsg>(),
                    exitLst = new List<ExitMsg>(),
                    moveLst = new List<MoveMsg>()
                }
            };

            // 将Enter更新写入网络协议消息包
            if (item.enterItemsList.Count > 0)
            {
                for (int i = 0; i < item.enterItemsList.Count; i++)
                {
                    pkg.ntfAOIMsg.enterLst.Add(new EnterMsg
                    {
                        entityID = item.enterItemsList[i].id,
                        PosX = item.enterItemsList[i].x,
                        PosZ = item.enterItemsList[i].z
                    });
                    this.Log($"AOIEntity: {item.enterItemsList[i].id} enter Cell: {cell.xIndex}_{cell.zIndex}, x: {item.enterItemsList[i].x}, z: {item.enterItemsList[i].z}");
                }
            }

            // 将Move更新写入网络协议消息包
            if (item.moveItemsList.Count > 0)
            {
                for (int i = 0; i < item.moveItemsList.Count; i++)
                {
                    pkg.ntfAOIMsg.moveLst.Add(new MoveMsg
                    {
                        entityID = item.moveItemsList[i].id,
                        PosX = item.moveItemsList[i].x,
                        PosZ = item.moveItemsList[i].z
                    });
                }
            }

            // 将Exit更新写入网络协议消息包
            if (item.exitItemsList.Count > 0)
            {
                for (int i = 0; i < item.exitItemsList.Count; i++)
                {
                    pkg.ntfAOIMsg.exitLst.Add(new ExitMsg
                    {
                        entityID = item.exitItemsList[i].id
                    });
                }
            }

            // 将网络消息包序列化成字节数组
            byte[] bytesPkg = AsyncTool.PackLenInfo(AsyncTool.Serialize(pkg));

            foreach (var e in cell.aOIEntitiesSet)
            {
                if (entityDic.TryGetValue(e.entityID, out BattleEntity entity))
                {
                    entity.OnUpdateStage(bytesPkg);
                }
            }
        }

        /// <summary>
        /// 移动实体
        /// </summary>
        public void MoveEntity(SendMovePos sendMovePosRequest)
        {
            if (entityDic.TryGetValue(sendMovePosRequest.entityID, out BattleEntity entity))
            {
                entity.playerTargetDirPos = new Vector3(sendMovePosRequest.PosX, 0, sendMovePosRequest.PosZ);
                UpdateStage(entity);
            }
        }

        /// <summary>
        /// 实体退出关卡
        /// </summary>
        public void EntityExit(uint entityID)
        {
            if (entityDic.TryGetValue(entityID, out BattleEntity entity))
            {
                ExitStage(entity);
            }
        }

        Random random = new Random();
        DateTime historyTime;
        DateTime lastTickTime = DateTime.Now;
        /// <summary>
        /// 随机服务器实体AI用于测试
        /// </summary>
        void RandomServerEntityAITest()
        {
            if (DateTime.Now > lastTickTime.AddSeconds(RegularConfigs.randomChangeDirInterval))
            {
                lastTickTime = DateTime.Now;

                foreach (var item in entityDic)
                {
                    BattleEntity entity = item.Value;
                    if (entity.driverEnum == EntityDriverEnum.Server
                        && random.Next(0, 100) < RegularConfigs.randomChangeDirRate) // 有一定几率改变方向
                    {
                        if (Math.Abs(entity.playerTargetDirPos.X) >= RegularConfigs.borderX
                            || Math.Abs(entity.playerTargetDirPos.Z) >= RegularConfigs.borderZ)
                        {
                            float randomX = random.Next(-RegularConfigs.borderX, RegularConfigs.borderX);
                            float randomZ = random.Next(-RegularConfigs.borderZ, RegularConfigs.borderZ);
                            entity.playerInitPos = Vector3.Normalize(new Vector3(randomX, 0, randomZ) * 0.9f - entity.playerTargetDirPos);
                        }
                        else
                        {
                            float randomX = random.Next(-100, 100);
                            float randomZ = random.Next(-100, 100);
                            entity.playerInitPos = Vector3.Normalize(new Vector3(randomX == 0 ? 1 : randomX, 0, randomZ == 0 ? 1 : randomZ));
                        }
                    }
                }
            }

            DateTime now = DateTime.Now;
            float deltaTime = (float)(now - historyTime).TotalMilliseconds / 1000;
            historyTime = now;
            foreach (var item in entityDic)
            {
                BattleEntity entity = item.Value;
                if (entity.driverEnum == EntityDriverEnum.Server)
                {
                    entity.playerTargetDirPos += entity.playerInitPos * RegularConfigs.moveSpeed * deltaTime;
                    UpdateStage(entity);
                }
            }
        }
        /// <summary>
        /// 检查实体字典中是否有客户端实体
        /// </summary>
        bool CheckEntityDicHaveClient()
        {
            foreach (var item in entityDic)
            {
                if (item.Value.driverEnum == EntityDriverEnum.Client)
                {
                    return true;
                }
            }
            return false;
        }
    }
}