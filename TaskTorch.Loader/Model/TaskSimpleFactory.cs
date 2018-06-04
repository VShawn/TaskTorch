using System.IO;
using SharpYaml.Serialization;

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
            }
            return null;
        }
    }
}
