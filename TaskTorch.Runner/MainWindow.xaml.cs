using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using TaskTorch.Loader.Model;

namespace TaskTorch.Runner
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(string taskName)
        {
            InitializeComponent();
            //RunCmd("ping 172.20.65.150");
            //RunPowershell("ping1 172.20.65.150");
            RunTask(taskName);
        }

        public void RunTask(string taskName)
        {
            try
            {
                var ymlPath = System.Environment.CurrentDirectory + "\\task\\" + taskName + "\\" + taskName + ".yml";
                if (File.Exists(ymlPath))
                {
                    string yml = File.ReadAllText(ymlPath);
                    var t = TaskSimpleFactory.CreateTask(yml);
                    if (t == null)
                        return;
                    var result = t.Excute();

                    // TODO 记录执行情况
                    switch (result)
                    {
                        case TaskStatus.Failure:
                            break;
                        case TaskStatus.Success:
                            break;
                        case TaskStatus.Run:
                            break;
                        case TaskStatus.Exception:
                            break;
                        case TaskStatus.NotRun:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    var next = t.GetNexTaskName();
                    if (string.IsNullOrEmpty(next))
                        return;
                    else if (next == taskName)
                    {
                        t.Excute();
                    }
                    else
                    {
                        RunTask(next);
                    }
                }
            }
            catch (Exception e)
            {
                // TODO 记录异常
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
