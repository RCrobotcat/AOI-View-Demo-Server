using PENet;
using System;

// 服务器启动入口
namespace AOIProtocol
{
    public enum CMD
    {
        LoginRequest,
        LoginResponse,

        SendMovePos, // 发送移动位置
        SendExitStage // 退出关卡
    }

    [Serializable]
    public class Package : AsyncMsg
    {
        public CMD cmd; // 该消息包是用来做什么的

        public LoginRequest loginRequest;
        public LoginResponse loginResponse;
    }

    /// <summary>
    /// 登录请求
    /// </summary>
    [Serializable]
    public class LoginRequest
    {
        public string account;
    }

    /// <summary>
    /// 登录响应
    /// </summary>
    [Serializable]
    public class LoginResponse
    {
        public uint entityID;
    }
}
