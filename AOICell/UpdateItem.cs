using System.Collections.Generic;

namespace AOICell
{
    public class UpdateItem
    {
        public List<EnterItem> enterItemsList;
        public List<MoveItem> moveItemsList;
        public List<ExitItem> exitItemsList;

        public bool IsEmpty
        {
            get
            {
                return enterItemsList.Count == 0 && moveItemsList.Count == 0 && exitItemsList.Count == 0;
            }
        }

        public UpdateItem(int enterAmount, int moveAmount, int exitAmount)
        {
            this.enterItemsList = new List<EnterItem>(enterAmount);
            this.moveItemsList = new List<MoveItem>(moveAmount);
            this.exitItemsList = new List<ExitItem>(exitAmount);
        }

        public void Reset()
        {
            enterItemsList.Clear();
            moveItemsList.Clear();
            exitItemsList.Clear();
        }

        public override string ToString()
        {
            string content = "";
            if (enterItemsList != null)
            {
                for (int i = 0; i < enterItemsList.Count; i++)
                {
                    EnterItem em = enterItemsList[i];
                    content += $"Enter: {em.id} {em.x},{em.z}\n";
                }
            }
            if (moveItemsList != null)
            {
                for (int i = 0; i < moveItemsList.Count; i++)
                {
                    MoveItem mm = moveItemsList[i];
                    content += $"Move: {mm.id} {mm.x},{mm.z}\n";
                }
            }
            if (exitItemsList != null)
            {
                for (int i = 0; i < exitItemsList.Count; i++)
                {
                    ExitItem em = exitItemsList[i];
                    content += $"Exit: {em.id}\n";
                }
            }

            return content;
        }
    }

    /// <summary>
    /// 进入时所有的更新
    /// </summary>
    public struct EnterItem
    {
        public uint id; // 进入的实体ID

        // 进入的位置
        public float x;
        public float z;

        public EnterItem(uint id, float x, float z)
        {
            this.id = id;
            this.x = x;
            this.z = z;
        }
    }
    /// <summary>
    /// 移动时所有的更新
    /// </summary>
    public struct MoveItem
    {
        public uint id; // 进入的实体ID

        // 进入的位置
        public float x;
        public float z;

        public MoveItem(uint id, float x, float z)
        {
            this.id = id;
            this.x = x;
            this.z = z;
        }
    }
    /// <summary>
    /// 退出时所有的更新
    /// </summary>
    public struct ExitItem
    {
        public uint id; // 进入的实体ID

        public ExitItem(uint id)
        {
            this.id = id;
        }
    }
}
