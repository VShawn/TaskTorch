using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32.TaskScheduler;

namespace TaskTorch.WinTaskHelper
{
    public static class TaskHelper
    {
        private const string FolderName = nameof(TaskTorch);

        private static readonly string ExePath = System.Environment.CurrentDirectory.Replace(":", "").Replace("\\", "_")
            .Replace("//", "_").Replace(@"\", "_").Replace("/", "_").Replace("?", "_").Replace("*", "_").Replace("\"", "_").Replace("<", "_").Replace(">", "_").Replace("|", "_");



        /// <summary>
        /// return a task name conbined with app name and exe path
        /// </summary>
        /// <returns></returns>
        private static string GetTaskFolderName()
        {
            return FolderName + "-on-" + ExePath;
        }


        /// <summary>
        /// a lock for thread safety
        /// </summary>
        private static readonly object Locker = new object();
        /// <summary>
        /// create new folder for this app in windows task scheduler.
        /// </summary>
        private static void BuildFolder()
        {
            using (var ts = new TaskService())
            {
                if (!ts.RootFolder.SubFolders.Exists(GetTaskFolderName()))
                    lock (Locker)
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
        /// enum all task in windows task scheduler which is belong to this app. 
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
        /// check if the 'taskName' is existed in windows task scheduler
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

        public static Task AddTmpTask<T>(string taskName, string taskDescription, string scriptName, T tigger,
            string userName = null, string password = null) where T : Trigger
        {
            taskName = nameof(TaskTorch) + "_TmpTask_" + taskName;
            // 检查runner是否存在
            var runnerPath = System.Environment.CurrentDirectory + @"\" + nameof(TaskTorch) + "." + "Runner" + ".exe";
            if (!File.Exists(runnerPath))
                throw new FileNotFoundException("Runner.exe not existed: " + ExePath, runnerPath);

            using (var ts = new TaskService())
            {
                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                    return ts.AddTask(taskName,
                        new TimeTrigger()
                        {
                            StartBoundary = DateTime.Now + TimeSpan.FromHours(1),
                            Enabled = false
                        },
                        new ExecAction(runnerPath, scriptName, System.Environment.CurrentDirectory), null, null, TaskLogonType.InteractiveToken,
                        nameof(TaskTorch) + ":" + taskDescription);
                else
                    return ts.AddTask(taskName,
                        new TimeTrigger()
                        {
                            StartBoundary = DateTime.Now + TimeSpan.FromHours(1),
                            Enabled = false
                        },
                        new ExecAction(runnerPath, scriptName, System.Environment.CurrentDirectory), userName, password,
                        TaskLogonType.InteractiveTokenOrPassword, nameof(TaskTorch) + ":" + taskDescription);
            }

            return null;
        }

        /// <summary>
        /// add a task in windows task scheduler for this app
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="taskName"></param>
        /// <param name="taskDescription"></param>
        /// <param name="scriptName"></param>
        /// <param name="tigger"></param>
        /// <returns></returns>
        public static Task AddTask<T>(string taskName, string taskDescription, string scriptName, T tigger, string userName = "", string password = "") where T : Trigger
        {
            // 检查runner是否存在
            var runnerPath = System.Environment.CurrentDirectory + @"\" + nameof(TaskTorch) + "." + "Runner" + ".exe";
            if (!File.Exists(runnerPath))
                throw new FileNotFoundException("Runner.exe not existed: " + ExePath, runnerPath);

            BuildFolder();

            // check task
            if (TaskExists(taskName))
                return null;
            using (var ts = new TaskService())
            {
                var tf = ts.RootFolder.SubFolders[GetTaskFolderName()];

                // Create a new task definition and assign properties
                var td = ts.NewTask();
                td.RegistrationInfo.Description = nameof(TaskTorch) + ":" + taskDescription;
                td.Principal.LogonType = TaskLogonType.InteractiveToken;

                // Add a trigger
                td.Triggers.Add(tigger);

                // Add an action that will launch Notepad whenever the trigger fires
                td.Actions.Add(new ExecAction(runnerPath, scriptName, System.Environment.CurrentDirectory));


                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                    return tf.RegisterTaskDefinition(taskName, td);
                else
                    return tf.RegisterTaskDefinition(taskName, td, TaskCreation.Create, userName, password,
                 TaskLogonType.InteractiveTokenOrPassword);
            }
        }
        public static Task AddTask(string taskName, Task t, string userName = "", string password = "")
        {
            // 检查runner是否存在
            var runnerPath = System.Environment.CurrentDirectory + @"\" + nameof(TaskTorch) + "." + "Runner" + ".exe";
            if (!File.Exists(runnerPath))
                throw new FileNotFoundException("Runner.exe not existed: " + ExePath, runnerPath);

            BuildFolder();

            // check 
            if (TaskExists(taskName))
                return null;
            using (var ts = new TaskService())
            {
                var tf = ts.RootFolder.SubFolders[GetTaskFolderName()];

                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                    return tf.RegisterTaskDefinition(taskName, t.Definition);
                else
                    return tf.RegisterTaskDefinition(taskName, t.Definition, TaskCreation.Create, userName, password,
                        TaskLogonType.InteractiveTokenOrPassword);
            }
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
        public static void DeleteTmpTask(string taskName)
        {
            taskName = nameof(TaskTorch) + "_TmpTask_" + taskName;
            // 检查task是否存在
            if (TaskExists(taskName))
                return;
            using (var ts = new TaskService())
            {
                var tf = ts.RootFolder;
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
