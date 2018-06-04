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
using TaskTorch.Loader.Model;

namespace TaskTorch.app
{
    /// <summary>
    /// NewTask.xaml 的交互逻辑
    /// </summary>
    public partial class NewTask : Window
    {
        public NewTask()
        {
            InitializeComponent();
        }

        private void ButtonNext_OnClick(object sender, RoutedEventArgs e)
        {
            var t = new TaskV20180602
            {
                TaskName = TbTaskName.Text,
                TaskDescription = TbDescription.Text,
                SuccessFlag = TbSuccessFlagString.Text,
                TaskAfterSuccess = TbTaskAfterSuccess.Text,
                TaskAfterFailure = TbTaskAfterFailure.Text,
            };


            t.TaskType = TaskType.Cmd;
            t.TaskCmd = TbDescription.Text;
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
                var taskName = t.TaskName;
                WinTaskHelper.TaskHelper.DeleteTask(taskName);
                WinTaskHelper.TaskHelper.DeleteTmpTask(taskName);
                var task = WinTaskHelper.TaskHelper.AddTmpTask(taskName, t.TaskDescription, taskName, new TimeTrigger()
                {
                    StartBoundary = DateTime.Now + TimeSpan.FromHours(1),
                    Enabled = false
                });
                // Edit task and re-register if user clicks Ok
                TaskEditDialog editorForm = new TaskEditDialog();
                editorForm.Editable = true;
                editorForm.AvailableTabs = AvailableTaskTabs.General | AvailableTaskTabs.Triggers |
                                           AvailableTaskTabs.Conditions | AvailableTaskTabs.Settings |
                                           AvailableTaskTabs.Properties;
                editorForm.RegisterTaskOnAccept = false;
                editorForm.Initialize(task);
                //editorForm.Initialize(ts);
                // ** The four lines above can be replaced by using the full constructor
                // TaskEditDialog editorForm = new TaskEditDialog(t, true, true);
                if (editorForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // if ok, move task to storage folder
                    if (!Directory.Exists(System.Environment.CurrentDirectory + "\\tasks\\" + taskName))
                        Directory.CreateDirectory(System.Environment.CurrentDirectory + "\\tasks\\" + taskName);
                    string ymlstr = t.ToYmlString();
                    File.WriteAllText(System.Environment.CurrentDirectory + "\\tasks\\" + taskName + "\\" + taskName + ".ylm", ymlstr);
                    WinTaskHelper.TaskHelper.AddTask(taskName, task);
                    WinTaskHelper.TaskHelper.DeleteTmpTask(taskName);
                }
                else
                {
                    // cancel or failed -> delete
                    WinTaskHelper.TaskHelper.DeleteTmpTask(taskName);
                    WinTaskHelper.TaskHelper.DeleteTask(taskName);
                }
            }
        }
    }
}
