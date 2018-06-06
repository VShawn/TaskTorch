using System;
using System.IO;
using System.Threading;
using ServiceStack.OrmLite;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace TaskTorch.Loader.Model
{
    public class TaskV20180602 : ITask
    {
        public TaskStatus Excute()
        {
            ExcuteInfos = new[] { "", "" };
            try
            {
                switch (TaskType)
                {
                    case TaskType.Cmd:
                        {
                            ExcuteInfos = Runner.Runner.RunCmd(TaskCmd);
                            break;
                        }
                    case TaskType.Powershell:
                        {
                            ExcuteInfos = Runner.Runner.RunPowershell(TaskCmd);
                            break;
                        }
                    default:
                        throw new NotImplementedException("can not run with task type :" + TaskType);
                }

                // 若设定了命令运行正确的标志位，则根据标志位判断是否允许正确。
                if (!string.IsNullOrEmpty(SuccessFlag))
                {
                    // 先截取命令的输出文本
                    if (ExcuteInfos[0].IndexOf(SuccessFlag) >= 0)
                        TaskStatus = TaskStatus.Success;
                    else
                        TaskStatus = TaskStatus.Failure;
                }
                else
                {
                    // 获得命令执行的返回码，为0时表示允许正确
                    if (int.TryParse(ExcuteInfos[1], out var result) && result == 0)
                        TaskStatus = TaskStatus.Success;
                    else
                        TaskStatus = TaskStatus.Failure;
                }
                // 记录执行情况
                AddLog();
            }
            catch (Exception e)
            {
                // 记录异常
                ExcuteInfos = new[] { e.Message, "" };
                AddLog();
                Console.WriteLine(e);
                throw e;
            }
            return TaskStatus;
        }

        public string GetExcuteInfo()
        {
            if (ExcuteInfos?.Length > 0)
                return ExcuteInfos[0];
            Excute();
            if (ExcuteInfos?.Length > 0)
                return ExcuteInfos[0];
            return "";
        }

        public string ToYmlString()
        {
            var serializer = new Serializer();
            var yml = serializer.Serialize(new
            {
                Version = TaskV20180602.Version,
                TaskName = TaskName,
                TaskType = TaskType.ToString(),
                TaskDescription = TaskDescription,
                TaskCmd = TaskCmd,
                TimeOut = TimeOut.ToString(),
                SuccessFlag = SuccessFlag,
                TaskAfterSuccess = TaskAfterSuccess,
                TaskAfterFailure = TaskAfterFailure,
                Retries = Retries.ToString(),
                RetryDelaySecond = RetryDelaySecond.ToString(),
            });
            return yml;
        }

        public bool FromYmlString(string ymlString)
        {
            TaskStatus = TaskStatus.NotRun;
            try
            {
                var input = new StringReader(ymlString);
                var yaml = new YamlStream();
                yaml.Load(input);
                var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
                TaskName = mapping.Children[new YamlScalarNode(nameof(TaskName))].ToString();
                TaskType = (TaskType)Enum.Parse(typeof(TaskType), mapping.Children[new YamlScalarNode(nameof(TaskType))].ToString());
                TaskDescription = mapping.Children[new YamlScalarNode(nameof(TaskDescription))].ToString();
                TaskCmd = mapping.Children[new YamlScalarNode(nameof(TaskCmd))].ToString();
                TimeOut = int.Parse(mapping.Children[new YamlScalarNode(nameof(TimeOut))].ToString());
                SuccessFlag = mapping.Children[new YamlScalarNode(nameof(SuccessFlag))].ToString();
                TaskAfterSuccess = mapping.Children[new YamlScalarNode(nameof(TaskAfterSuccess))].ToString();
                TaskAfterFailure = mapping.Children[new YamlScalarNode(nameof(TaskAfterFailure))].ToString();
                Retries = int.Parse(mapping.Children[new YamlScalarNode(nameof(Retries))].ToString());
                RetryDelaySecond = int.Parse(mapping.Children[new YamlScalarNode(nameof(RetryDelaySecond))].ToString());
            }
            catch (Exception e)
            {
                TaskStatus = TaskStatus.NotRun;
                ExcuteInfos = new[] { e.Message, "" };
                AddLog();
                Console.WriteLine(e);
                return false;
            }

            TaskStatus = TaskStatus.NotRun;
            return true;
        }

        public string GetVersion()
        {
            return Version;
        }

        public int NeedRetry()
        {
            if (TaskStatus != TaskStatus.Failure || Retries <= 0) return -1;
            --Retries;
            return RetryDelaySecond > 0 ? RetryDelaySecond : 0;
        }

        public string GetNexTaskName()
        {
            switch (TaskStatus)
            {
                case TaskStatus.Success:
                    return TaskAfterSuccess;
                case TaskStatus.Failure:
                case TaskStatus.Exception:
                    return TaskAfterFailure;
                case TaskStatus.NotRun:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return string.Empty;
        }

        public string GetTaskFolderPath()
        {
            return System.Environment.CurrentDirectory + "\\tasks\\" + TaskName;
        }

        public string GetDetail()
        {
            string ret = @"
任务名 = " + TaskName + @", 描述 = " + TaskDescription + @", 超时时间/秒 = " + TimeOut + @", 失败后重试次数 = " + Retries + @", 重试推延执行时间/秒 = " + RetryDelaySecond + @",
任务成功标志 = " + (SuccessFlag == "" ? "[cmd return 0]" : "[cmd output: “" + SuccessFlag + "”]") + @",
若任务成功则执行 = " + (TaskAfterSuccess == "" ? "null" : TaskAfterSuccess) + @", 任务失败则执行= " + (TaskAfterSuccess == "" ? "null" : TaskAfterSuccess) + @",
";
            return ret;
        }

        public OrmLiteConnectionFactory GetDbFactory()
        {
            var dbpath = GetTaskFolderPath() + "\\" + TaskName + ".db";
            var dbFactory = new OrmLiteConnectionFactory(dbpath, SqliteDialect.Provider);
            return dbFactory;
        }
        public void AddLog()
        {
            if (Directory.Exists(GetTaskFolderPath()))
            {
                // 写数据库
                var dbpath = GetTaskFolderPath() + "\\" + TaskName + ".db";
                using (var db = GetDbFactory().Open())
                {
                    if (!db.TableExists(nameof(TaskRunLog)))
                        db.CreateTable<TaskRunLog>();
                    var trl = new TaskRunLog
                    {
                        Date = DateTime.Now,
                        Msg = ExcuteInfos?[0],
                        Statu = TaskStatus
                    };
                    db.Insert(trl);
                }
                // 写文件
                var txtpath = GetTaskFolderPath() + "\\" + TaskName + ".log";
                using (var sw = new StreamWriter(txtpath, true))
                {
                    var line = DateTime.Now.ToString("s") + "," + TaskStatus.ToString() + "," + ExcuteInfos?[0].Replace("\r", " ").Replace(",", " ").Replace("\n", " ") + "," +
                               ExcuteInfos?[1];
                    sw.WriteLine(line);
                }
            }
        }


        public const string Version = "20180602";
        public string[] ExcuteInfos { get; set; }
        public TaskStatus TaskStatus { get; set; }


        public string TaskName { get; set; }
        public TaskType TaskType { get; set; }
        public string TaskDescription { get; set; }
        public string TaskCmd { get; set; }
        public int TimeOut { get; set; } = 0;
        public string SuccessFlag { get; set; }
        public string TaskAfterSuccess { get; set; }
        public string TaskAfterFailure { get; set; }
        public int Retries { get; set; } = 0;
        public int RetryDelaySecond { get; set; } = 0;
    }
}
