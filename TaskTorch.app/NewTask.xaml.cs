using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32.TaskScheduler;
using TaskTorch.app.Class;
using TaskTorch.Loader.Model;

namespace TaskTorch.app
{
    /// <summary>
    /// NewTask.xaml 的交互逻辑
    /// </summary>
    public partial class NewTask : Window
    {
        public bool IsEdit = false;
        public NewTask()
        {
            IsEdit = false;
            InitializeComponent();

            TbRetries.Text = "0";
            TbRetryDelaySecond.Text = "0";
        }
        public NewTask(string taskName)
        {
            IsEdit = true;
            InitializeComponent();

            var t = TaskSimpleFactory.GetTask(taskName) as TaskV20180602;
            if (t == null)
            {
                MessageBox.Show(taskName + " 不存在！");
                Close();
            }

            TbTaskName.Text = t.TaskName;
            TbDescription.Text = t.TaskDescription;
            TbSuccessFlagString.Text = t.SuccessFlag;
            TbTaskAfterSuccess.Text = t.TaskAfterSuccess;
            TbTaskAfterFailure.Text = t.TaskName;
            TbScriptPath.Text = t.TaskCmd;
            TbRetries.Text = t.Retries.ToString();
            TbRetryDelaySecond.Text = t.RetryDelaySecond.ToString();
        }

        private void ButtonNext_OnClick(object sender, RoutedEventArgs e)
        {
            var taskName = TbTaskName.Text;
            try
            {
                var t = new TaskV20180602
                {
                    TaskName = TbTaskName.Text,
                    TaskDescription = TbDescription.Text,
                    SuccessFlag = TbSuccessFlagString.Text,
                    TaskAfterSuccess = TbTaskAfterSuccess.Text,
                    TaskAfterFailure = TbTaskAfterFailure.Text,
                    Retries = int.Parse(TbRetries.Text),
                    RetryDelaySecond = int.Parse(TbRetryDelaySecond.Text),
                    TaskType = TaskType.Cmd,
                    TaskCmd = TbScriptPath.Text,
                };

                if (string.IsNullOrEmpty(t.TaskName) || string.IsNullOrEmpty(t.TaskCmd))
                    return;
                if (CbRunWhenNoUser.IsChecked == true)
                {
                    if (string.IsNullOrEmpty(TbPass.Password) || string.IsNullOrEmpty(TbUserName.Text))
                        return;
                }



                if (Directory.Exists(t.GetTaskFolderPath()) &&
                WinTaskHelper.TaskHelper.TaskExists(t.TaskName) &&
                MessageBox.Show("计划任务[" + t.TaskName + "]已存在，要覆盖吗？", "警告", MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                // delete previous task
                WinTaskHelper.TaskHelper.DeleteTask(t.TaskName);
                if (Directory.Exists(t.GetTaskFolderPath()) && !IsEdit)
                    Directory.Delete(t.GetTaskFolderPath(), true);


                // make cmd
                if (File.Exists(TbScriptPath.Text))
                {
                    FileInfo fi = new FileInfo(TbScriptPath.Text);
                    if (fi.Extension == ".ps1")
                        t.TaskType = TaskType.Powershell;
                    else if (fi.Extension == ".py")
                        t.TaskCmd = "python " + t.TaskCmd;
                }

                // Get the service on the local machine
                using (TaskService ts = new TaskService())
                {
                    // Create a new task int tmp folder
                    WinTaskHelper.TaskHelper.DeleteTmpTask(taskName);
                    var task = WinTaskHelper.TaskHelper.AddTmpTask(taskName, t.TaskDescription, taskName,
                        new TimeTrigger()
                        {
                            StartBoundary = DateTime.Now + TimeSpan.FromHours(1),
                            Enabled = false
                        }, TbUserName.Text, TbPass.Password);
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
                        // if ok, move tmp task to storage folder
                        if (!Directory.Exists(t.GetTaskFolderPath()))
                            Directory.CreateDirectory(t.GetTaskFolderPath());
                        string ymlstr = t.ToYmlString();
                        File.WriteAllText(t.GetTaskFolderPath() + "\\" + taskName + ".yml", ymlstr);
                        WinTaskHelper.TaskHelper.DeleteTask(taskName);
                        WinTaskHelper.TaskHelper.AddTask(taskName, task, TbUserName.Text, TbPass.Password);
                        WinTaskHelper.TaskHelper.DeleteTmpTask(taskName);

                        Close();
                    }
                    else
                    {
                        // cancel or failed -> delete
                        WinTaskHelper.TaskHelper.DeleteTmpTask(taskName);
                        if (!IsEdit)
                        {
                            WinTaskHelper.TaskHelper.DeleteTask(taskName);
                            if (Directory.Exists(t.GetTaskFolderPath()))
                                Directory.Delete(t.GetTaskFolderPath(), true);
                        }
                    }
                }

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                MessageBox.Show(exception.Message);
            }
            finally
            {
                WinTaskHelper.TaskHelper.DeleteTmpTask(taskName);
            }
        }

        private void CbRunWhenNoUser_OnClick(object sender, RoutedEventArgs e)
        {
            if (CbRunWhenNoUser.IsChecked == true)
            {
                TbPass.IsEnabled = true;
                TbUserName.IsEnabled = true;
                TbPass.Password = "";
                TbUserName.Text = "";
            }
            else
            {
                TbPass.IsEnabled = false;
                TbUserName.IsEnabled = false;
                TbPass.Password = "";
                TbUserName.Text = "";
            }
        }

        private void BtnSelectScript_OnClick(object sender, RoutedEventArgs e)
        {
            var fi = FileHelper.OpenFile();
            TbScriptPath.Text = fi.FullName;
        }
    }
}
