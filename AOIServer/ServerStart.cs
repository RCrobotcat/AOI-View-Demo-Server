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

            Console.WriteLine("Hello World!");

            Task.Run(() =>
            {
                ServerRoot.Instance.Init();

                while (true)
                {
                    ServerRoot.Instance.Tick();

                    Thread.Sleep(10); // sleep 10ms: 防止线程过度占用
                }
            });

            while (true)
            {
                string input = Console.ReadLine();
            }
        }
    }
}
