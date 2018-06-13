using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TaskTorch.app.Presenter;
using TaskTorch.Loader.Model;
using TaskStatus = System.Threading.Tasks.TaskStatus;

namespace TaskTorch.app.Frame
{
    /// <summary>
    /// PageTaskList.xaml 的交互逻辑
    /// </summary>
    public partial class PageTaskList : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly System.Timers.Timer _timer;

        public PageTaskList()
        {
            InitializeComponent();
            Grid.DataContext = this;
            try
            {
                Init();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                MessageBox.Show(e.Message);
            }

            _timer = new System.Timers.Timer(10000);
            _timer.Elapsed += (sender, args) => { Dispatcher.Invoke(callback: () => { Init(); }); };
            _timer.Interval = 1000 * 60 * 10;
            _timer.Enabled = true;
        }



        public List<ITask> Tasks { get; set; } = new List<ITask>();
        public void Init()
        {
            Tasks.Clear();
            LvTaskList.Items.Refresh();
            var folderPath = System.Environment.CurrentDirectory + "\\tasks\\";
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            var dis = Directory.GetDirectories(folderPath);
            foreach (var dpath in dis)
            {
                var di = new DirectoryInfo(dpath);
                var fpath = dpath + "\\" + di.Name + ".yml";
                if (!File.Exists(fpath))
                {
                    //Directory.Delete(dpath, true);
                    continue;
                }

                var t = TaskSimpleFactory.CreateTask(File.ReadAllText(fpath));
                if (t == null)
                {
                    continue;
                }
                Tasks.Add(t);
            }
            LvTaskList.Items.Refresh();
        }

        /// <summary>
        /// open new window to create new TaskTorch task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnNewTask_OnClick(object sender, RoutedEventArgs e)
        {
            AddTaskPresenter.Instance.IsEditMode = false;
            MainPresenter.Instance.ShowPage(MainPresenter.MainPage.AddTask);
            //Init();
        }

        private void BtnEditTask_OnClick(object sender, RoutedEventArgs e)
        {
            var curItem = ((ListBoxItem)LvTaskList.ContainerFromElement((Button)sender)).Content;
            var t = curItem as ITask;
            AddTaskPresenter.Instance.Task = t as TaskV20180602;
            AddTaskPresenter.Instance.IsEditMode = true;
            MainPresenter.Instance.ShowPage(MainPresenter.MainPage.AddTask);
            Init();
        }

        private void BtnDeleteTask_OnClick(object sender, RoutedEventArgs e)
        {
            var curItem = ((ListBoxItem)LvTaskList.ContainerFromElement((Button)sender)).Content;
            var t = curItem as ITask;
            var taskName = t.TaskName;
            if (MessageBox.Show("确定要删除计划任务[" + t.TaskName + "]吗？", "警告", MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                WinTaskHelper.TaskHelper.DeleteTmpTask(taskName);
                WinTaskHelper.TaskHelper.DeleteTask(taskName);
                if (Directory.Exists(t.GetTaskFolderPath()))
                    Directory.Delete(t.GetTaskFolderPath(), true);
                Init();
            }
        }

        private void BtnTestRunTask_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var curItem = ((ListBoxItem)LvTaskList.ContainerFromElement((Button)sender)).Content;
                var t = curItem as ITask;
                var taskName = t.TaskName;
                t = TaskSimpleFactory.GetTask(taskName);
                MessageBox.Show(t.GetExcuteInfo());
                Init();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                MessageBox.Show(exception.Message);
            }
        }

        private void BtnRunTask_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var curItem = ((ListBoxItem)LvTaskList.ContainerFromElement((Button)sender)).Content;
                var t = curItem as ITask;
                var taskName = t.TaskName;
                t = TaskSimpleFactory.GetTask(taskName);
                var result = t.Excute();

                if (result == Loader.Model.TaskStatus.Failure)
                {
                    // 检查是否需要重试任务
                    var ts = t.NeedRetry();
                    while (ts >= 0)
                    {
                        MessageBox.Show("执行失败，将在sleep" + ts + "s后重试");
                        Thread.Sleep(ts * 1000);
                        t.Excute();
                        ts = t.NeedRetry();
                    }
                }

                // 判断是否有下一步执行任务
                var next = t.GetNexTaskName();
                if (!string.IsNullOrEmpty(next))
                {
                    MessageBox.Show("执行结束，实际运行时下一步将执行：" + next);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                MessageBox.Show(exception.Message);
            }
            Init();
        }

        private void LvTaskList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var t = LvTaskList.SelectedItem as ITask;
            TbTaskDetial.Text = t?.GetDetail() ?? "...";
        }
    }
}
