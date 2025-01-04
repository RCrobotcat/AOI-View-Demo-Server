using System;
using System.Threading.Tasks;
using PEUtils;

namespace AOIServer
{
    class ServerStart
    {
        static void Main(string[] args)
        {
            LogConfig cfg = new()
            {
                loggerEnum = LoggerType.Console,
            };
            PELog.InitSettings(cfg);

            int testMonsterCount = 0; // 测试怪物数量

            Console.WriteLine("Hello World!");

            Task.Run(() =>
            {
                ServerRoot.Instance.Init();

                while (true)
                {
                    for (int i = 0; i < testMonsterCount; i++)
                    {
                        ServerRoot.Instance.CreateServerEntity();
                    }
                    testMonsterCount = 0;

                    ServerRoot.Instance.Tick();

                    Thread.Sleep(10); // sleep 10ms: 防止线程过度占用
                }
            });

            while (true)
            {
                string input = Console.ReadLine();

                if (input == "quit")
                {
                    break;
                }

                if (int.Parse(input) > 0)
                {
                    testMonsterCount = int.Parse(input);
                }

            }
        }
    }
}
