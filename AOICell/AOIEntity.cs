using System;
using System.Collections.Generic;
using System.Text;

// AOI实体
namespace AOICell
{
    public class AOIEntity
    {
        public int entityID; // 实体ID
        public AOIManager aoiManager;

        public AOIEntity(int entityID, AOIManager aoiManager)
        {
            this.entityID = entityID;
            this.aoiManager = aoiManager;
        }
    }
}
