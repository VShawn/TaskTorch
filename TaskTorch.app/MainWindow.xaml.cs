using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using TaskTorch.app.Frame;
using TaskTorch.app.Presenter;

namespace TaskTorch.app
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public PageAddTask PageAddTask = new PageAddTask();
        public PageTaskList PageTaskList = new PageTaskList();
        public MainWindow()
        {
            InitializeComponent();
            Grid.DataContext = this;
            WinTaskHelper.TaskHelper.DeleteAllTmpTask();

            FrameTaskList.Navigate(PageTaskList);
            FrameAddTask.Navigate(PageAddTask);

            MainPresenter.Instance.ShowPage(MainPresenter.MainPage.TaskList);
            this.Closed += (sender, args) => { WinTaskHelper.TaskHelper.DeleteAllTmpTask(); };

        }

        private void BtnTaskList_OnClick(object sender, RoutedEventArgs e)
        {
            AddTaskPresenter.Instance.IsEditMode = false;
            MainPresenter.Instance.ShowPage(MainPresenter.MainPage.TaskList);
        }
        private void BtnNewTask_OnClick(object sender, RoutedEventArgs e)
        {
            AddTaskPresenter.Instance.IsEditMode = false;
            MainPresenter.Instance.ShowPage(MainPresenter.MainPage.AddTask);
        }
        private void BtnScript_OnClick(object sender, RoutedEventArgs e)
        {
            AddTaskPresenter.Instance.IsEditMode = false;
            MainPresenter.Instance.ShowPage(MainPresenter.MainPage.ScriptEnvironment);
        }
    }
}
