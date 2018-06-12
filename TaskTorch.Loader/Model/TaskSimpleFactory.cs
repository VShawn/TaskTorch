using System;
using System.IO;
using YamlDotNet.RepresentationModel;

namespace TaskTorch.Loader.Model
{
    public class TaskSimpleFactory
    {
        public static ITask CreateTask(string ymlString)
        {
            var input = new StringReader(ymlString);
            var yaml = new YamlStream();
            yaml.Load(input);
            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
            var node = new YamlScalarNode("Version");
            if (mapping.Children.ContainsKey(node))
            {
                var v = mapping.Children[node].ToString();
                if (v == TaskV20180602.Version)
                {
                    var task = new TaskV20180602();
                    if (task.FromYmlString(ymlString))
                        return task;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("no version match");
                }
            }
            return null;
        }

        public static ITask GetTask(string taskName)
        {
            var it = new TaskV20180602 {TaskName = taskName};
            var path = it.GetTaskFolderPath();
            var fpath = path + "\\" + taskName + ".yml";
            if (!File.Exists(fpath))
            {
                return null;
            }
            var t = TaskSimpleFactory.CreateTask(File.ReadAllText(fpath));
            return t;
        }
    }
}
