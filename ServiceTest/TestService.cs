namespace ServiceTest
{
    class TestService : ServiceFramework.ServiceBase
    {
        bool Finished = false;

        public TestService()
        {
            Flag = "-Tct";
        }

        public override void OnStop()
        {
            Log("Start Ending...");
            while (!Finished)
            {
                Log("Checking Works... Please wait.");
                System.Threading.Thread.Sleep(1000);
            }
        }

        public override void OnStart()
        {
            Finished = false;
            Log("This work is started.");
            System.Threading.Tasks.Task.Run(() =>
            {
                while (!this.IsStop)
                {
                    Log($"Current time: {System.DateTime.Now.ToString()}", false);
                    System.Threading.Thread.Sleep(5000);
                }
                Finished = true;
            });
        }

        public override void ReceivedArguments(string[] args)
        {
            if (args.Length > 0)
                Log($"Get: {string.Join(" ", args)}");
        }

        private void Log(string str, bool tosupervisor = true)
        {
            System.Console.WriteLine(str);
            if (tosupervisor) SendMessage(str);
        }
    }
}
