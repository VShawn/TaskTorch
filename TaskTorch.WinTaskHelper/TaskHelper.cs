using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32.TaskScheduler;

namespace TaskTorch.WinTaskHelper
{
    public static class TaskHelper
    {
        private const string FolderName = nameof(TaskTorch);

        private static string ExePath = System.Environment.CurrentDirectory.Replace(":", "").Replace("\\", "_")
            .Replace("//", "_").Replace(@"\", "_").Replace("/", "_").Replace("?", "_").Replace("*", "_").Replace("\"", "_").Replace("<", "_").Replace(">", "_").Replace("|", "_");

        private static object locker = new object();


        private static string GetTaskFolderName()
        {
            return FolderName + "-on-" + ExePath;
        }
        private static void BuildFolder()
        {
            using (var ts = new TaskService())
            {
                if (!ts.RootFolder.SubFolders.Exists(GetTaskFolderName()))
                    lock (locker)
                    {
                        try
                        {
                            ts.RootFolder.CreateFolder(GetTaskFolderName());
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
            }
        }


        /// <summary>
        /// 列出本管理程序的所有计划任务
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public static List<Task> EnumAllTasks(string folderName = "")
        {
            BuildFolder();
            if (string.IsNullOrEmpty(folderName))
                folderName = GetTaskFolderName();
            using (var ts = new TaskService())
            {
                foreach (var sfld in ts.RootFolder.SubFolders)
                {
                    if (sfld.Name == folderName)
                        return new List<Task>(sfld.Tasks);
                }
            }
            return null;
        }

        /// <summary>
        /// 检查某个计划任务是否存在
        /// </summary>
        /// <param name="taskName"></param>
        /// <returns></returns>
        public static bool TaskExists(string taskName)
        {
            using (var ts = new TaskService())
            {
                // 检查目录是否存在
                if (!ts.RootFolder.SubFolders.Exists(GetTaskFolderName()))
                    return false;
                // 检查task是否存在
                var tf = ts.RootFolder.SubFolders[GetTaskFolderName()];
                if (tf.Tasks.Exists(taskName))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 添加一个计划任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="taskName"></param>
        /// <param name="taskDescription"></param>
        /// <param name="scriptName"></param>
        /// <param name="tigger"></param>
        /// <returns></returns>
        public static bool AddTask<T>(string taskName, string taskDescription, string scriptName, T tigger,string userName = "",string password = "") where T : Trigger
        {
            // 检查runner是否存在
            string runnerPath = ExePath + @"\" + nameof(TaskTorch) + "." + "Runner" + ".exe";
            if (!File.Exists(runnerPath))
                throw new FileNotFoundException("Runner.exe并不存在于路径" + ExePath, runnerPath);

            BuildFolder();

            // 检查task是否存在
            if (TaskExists(taskName))
                return false;
            using (var ts = new TaskService())
            {
                var tf = ts.RootFolder.SubFolders[GetTaskFolderName()];
                 
                // 创建task

                // Create a new task definition and assign properties
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = taskDescription;
                td.Principal.LogonType = TaskLogonType.InteractiveTokenOrPassword;

                // Add a trigger
                td.Triggers.Add(tigger);

                // Add an action that will launch Notepad whenever the trigger fires
                td.Actions.Add(new ExecAction(runnerPath, scriptName, null));


                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                    tf.RegisterTaskDefinition(taskName, td);
                else
                    tf.RegisterTaskDefinition(taskName, td, TaskCreation.Create, userName, password,
                        TaskLogonType.InteractiveTokenOrPassword);
            }
            return true;
        }

        public static void DeleteTask(string taskName)
        {
            // 检查task是否存在
            if (TaskExists(taskName))
                return;

            using (var ts = new TaskService())
            {
                var tf = ts.RootFolder.SubFolders[GetTaskFolderName()];
                tf.DeleteTask(taskName, false);
            }
        }
        public static void DisableTask(string taskName)
        {
            // 检查task是否存在
            if (TaskExists(taskName))
                return;

            using (var ts = new TaskService())
            {
                var tf = ts.RootFolder.SubFolders[GetTaskFolderName()];
                tf.Tasks[taskName].Enabled = false;
            }
        }
    }
}
