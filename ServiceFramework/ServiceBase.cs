using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceFramework
{
    public abstract class ServiceBase
    {
        /// <summary>
        /// 是否已经停止掉工作
        /// </summary>
        private bool hasStopped = false;

        /// <summary>
        /// 是否需要终止服务
        /// </summary>
        private bool IsTerminate = false;

        /// <summary>
        /// 是否是运行实例。
        /// </summary>
        private bool self = true;

        /// <summary>
        /// 服务中的通信服务。
        /// </summary>
        private CommunicationService cs;

        private ArgumentsService argsrv = new ArgumentsService() { Shell = ShellType.PowerShell };

        /// <summary>
        /// 是否收到停止消息。
        /// </summary>
        public Boolean IsStop { get; private set; } = false;

        /// <summary>
        /// 当服务收到停止命令的时候执行的代码。
        /// </summary>
        public abstract void OnStop();

        /// <summary>
        /// 当服务初始化时执行的代码。
        /// </summary>
        public abstract void OnStart();

        /// <summary>
        /// 当收到命令参数的时候响应这些参数的代码块。
        /// </summary>
        public abstract void ReceivedArguments(String[] args);

        /// <summary>
        /// 获取或者设定服务命令的标记。
        /// </summary>
        public String Flag { get { return BasicServiceOrder.Flag; } set { BasicServiceOrder.Flag = value; } }

        public ShellType Shell { get { return argsrv.Shell; } set { argsrv.Shell = value; } }

        /// <summary>
        /// 初始化通讯服务然后处理委托。
        /// </summary>
        public ServiceBase()
        {
            cs = new CommunicationService(this.GetType().FullName);
            cs.ReceivedMessage += Cs_ReceivedMessage;
            cs.ReturnedMessage += Cs_ReturnedMessage;
        }

        /// <summary>
        /// 当收到来自服务程序的消息的时候，处理消息事务。
        /// </summary>
        /// <param name="obj"></param>
        private void Cs_ReturnedMessage(SupervisorEventArgs obj)
        {
            Console.WriteLine(obj.Message);
            if (obj.Action == Order.terminate)
            {
                IsStop = true;
            }
        }

        /// <summary>
        /// 当收到来自控制程序的消息的时候，处理消息事务。
        /// </summary>
        /// <param name="obj"></param>
        private void Cs_ReceivedMessage(MessageEventArgs obj)
        {
            if (obj.Order != Order.test)
            {
                if (obj.Order == Order.stop || obj.Order == Order.terminate || obj.Order == Order.restart)
                {
                    SendAndShow("Stopping...");
                    IsStop = true;
                    Stop();
                    SendAndShow("Service has been stopped.");
                }
                if (obj.Order == Order.terminate)
                {
                    IsTerminate = true;
                }
                if (obj.Order == Order.start || obj.Order == Order.restart)
                {
                    if (hasStopped)
                    {
                        SendAndShow("Starting...");
                        var ex = Utils.InvokeSomethingWithoutWatching(() => Start());
                        if (ex != "")
                        {
                            SendAndShow(ex);
                        }
                        SendAndShow("Start successfully.");
                    }
                    else
                    {
                        cs.SendToSupervisor("Already ran.".ToSupervisorAnswer());
                    }
                }
                ReceivedArguments(obj.Arguments);
                cs.SendToSupervisor("Done.".ToSupervisorAnswer());
            }
        }

        /// <summary>
        /// 开始当前服务。
        /// </summary>
        /// <param name="args"></param>
        public void Start(string[] args = null)
        {
            try
            {
                //如果不是热启动，则尝试侦听
                if (!hasStopped)
                {
                    //尝试开始侦听，如果出错则将参数消息发送给已存在实例。
                    if (!cs.SendToHost(BasicServiceOrder.Test))
                    {
                        cs.ListenAsHost();
                    }
                    else
                    {
                        //如果有回音，则表示实例已存在。
                        throw new Exception("Instance Already Existed.");
                    }
                }
                self = true;
                IsStop = false;
                var work = Task.Run(() => OnStart());
                if (work.Wait(new TimeSpan(0, 0, 1, 0)))
                {
                    if (!hasStopped)
                    {
                        var input = "";
                        while (!IsStop)
                        {
                            input = Console.ReadLine();
                            if (input != "") cs.SendToHost(argsrv.Generate(input));
                        }
                    }
                    else
                    {
                        hasStopped = !hasStopped;
                    }
                }
                else
                {
                    var error = "The service can not initialize in 60 seconds, this work has been revoked.";
                    if (!hasStopped)
                    {
                        throw new Exception(error);
                    }
                    Console.WriteLine(error);
                }
            }
            catch (Exception ex)
            {
                if (args.Count() == 0)
                {
                    Console.WriteLine("Instance already existed.");
                }
                else
                {
                    cs.ListenAsSupervisor();
                    self = false;
                    //发送接收到的参数信息。
                    var task = Task.Run(() => cs.SendToHost(args));
                    if (!task.Wait(10000))
                    {
                        Console.WriteLine("Remote connection has been lost.");
                        IsStop = true;
                    }
                    while (!IsStop)
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                }
            }
        }

        /// <summary>
        /// 停止当前服务。
        /// </summary>
        public void Stop()
        {
            if (!IsStop)
            {
                cs.SendToHost(BasicServiceOrder.Stop);
            }
            if (self)
            {
                OnStop();
                hasStopped = true;
                if (IsTerminate) Environment.Exit(0);
            }
        }

        /// <summary>
        /// 发送消息给操作客户端。
        /// </summary>
        /// <returns><c>true</c>, if message was sent, <c>false</c> otherwise.</returns>
        /// <param name="msg">Message.</param>
        public bool SendMessage(String msg)
        {
            return cs.SendToSupervisor(msg.ToSupervisorReply());
        }

        private bool SendAndShow(String msg)
        {
            Console.WriteLine($"[INFO] {msg}");
            return SendMessage(msg);
        }
    }
}
