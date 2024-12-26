using PENet;
using AOIProtocol;

// 服务器会话管理
namespace AOIServer
{
    public class ServerSession : AsyncSession<Package>
    {
        protected override void OnConnected(bool result)
        {
            this.LogGreen("New Client Online: {0}.", result);
        }

        protected override void OnDisConnected()
        {
            this.LogYellow("Cliend Offline.");
        }

        protected override void OnReceiveMsg(Package msg)
        {
            this.LogGreen($"Receive Msg from Client: {msg.cmd.ToString()}.");
            NetPackage netPackage = new NetPackage(this, msg);
            ServerRoot.Instance.AddMsgPackage(netPackage);
        }
    }

    /// <summary>
    /// 网络消息数据包(包含属于哪个Session会话)
    /// </summary>
    public class NetPackage
    {
        public ServerSession session;
        public Package package;

        public NetPackage(ServerSession session, Package package)
        {
            this.session = session;
            this.package = package;
        }
    }
}
