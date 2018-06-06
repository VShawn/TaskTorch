using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
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
            this.Visibility = Visibility.Hidden;
            this.ShowInTaskbar = false;
            //RunCmd("ping 172.20.65.150");
            //RunPowershell("ping1 172.20.65.150");
            RunTask(taskName);
        }

        public void RunTask(string taskName)
        {
            try
            {
                var ymlPath = System.Environment.CurrentDirectory + "\\tasks\\" + taskName + "\\" + taskName + ".yml";
                if (File.Exists(ymlPath))
                {
                    var yml = File.ReadAllText(ymlPath);
                    var t = TaskSimpleFactory.CreateTask(yml);
                    if (t == null)
                        return;
                    var result = t.Excute();

                    if (result == TaskStatus.Failure)
                    {
                        // 检查是否需要重试任务
                        var ts = t.NeedRetry();
                        while (ts >= 0)
                        {
                            Thread.Sleep(ts * 1000);
                            t.Excute();
                            ts = t.NeedRetry();
                        }
                    }

                    // 判断是否有下一步执行任务
                    var next = t.GetNexTaskName();
                    if (string.IsNullOrEmpty(next))
                        return;
                    RunTask(next);
                }
                else
                {
                    MessageBox.Show(ymlPath + "不存在");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
