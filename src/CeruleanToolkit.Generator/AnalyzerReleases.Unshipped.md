### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
CTK001  | CeruleanToolkit.SourceGenerator | Error | ServiceType 必须是接口
CTK002  | CeruleanToolkit.SourceGenerator | Error | 类必须实现指定的服务接口
CTK003  | CeruleanToolkit.SourceGenerator | Error | 服务类名必须以 Service 结尾
CTK004  | CeruleanToolkit.SourceGenerator | Error | 服务类必须可实例化
CTK005  | CeruleanToolkit.SourceGenerator | Error | 服务类必须有公共构造函数
CTK006  | CeruleanToolkit.SourceGenerator | Error | 不支持的服务泛型组合
CTK101  | CeruleanToolkit.SourceGenerator | Error | 命令类名必须以 Command 结尾
CTK102  | CeruleanToolkit.SourceGenerator | Error | 命令类必须继承 System.CommandLine.Command
CTK103  | CeruleanToolkit.SourceGenerator | Error | 子命令的父类型必须是命令类型
CTK104  | CeruleanToolkit.SourceGenerator | Error | 不能同时标记 Command 和 SubCommand
CTK105  | CeruleanToolkit.SourceGenerator | Error | 命令父子关系存在循环
CTK106  | CeruleanToolkit.SourceGenerator | Error | 命令类必须可实例化
CTK107  | CeruleanToolkit.SourceGenerator | Error | 命令类必须有公共构造函数
CTK108  | CeruleanToolkit.SourceGenerator | Error | 父类型没有标记为命令
CTK109  | CeruleanToolkit.SourceGenerator | Error | 子命令平台不能比父命令更宽
