using System;
using System.IO;
using System.Threading;
using SharpYaml.Serialization;

namespace TaskTorch.Loader.Model
{
    public class TaskV20180602 : ITask
    {
        public TaskStatus Excute()
        {
            string retInfo = string.Empty;
            try
            {
                switch (TaskType)
                {
                    case TaskType.Cmd:
                    {
                        retInfo = Runner.Runner.RunCmd(TaskCmd);
                        break;
                    }
                    case TaskType.Powershell:
                    {
                        retInfo = Runner.Runner.RunCmd(TaskCmd);
                        break;
                    }
                    default:
                        throw new NotImplementedException("can not run with task type :" + TaskType);
                }

                if (string.IsNullOrEmpty(SuccessFlag))
                {
                    if (retInfo.IndexOf(SuccessFlag) >= 0)
                    {
                        return TaskStatus.Success;
                    }
                    else
                    {
                        return TaskStatus.Failure;
                    }
                }
                else
                {
                    return TaskStatus.Run;
                }
            }
            catch (Exception e)
            {
                // TODO 记录异常
                Console.WriteLine(e);
                throw e;
            }
        }

        public string ToYmlString()
        {
            var serializer = new Serializer();
            var yml = serializer.Serialize(new
            {
                Version = TaskV20180602.Version,
                TaskName = TaskName,
                TaskType = TaskType,
                TaskDescription = TaskDescription,
                TaskCmd = TaskCmd,
                TimeOut = TimeOut,
                SuccessFlag = SuccessFlag,
                TaskAfterSuccess = TaskAfterSuccess,
                TaskAfterFailure = TaskAfterFailure,
                Retries = Retries,
                RetryDelaySecond = RetryDelaySecond,
            });
            return yml;
        }

        public bool FromYmlString(string ymlString)
        {
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
                TaskAfterSuccess = mapping.Children[new YamlScalarNode(nameof(TaskAfterSuccess))].ToString();
                TaskAfterFailure = mapping.Children[new YamlScalarNode(nameof(TaskAfterFailure))].ToString();
                Retries = int.Parse(mapping.Children[new YamlScalarNode(nameof(Retries))].ToString());
                RetryDelaySecond = int.Parse(mapping.Children[new YamlScalarNode(nameof(RetryDelaySecond))].ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            TaskStatus = TaskStatus.NotRun;
            return true;

            //var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
            //foreach (var entry in mapping.Children)
            //{
            //    Console.WriteLine((YamlScalarNode)entry.Key.ToString() + " " + (YamlScalarNode)entry.Value.ToString());
            //}
        }

        public string GetVersion()
        {
            return Version;
        }

        public string GetNexTaskName()
        {
            switch (TaskStatus)
            {
                case TaskStatus.Success:
                case TaskStatus.Run:
                    return TaskAfterSuccess;
                case TaskStatus.Failure:
                case TaskStatus.Exception:
                    if (Retries > 0)
                    {
                        --Retries;
                        Thread.Sleep(RetryDelaySecond);
                        return TaskName;
                    }
                    return TaskAfterFailure;
                case TaskStatus.NotRun:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return string.Empty;
        }

        public const string Version = "20180602";
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



        public TaskStatus TaskStatus { get; set; }
    }
}
