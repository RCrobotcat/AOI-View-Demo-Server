using PENet;
using System;
using System.Collections.Generic;

// 服务器启动入口
namespace AOIProtocol
{
    public enum CMD
    {
        LoginRequest,
        LoginResponse,

        NtfAOIMsg, // AOI通知消息

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

        public NtfAOIMsg ntfAOIMsg;
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

    /// <summary>
    /// AOI通知消息
    /// </summary>
    [Serializable]
    public class NtfAOIMsg
    {
        public int type;
        public List<EnterMsg> enterLst;
        public List<MoveMsg> moveLst;
        public List<ExitMsg> exitLst;
        public override string ToString()
        {
            string content = "";
            if (enterLst != null)
            {
                for (int i = 0; i < enterLst.Count; i++)
                {
                    EnterMsg em = enterLst[i];
                    content += $"Enter:{em.entityID} {em.PosX},{em.PosZ}\n";
                }
            }
            if (moveLst != null)
            {
                for (int i = 0; i < moveLst.Count; i++)
                {
                    MoveMsg mm = moveLst[i];
                    content += $"Move:{mm.entityID} {mm.PosX},{mm.PosZ}\n";
                }
            }
            if (exitLst != null)
            {
                for (int i = 0; i < exitLst.Count; i++)
                {
                    ExitMsg mm = exitLst[i];
                    content += $"Exit:{mm.entityID}\n";
                }
            }
            return content;
        }
    }
    [Serializable]
    public class EnterMsg
    {
        public uint entityID;
        public float PosX;
        public float PosZ;
    }
    [Serializable]
    public class MoveMsg
    {
        public uint entityID;
        public float PosX;
        public float PosZ;
    }
    [Serializable]
    public class ExitMsg
    {
        public uint entityID;
    }
    [Serializable]
    public class NtfCell
    {
        public int xIndex;
        public int zIndex;
    }
    [Serializable]
    public class SndMovePos
    {
        public uint entityID;
        public float PosX;
        public float PosZ;
    }
    [Serializable]
    public class SndExit
    {
        public uint entityID;
    }
}
