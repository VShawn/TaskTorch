using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32.TaskScheduler;
using TaskTorch.app.Class;
using TaskTorch.app.Dialog;
using TaskTorch.app.Presenter;
using TaskTorch.Loader.Model;

namespace TaskTorch.app.Frame
{
    /// <summary>
    /// PageAddTask.xaml 的交互逻辑
    /// </summary>
    public partial class PageAddTask : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsEdit { get; set; }= false;
        public PageAddTask()
        {
            IsEdit = false;
            InitializeComponent();
            Grid.DataContext = this;
        }
        private void BtnSetTigger_OnClick(object sender, RoutedEventArgs e)
        {
            // set the temp task name
            var wat = new WinAddTigger
            {
                Task = AddTaskPresenter.Instance.Task,
                IsEditMode = AddTaskPresenter.Instance.IsEditMode,
                TmpTaskName = DateTime.Now.ToString("yyyyMMdd_hhmmss.fff"),
                UserName = AddTaskPresenter.Instance.UserName,
                Password = AddTaskPresenter.Instance.Password,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };
            wat.ShowDialog();
            if (wat.Result)
            {
                AddTaskPresenter.Instance.TmpTaskName = wat.TmpTaskName;
                AddTaskPresenter.Instance.UserName = wat.UserName;
                AddTaskPresenter.Instance.Password = wat.Password;
            }
        }

        private void BtnAdd_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var trigger = WinTaskHelper.TaskHelper.GetTmpTask(AddTaskPresenter.Instance.TmpTaskName);
                if (trigger == null)
                {
                    MessageBox.Show("任务触发器未设置！");
                    return;
                }
                if (Directory.Exists(AddTaskPresenter.Instance.Task.GetTaskFolderPath()) &&
                    WinTaskHelper.TaskHelper.TaskExists(AddTaskPresenter.Instance.Task.TaskName) &&
                    MessageBox.Show("任务[" + AddTaskPresenter.Instance.Task.TaskName + "]已存在，要覆盖吗？", "警告", MessageBoxButton.YesNo,
                        MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                // create new folder
                if (!Directory.Exists(AddTaskPresenter.Instance.Task.GetTaskFolderPath()))
                    Directory.CreateDirectory(AddTaskPresenter.Instance.Task.GetTaskFolderPath());
                // save task to yml
                var ymlstr = AddTaskPresenter.Instance.Task.ToYmlString();
                File.WriteAllText(AddTaskPresenter.Instance.Task.GetTaskFolderPath() + "\\" + AddTaskPresenter.Instance.Task.TaskName + ".yml", ymlstr);
                // move tmp task to storage folder
                WinTaskHelper.TaskHelper.DeleteTask(AddTaskPresenter.Instance.Task.TaskName);
                WinTaskHelper.TaskHelper.AddTask(AddTaskPresenter.Instance.Task.TaskName, trigger, AddTaskPresenter.Instance.UserName, AddTaskPresenter.Instance.Password);
                WinTaskHelper.TaskHelper.DeleteTmpTask(AddTaskPresenter.Instance.Task.TaskName);
                WinTaskHelper.TaskHelper.DeleteAllTmpTask();

                AddTaskPresenter.Instance.Task = new TaskV20180602();
                MainPresenter.Instance.ShowPage(MainPresenter.MainPage.TaskList);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);

                if (Directory.Exists(AddTaskPresenter.Instance.Task.GetTaskFolderPath()) && !IsEdit)
                    Directory.Delete(AddTaskPresenter.Instance.Task.GetTaskFolderPath(), true);
                MessageBox.Show(exception.Message);
            }
        }

        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            MainPresenter.Instance.ShowPage(MainPresenter.MainPage.TaskList);
        }
    }
}
