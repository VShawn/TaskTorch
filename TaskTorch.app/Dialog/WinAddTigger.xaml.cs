using System;
using System.IO;
using System.Windows;
using Microsoft.Win32.TaskScheduler;
using TaskTorch.Loader.Model;

namespace TaskTorch.app.Dialog
{
    /// <summary>
    /// WinAddTigger.xaml 的交互逻辑
    /// </summary>
    public partial class WinAddTigger : Window
    {
        public WinAddTigger()
        {
            InitializeComponent();
        }
        private void WinAddTigger_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Task == null || string.IsNullOrEmpty(TmpTaskName))
            {
                MessageBox.Show("任务名不能为空！");
                Close();
                return;
            }
            if (IsEditMode == null)
            {
                MessageBox.Show("任务编辑模式未设置！");
                Close();
                return;
            }
        }
        public ITask Task { get; set; }

        public string TmpTaskName { get; set; }
        /// <summary>
        /// is add new task or edit old task
        /// </summary>
        public bool? IsEditMode { get; set; } = null;
        /// <summary>
        /// is add new task or edit old task
        /// </summary>
        public bool Result { get; private set; } = false;

        public string UserName { get; set; }
        public string Password { get; set; }

        private void CbRunWhenNoUser_OnClick(object sender, RoutedEventArgs e)
        {
            if (CbRunWhenNoUser.IsChecked == true)
            {
                TbPass.IsEnabled = true;
                TbUserName.IsEnabled = true;
                TbPass.Password = Password;
                TbUserName.Text = UserName;
            }
            else
            {
                TbPass.IsEnabled = false;
                TbUserName.IsEnabled = false;
                TbPass.Password = "";
                TbUserName.Text = "";
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (Task == null || string.IsNullOrEmpty(TmpTaskName))
            {
                MessageBox.Show("任务名不能为空！");
                Close();
            }
            if (IsEditMode == null)
            {
                MessageBox.Show("任务编辑模式为设置！");
                Close();
            }
            this.Visibility = Visibility.Collapsed;
            this.ShowInTaskbar = false;
            try
            {
                using (var ts = new TaskService())
                {
                    WinTaskHelper.TaskHelper.DeleteTmpTask(TmpTaskName);
                    var tmptask = WinTaskHelper.TaskHelper.GetTask(Task.TaskName);
                    Task task = null;
                    if (IsEditMode == true && tmptask != null)
                    {
                        task = WinTaskHelper.TaskHelper.AddTmpTask(TmpTaskName, Task.TaskDescription, Task.TaskName, tmptask.Definition.Triggers, TbUserName.Text, TbPass.Password);
                    }
                    else
                    {
                        // Create a new task in tmp folder
                        task = WinTaskHelper.TaskHelper.AddTmpTask(TmpTaskName, Task.TaskDescription, Task.TaskName,
                        new TimeTrigger()
                        {
                            StartBoundary = DateTime.Now + TimeSpan.FromHours(1),
                            Enabled = false
                        },
                        TbUserName.Text, TbPass.Password);
                    }

                    // Edit task and re-register if user clicks Ok
                    var editorForm = new TaskEditDialog
                    {
                        Editable = true,
                        AvailableTabs = AvailableTaskTabs.General | AvailableTaskTabs.Triggers |
                                        AvailableTaskTabs.Conditions | AvailableTaskTabs.Settings |
                                        AvailableTaskTabs.Properties,
                        RegisterTaskOnAccept = false
                    };
                    editorForm.Initialize(task);
                    if (editorForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        // save psw of windows
                        if (!string.IsNullOrEmpty(TbPass.Password) && !string.IsNullOrEmpty(TbUserName.Text))
                        {
                            Password = TbPass.Password;
                            UserName = TbUserName.Text;
                        }
                        this.Result = true;
                        Close();
                        return;
                    }
                    else
                    {
                        // cancel or failed -> delete
                        WinTaskHelper.TaskHelper.DeleteTmpTask(TmpTaskName);
                        this.Result = false;
                        Close();
                        return;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                MessageBox.Show(exception.Message);
                //throw;
            }
        }
    }
}
