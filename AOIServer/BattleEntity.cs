using AOIProtocol;
using System.Numerics;
using AOICell;

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

        public ServerSession session; // 哪一个session会话来处理连接

        public Vector3 playerTargetDir; // 玩家当前的目标朝向
        public Vector3 playerPos; // 玩家当前位置

        public AOIEntity aOIEntity; // 玩家所在的AOI实体(玩家实体对应的AOI实体)

        public void SendMsg(Package package)
        {
            session?.SendMsg(package);
        }

        /// <summary>
        /// 当前实体进入关卡的回调
        /// </summary>
        public void OnEnterStage(BattleStage stage)
        {

        }

        /// <summary>
        /// 当前实体在关卡中更新的回调
        /// </summary>
        public void OnUpdateStage(Package package)
        {

        }

        /// <summary>
        /// 当前实体离开关卡的回调
        /// </summary>
        public void OnExitStage()
        {

        }
    }
}
