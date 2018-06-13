using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TaskTorch.app
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {

        public App()
        {
            this.Startup += new StartupEventHandler(App_Startup);
        }


        public static MainWindow Window;
        private void ApplicationStart(object sender, StartupEventArgs e)
        {
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            // 检查配置文件是否存在

            {
                Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                Window = new MainWindow();
                Current.MainWindow = Window;
                Window.Show();
            }
        }
        void App_Startup(object sender, StartupEventArgs e)
        {
            var mutex = new System.Threading.Mutex(true, "TaskTorch.app", out var ret);
            if (!ret)
            {
                MessageBox.Show("已有程序在运行中！");
                Environment.Exit(0);
            }
        }
    }
}
