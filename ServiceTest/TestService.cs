using ServiceFramework;
using System;
using System.Threading.Tasks;

namespace ServiceTest
{
    class TestService : ServiceBase
    {
        bool Finished = false;

        public TestService()
        {
            Flag = "-Tct";
            this.OnStart += TestService_OnStart;
            this.OnStop += TestService_OnStop;
            this.ReceivedArgument += TestService_ReceivedArgument;
        }

        private void TestService_ReceivedArgument(string[] obj)
        {
            if (obj.Length > 0)
                Console.WriteLine($"Get: {String.Join(" ", obj)}");
        }

        private void TestService_OnStop()
        {
            Console.WriteLine("Start Ending...");
            while (!Finished)
            {
                Console.WriteLine("Checking Works... Please wait.");
                System.Threading.Thread.Sleep(1000);
            }
        }

        private void TestService_OnStart()
        {
            Finished = false;
            Console.WriteLine("This work is started.");
            Task.Run(() =>
            {
                while (!this.IsStop)
                {
                    Console.WriteLine($"Current time: {DateTime.Now.ToString()}");
                    System.Threading.Thread.Sleep(5000);
                }
                Finished = true;
            });
        }
    }
}
