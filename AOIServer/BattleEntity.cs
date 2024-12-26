using AOIProtocol;
using System.Numerics;

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

        public void SendMsg(Package package)
        {
            session?.SendMsg(package);
        }
    }
}
