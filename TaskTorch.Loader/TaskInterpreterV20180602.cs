using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpYaml.Serialization;

namespace TaskLoader
{
    public class TaskInterpreterV20180602 : ITaskInterpreter
    {
        public void Excute()
        {
            throw new NotImplementedException();
        }

        public string ToYmlString()
        {
            var serializer = new Serializer();
            var yml = serializer.Serialize(new
            {
                Version = TaskInterpreterV20180602.Version,
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
                TaskType = (TaskLoader.TaskType)Enum.Parse(typeof(TaskLoader.TaskType), mapping.Children[new YamlScalarNode(nameof(TaskType))].ToString());
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

            return true;

            //var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
            //foreach (var entry in mapping.Children)
            //{
            //    Console.WriteLine((YamlScalarNode)entry.Key.ToString() + " " + (YamlScalarNode)entry.Value.ToString());
            //}
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
    }
}
