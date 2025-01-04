using AOIProtocol;
using System.Numerics;
using AOICellSpace;

namespace AOIServer
{
    public enum PlayerStateEnum
    {
        None = 0,
        Online = 1, // 玩家上线
        Offline = 2, // 玩家离线
        Mandate = 3 // 服务器托管状态
    }

    public class BattleEntity
    {
        public uint entityID;
        public PlayerStateEnum playerState; // 玩家当前状态
        public EntityDriverEnum driverEnum = EntityDriverEnum.None; // 实体驱动类型

        public ServerSession session; // 哪一个session会话来处理连接

        public Vector3 playerTargetDirPos; // 玩家当前的目标位置
        public Vector3 playerInitPos; // 玩家初始位置

        public AOIEntity aOIEntity; // 玩家所在的AOI实体(玩家实体对应的AOI实体)

        public void SendMsg(Package package)
        {
            session?.SendMsg(package);
        }
        public void SendMsg(byte[] bytePackage)
        {
            session?.SendMsg(bytePackage);
        }

        /// <summary>
        /// 当前实体进入关卡的回调
        /// </summary>
        public void OnEnterStage(BattleStage stage)
        {
            playerState = PlayerStateEnum.Online;
            this.LogCyan($"Player(entityID: {entityID}) Online! Enter Stage:{stage.stageConfig.stageID}.");
        }

        /// <summary>
        /// 当前实体在关卡中更新的回调
        /// </summary>
        public void OnUpdateStage(Package package)
        {
            if (playerState == PlayerStateEnum.Online)
            {
                SendMsg(package);
            }
        }
        /// <summary>
        /// 当前实体在关卡中更新的回调
        /// </summary>
        public void OnUpdateStage(byte[] bytePackage)
        {
            if (playerState == PlayerStateEnum.Online)
            {
                SendMsg(bytePackage);
            }
        }

        /// <summary>
        /// 当前实体离开关卡的回调
        /// </summary>
        public void OnExitStage()
        {
            playerState = PlayerStateEnum.Offline;
            this.LogYellow($"Player(entityID: {entityID}) Offline!");
        }
    }
}
