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
                if (ts.RootFolder.SubFolders.Exists(GetTaskFolderName())) return;
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
        /// enum all task in windows task scheduler witch is belong to this app. 
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
                //if (tf.Tasks.Exists(taskName))
                //    return true;
                foreach (var t in tf.Tasks)
                {
                    if (t.Name == taskName)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="taskName"></param>
        /// <returns></returns>
        public static Task GetTask(string taskName)
        {
            using (var ts = new TaskService())
            {
                // 检查目录是否存在
                if (!ts.RootFolder.SubFolders.Exists(GetTaskFolderName()))
                    return null;
                // 检查task是否存在
                var tf = ts.RootFolder.SubFolders[GetTaskFolderName()];
                //if (tf.Tasks.Exists(taskName))
                //    return true;
                foreach (var t in tf.Tasks)
                {
                    if (t.Name == taskName)
                        return t;
                }
            }
            return null;
        }

        public static Task AddTmpTask<T>(string taskName, string taskDescription, string scriptName, T tigger,
            string userName = null, string password = null) where T : Trigger
        {
            taskName = GetTaskFolderName() + "_TmpTask_" + taskName;
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
        /// <summary>
        /// 向任务计划程序中新增一个计划
        /// </summary>
        /// <param name="taskName"></param>
        /// <param name="t"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 删除任务计划程序中的某个计划任务
        /// </summary>
        /// <param name="taskName"></param>
        public static void DeleteTask(string taskName)
        {
            // 检查task是否存在
            if (!TaskExists(taskName))
                return;

            using (var ts = new TaskService())
            {
                var tf = ts.RootFolder.SubFolders[GetTaskFolderName()];
                tf.DeleteTask(taskName, false);
            }
        }
        public static void DeleteTmpTask(string taskName)
        {
            taskName = GetTaskFolderName() + "_TmpTask_" + taskName;
            // 检查task是否存在
            if (TaskExists(taskName))
                return;

            using (var ts = new TaskService())
            {
                var tf = ts.RootFolder;
                tf.DeleteTask(taskName, false);
            }
        }
        /// <summary>
        /// 删除所有定时任务的临时缓存
        /// 注意，这个方法是线程不安全的！
        /// </summary>
        public static void DeleteAllTmpTask()
        {
            using (var ts = new TaskService())
            {
                foreach (var t in ts.RootFolder.Tasks)
                {
                    if (t.Name.StartsWith(GetTaskFolderName()))
                        ts.RootFolder.DeleteTask(t.Name);
                }
            }
        }

        /// <summary>
        /// 设置某个计划任务禁用、启用
        /// </summary>
        /// <param name="taskName"></param>
        /// <param name="enable"></param>
        public static void SetEnable(string taskName, bool enable)
        {
            // 检查task是否存在
            if (TaskExists(taskName))
                return;

            using (var ts = new TaskService())
            {
                var tf = ts.RootFolder.SubFolders[GetTaskFolderName()];
                tf.Tasks[taskName].Enabled = enable;
            }
        }
    }
}
