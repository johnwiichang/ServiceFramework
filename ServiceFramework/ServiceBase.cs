using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceFramework
{
    public class ServiceBase
    {
        private bool hasStopped = false;

        //是否是运行实例。
        private bool self = true;

        //服务中的通信服务。
        private CommunicationService cs;

        private ArgumentsService argsrv;

        public Boolean IsStop { get; private set; } = false;

        /// <summary>
        /// 当服务收到停止命令的时候执行的代码。
        /// </summary>
        public event Action OnStop;

        /// <summary>
        /// 当服务初始化时执行的代码。
        /// </summary>
        public event Action OnStart;

        /// <summary>
        /// 当收到命令参数的时候响应这些参数的代码块。
        /// </summary>
        public event Action<String[]> ReceivedArgument;

        /// <summary>
        /// 获取或者设定服务命令的标记。
        /// </summary>
        public String Flag { get { return BasicServiceOrder.Flag; } set { BasicServiceOrder.Flag = value; } }

        public ShellType Shell { get { return argsrv.Shell; } set { argsrv.Shell = value; } }

        public ServiceBase()
        {
            argsrv = new ArgumentsService();
            argsrv.Shell = ShellType.PowerShell;
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
                    cs.SendToSupervisor(SupervisorEventArgs.Reply("Stopping..."));
                    IsStop = true;
                    Stop();
                    cs.SendToSupervisor(SupervisorEventArgs.Reply("Service has been stopped."));
                }
                if (obj.Order == Order.terminate)
                {
                    OnStop += () => Environment.Exit(0);
                }
                if (obj.Order == Order.start || obj.Order == Order.restart)
                {
                    if (hasStopped)
                    {
                        cs.SendToSupervisor(SupervisorEventArgs.Reply("Starting..."));
                        var ex = InvokeSomethingWithoutWatching(() => Start());
                        if (ex != "")
                        {
                            cs.SendToSupervisor(SupervisorEventArgs.Reply(ex));
                        }
                        cs.SendToSupervisor(SupervisorEventArgs.Reply("Start successfully."));
                    }
                    else
                    {
                        cs.SendToSupervisor(SupervisorEventArgs.Reply("Already ran."));
                    }
                }
                cs.SendToSupervisor(SupervisorEventArgs.Answer("Done."));
                ReceivedArgument(obj.Arguments);
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
                    if (Utils.DetectPlatform() == Platform.Windows)
                    {
                        //如果是Windows平台，直接开始侦听。
                        cs.ListenAsHost();
                    }
                    else
                    {
                        //其他平台需要尝试发送消息
                        var test = Task.Run(() =>
                        {
                            cs.SendToHost(BasicServiceOrder.Test);
                            return true;
                        });
                        if (!test.Wait(1000))
                        {
                            //如果没有回音
                            cs.ListenAsHost();
                        }
                        else
                        {
                            //如果有回音，则表示实例已存在。
                            throw new Exception("Instance Already Existed.");
                        }
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
                        while (!IsStop && input.ToLower() != "stop")
                        {
                            input = Console.ReadLine();
                            //注意在这里，终端只接收数据参数。
                            if (input != "") ReceivedArgument(argsrv.Generate(input));
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
            }
        }

        /// <summary>
        /// 肮脏的写法，调用此方法将会忽略在调用过程中产生的一切错误。
        /// </summary>
        /// <param name="act"></param>
        /// <returns></returns>
        private static String InvokeSomethingWithoutWatching(Action act)
        {
            try
            {
                act();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
