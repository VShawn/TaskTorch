using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Xml.Serialization;

namespace TaskTorch.app.Presenter
{
    [Serializable]
    public class Script:INotifyPropertyChanged
    {
        private string _extension;
        private string _CmdTemplate;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 脚本的后缀名如“.py”
        /// </summary>
        public string Extension
        {
            get => _extension;
            set => _extension = value.Trim();
        }

        /// <summary>
        /// 脚本执行命令模板，例如模板“python %scriptname%.py”，运行时将变为“python script.py”
        /// </summary>
        public string CmdTemplate
        {
            get => _CmdTemplate;
            set => _CmdTemplate = value.Trim();
        }
    }

    public class ScriptEnvironmentsPresenter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region 实现单例
        private static volatile ScriptEnvironmentsPresenter _ret;
        private static readonly object Obj = new object();
        private List<Script> _scripts = new List<Script>();

        public static ScriptEnvironmentsPresenter Instance
        {
            get
            {
                if (null == _ret)
                {
                    lock (Obj)
                    {
                        if (null == _ret)
                        {
                            _ret = new ScriptEnvironmentsPresenter();
                            _ret.Scripts = _ret.ReserializeMethod();
                        }
                    }

                }
                return _ret;
            }
        }
        #endregion

        public List<Script> Scripts
        {
            get => _scripts;
            set
            {
                _scripts = value;
                SerializeMethod(_scripts);
            }
        }

        //序列化操作
        public static void SerializeMethod(List<Script> list)
        {
            try
            {
                var x = new XmlSerializer(typeof(List<Script>));
                var sb = new StringBuilder();
                var sw = new StringWriter(sb);
                x.Serialize(sw, list);
                var s = sb.ToString();
                File.WriteAllText("Scripts.xml", s);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                MessageBox.Show(e.Message);
            }
        }

        public const string SCRIPT_NAME = "%script_name%";
        //反序列化操作
        public List<Script> ReserializeMethod()
        {
            try
            {

                if (File.Exists("Scripts.xml"))
                {
                    var s = File.ReadAllText("Scripts.xml");
                    using (StringReader sr = new StringReader(s))
                    {
                        var x = new XmlSerializer(typeof(List<Script>));
                        var ret = (List<Script>)x.Deserialize(sr);
                        if (ret.Count > 0)
                            return ret;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                MessageBox.Show(e.Message);
            }

            var newlist = new List<Script>
            {
                new Script
                {
                    Extension = ".exe",
                    CmdTemplate = SCRIPT_NAME + ".exe",
                },
                new Script
                {
                    Extension = ".py",
                    CmdTemplate = "python " + SCRIPT_NAME + ".py",
                },
                new Script
                {
                    Extension = ".r",
                    CmdTemplate = "R CMD BATCH --args " + SCRIPT_NAME + ".r",
                },
                new Script
                {
                    Extension = ".sql",
                    CmdTemplate = "sqlcmd -S . -U sa -P 123 -d test -i " + SCRIPT_NAME + ".sql",
                },
                new Script
                {
                    Extension = ".php",
                    CmdTemplate = "php " + SCRIPT_NAME + ".php",
                },
                new Script
                {
                    Extension = ".m",
                    CmdTemplate = "matlab -nosplash -nodesktop -r " + SCRIPT_NAME + "",
                }
            };
            return newlist;
        }
    }
}
