using AOIProtocol;
using PENet;
using System.Collections.Concurrent;
using System.Numerics;
using AOICellSpace;

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
                playerState = PlayerStateEnum.None,
                driverEnum = EntityDriverEnum.Client
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

        Random random = new Random();
        /// <summary>
        /// 创建服务器实体(用于AOI测试)
        /// </summary>
        public void CreateServerEntity()
        {
            float randomPosX = random.Next(-RegularConfigs.borderX, RegularConfigs.borderX);
            float randomPosZ = random.Next(-RegularConfigs.borderZ, RegularConfigs.borderZ);

            BattleEntity entity = new BattleEntity
            {
                entityID = GetServerUniqueID(),
                playerTargetDirPos = new Vector3(randomPosX, 0, randomPosZ),
                playerState = PlayerStateEnum.None,
                driverEnum = EntityDriverEnum.Server
            };

            stage.EnterStage(entity);
        }

        uint uid = 1000;
        uint GetEntityUniqueID()
        {
            return ++uid;
        }

        uint serverUid = 2000;
        public uint GetServerUniqueID()
        {
            return ++serverUid;
        }
    }
}
