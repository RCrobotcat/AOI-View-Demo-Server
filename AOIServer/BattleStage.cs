using System;

// 游戏关卡类
namespace AOIServer
{
    public class BattleStage
    {
        public void InitStage(int stageID)
        {
            this.LogYellow($"Init Stage:{stageID} done!");
        }
        public void TickStage() { }
        public void UnInitStage() { }

        /// <summary>
        /// 进入关卡
        /// </summary>
        public void EnterStage(BattleEntity entity)
        {

        }

        /// <summary>
        /// 更新关卡
        /// </summary>
        public void UpdateStage(BattleEntity entity)
        {

        }

        /// <summary>
        /// 退出关卡
        /// </summary>
        public void ExitStage(BattleEntity entity)
        {

        }
    }
}
