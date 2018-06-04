﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32.TaskScheduler;
using TaskTorch.Loader.Model;
using TaskTorch.WinTaskHelper;

namespace TaskTorch.app
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //// Get the service on the local machine
            //using (TaskService ts = new TaskService())
            //{
            //    // Create a new task
            //    const string taskName = "Test";
            //    Task t = ts.AddTask(taskName,
            //        new TimeTrigger()
            //        {
            //            StartBoundary = DateTime.Now + TimeSpan.FromHours(1),
            //            Enabled = false
            //        },
            //        new ExecAction("notepad.exe", "c:\\test.log", "C:\\"));

            //    // Edit task and re-register if user clicks Ok
            //    TaskEditDialog editorForm = new TaskEditDialog();
            //    editorForm.Editable = true;
            //    editorForm.AvailableTabs = AvailableTaskTabs.General | AvailableTaskTabs.Triggers | AvailableTaskTabs.Conditions | AvailableTaskTabs.Settings | AvailableTaskTabs.Properties | AvailableTaskTabs.RunTimes | AvailableTaskTabs.History;
            //    editorForm.RegisterTaskOnAccept = true;
            //    editorForm.Initialize(t);
            //    //editorForm.Initialize(ts);
            //    // ** The four lines above can be replaced by using the full constructor
            //    // TaskEditDialog editorForm = new TaskEditDialog(t, true, true);
            //    editorForm.ShowDialog();
            //}


            Init();
        }
        public Dictionary<ITask, string> Tasks = new Dictionary<ITask, string>();
        public void Init()
        {
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

                if (TaskHelper.TaskExists(di.Name))
                {
                    Tasks.Add(t, "");
                }
                else
                {
                    Tasks.Add(t, di.Name);
                }
            }
        }

        /// <summary>
        /// open new window to create new TaskTorch task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnNewTask_OnClick(object sender, RoutedEventArgs e)
        {
            NewTask nt =new NewTask();
            nt.Show();
        }
    }
}