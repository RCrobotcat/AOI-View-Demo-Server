using AOIProtocol;
using PENet;
using System.Collections.Concurrent;
using System.Numerics;

// 服务器根节点
namespace AOIServer
{
    public class ServerRoot
    {
        private static ServerRoot instance;
        public static ServerRoot Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ServerRoot();
                }
                return instance;
            }
        }

        // 网络通信相关模块
        AsyncNet<ServerSession, Package> server = new AsyncNet<ServerSession, Package>();
        ConcurrentQueue<NetPackage> packageQueue = new ConcurrentQueue<NetPackage>(); // 消息包队列

        // 游戏内相关模块
        BattleStage stage = new BattleStage();

        public void Init()
        {
            server.StartAsServer("127.0.0.1", 18000);
            stage.InitStage(101);
        }
        public void Tick()
        {
            while (!packageQueue.IsEmpty)
            {
                if (packageQueue.TryDequeue(out NetPackage package))
                {
                    switch (package.package.cmd)
                    {
                        case CMD.LoginRequest:
                            LoginStage(package);
                            break;
                        case CMD.SendMovePos:
                            stage.MoveEntity(package.package.sendMovePos);
                            break;
                        case CMD.SendExitStage:
                            stage.EntityExit(package.package.sendExit.entityID);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    this.Error("Failed to dequeue package.");
                }
            }

            stage.TickStage();
        }
        public void UnInit()
        {
            stage.UnInitStage();
        }

        public void AddMsgPackage(NetPackage package)
        {
            packageQueue.Enqueue(package);
        }

        /// <summary>
        /// 登录关卡
        /// </summary>
        void LoginStage(NetPackage package)
        {
            BattleEntity entity = new BattleEntity
            {
                entityID = GetEntityUniqueID(),
                session = package.session,
                playerInitPos = new Vector3(10, 0, 10),
                playerState = PlayerStateEnum.None
            };

            stage.EnterStage(entity);

            package.session.SendMsg(new Package
            {
                cmd = CMD.LoginResponse,
                loginResponse = new LoginResponse
                {
                    entityID = entity.entityID
                }
            });
        }

        uint uid = 1000;
        uint GetEntityUniqueID()
        {
            return ++uid;
        }
    }
}
