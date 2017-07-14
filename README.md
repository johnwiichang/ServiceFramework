# ServiceFramework
用于构建服务程序的简单框架——使用 .NET Core 构建跨平台的服务程序。

# 概述
.NET Core 提供多种平台可选，但是 .NET Core 对于服务应用程序的开发不如 .NET Framework 的 Windows 服务开发那么便利。使用 ServiceFramework 可以轻松迁移到 .NET Core。

# QuickStart
## 构建服务
参考 TestService.cs 文件，使用 ServiceBase 类构建一个服务程序。

> 先决条件：如果需要自定义服务操作标识，请在构造函数内定义 `Flag` 属性，否则默认标识为 ```-OpSrv```。

分别完成方法体：

    public override void ReceivedArguments(string[] args)
    public override void OnStop()
    public override void OnStart()

在 ```Main``` 方法里，请将参数形参传入 ```Start``` 方法：

    new TestService().Start(args);

> 注意：当且仅当程序启动后，服务标识才会被识别！

## 操作服务

> 注意：服务执行成功后，可以在命令行内键入交给业务逻辑处理的数据参数信息，这里不接收服务操作参数。

> 还请注意：默认情况服务使用 PowerShell 风格的参数处理在服务命令行输入的字符串。如果需要修改为 Terminal 风格，请设定 ```Shell``` 属性。

带参数执行，请在第一个参数（省略服务操作标识）或者第一二个参数（不省略）说明对服务的操作。服务框架将会处理掉针对服务操作的参数，且服务操作参数不会进入业务逻辑。

参考：

    dotnet yourservice.dll [Flag] Action [arg0] [arg1] ...

Action：

 - 停止：Stop
 - 开始：Start
 - 重启：ReStart
 - 终止：Terminate
 - 忽略：NoAction
 - 测试：Test（请不要使用）

> 还请注意：上述指令是在有服务运行的时候对服务的操作！

> 如果你的程序的数据参数可能为 Action 中的一个单词，建议带标识执行。或者确保输入时第一个输入的参数和第二个输入的参数与 Action 中的单词不冲突。如果无法避免，尝试使用 Flag NoAction（例如 ```-OpSrv NoAction```）彻底回避。

参数被程序收到后，服务操作将会反馈操作进程，数据操作的输出不会回传。

# 有一些补充

为了程序在各个平台的一致性，实现时选择了 dotnet Core 在各个平台都完整的方法。