using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Microsoft.Win32.TaskScheduler;
using ServiceStack.OrmLite;
using TaskTorch.Loader.Model;
using TaskStatus = System.Threading.Tasks.TaskStatus;

namespace TaskTorch.app.Class
{
    [ValueConversion(typeof(string), typeof(int))]
    public class TaskRuningCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string taskName = value.ToString();
            var ymlPath = System.Environment.CurrentDirectory + "\\tasks\\" + taskName + "\\" + taskName + ".yml";
            if (File.Exists(ymlPath))
            {
                var t = TaskSimpleFactory.CreateTask(File.ReadAllText(ymlPath));
                using (var db = t.GetDbFactory().Open())
                {
                    if (db.TableExists(nameof(TaskRunLog)))
                        return db.Select<TaskRunLog>(x => x.Id > 0).Count;
                }
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(string), typeof(string))]
    public class TaskLastStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string taskName = value.ToString();
            var ymlPath = System.Environment.CurrentDirectory + "\\tasks\\" + taskName + "\\" + taskName + ".yml";
            if (File.Exists(ymlPath))
            {
                var t = TaskSimpleFactory.CreateTask(File.ReadAllText(ymlPath));
                using (var db = t.GetDbFactory().Open())
                {
                    if (db.TableExists(nameof(TaskRunLog)))
                    {
                        var query = db.From<TaskRunLog>().OrderByDescending(e => e.Id);
                        var l = db.Select<TaskRunLog>(query);
                        if (l?.Count() > 0)
                            return l[0].Statu.ToString();
                    }
                }
            }
            return TaskTorch.Loader.Model.TaskStatus.NotRun.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(string), typeof(string))]
    public class TaskLastRunTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string taskName = value.ToString();
            var ymlPath = System.Environment.CurrentDirectory + "\\tasks\\" + taskName + "\\" + taskName + ".yml";
            if (File.Exists(ymlPath))
            {
                var t = TaskSimpleFactory.CreateTask(File.ReadAllText(ymlPath));
                using (var db = t.GetDbFactory().Open())
                {
                    if (db.TableExists(nameof(TaskRunLog)))
                    {
                        var query = db.From<TaskRunLog>().OrderByDescending(e => e.Id);
                        var l = db.Select<TaskRunLog>(query);
                        if (l?.Count() > 0)
                            return l[0].Date.ToString();
                    }
                }
            }
            return TaskTorch.Loader.Model.TaskStatus.NotRun.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(string), typeof(string))]
    public class TaskTiggerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string taskName = value.ToString();
            var task = WinTaskHelper.TaskHelper.GetTask(taskName);
            if (task == null)
                return "null";
            string ret = task.Definition.Principal.LogonType == TaskLogonType.InteractiveToken ?  "用户已登录且：" : "无论是否登录且：";
            foreach (var tigger in task.Definition.Triggers)
            {
                ret += tigger.ToString() + " -> " + (tigger.Enabled ? "Enabled" : "Disable") + "; ";
            }

            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}