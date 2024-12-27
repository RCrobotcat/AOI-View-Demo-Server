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

    public class RegularConfigs
    {
        public const int aoiSize = 50;
        public const int aoiInitCount = 200;
        public const float moveSpeed = 40;

        public const int borderX = 500; // 边界X
        public const int borderZ = 500; // 边界Z

        public const int randomChangeDirInterval = 1; // 随机改变方向的时间间隔: 1s
        public const int randomChangeDirRate = 30; // 随机改变方向的概率: 30%

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
