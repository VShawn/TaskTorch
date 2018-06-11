using System.ComponentModel;
using ServiceStack.OrmLite;

namespace TaskTorch.Loader.Model
{
    public interface ITask : INotifyPropertyChanged
    {
        event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 执行任务
        /// </summary>
        /// <returns></returns>
        TaskStatus Excute();
        /// <summary>
        /// 返回任务上一次的执行输出信息
        /// </summary>
        /// <returns></returns>
        string GetExcuteInfo();
        string GetVersion();
        /// <summary>
        /// 检查任务是否需要重试，并返回重试的延时时间（秒）
        /// </summary>
        /// <returns></returns>
        int NeedRetry();
        /// <summary>
        /// 获取任务执行完成后下一步需要执行的任务名
        /// 若无下一步任务，返回空字符串
        /// </summary>
        /// <returns></returns>
        string GetNexTaskName();

        string GetTaskFolderPath();

        /// <summary>
        /// 获取任务详细的描述信息
        /// </summary>
        /// <returns></returns>
        string GetDetail();

        /// <summary>
        /// 获得该任务的数据库连接
        /// </summary>
        /// <returns></returns>
        OrmLiteConnectionFactory GetDbFactory();
        /// <summary>
        /// 将上一次的执行状态记录到数据库和log文件
        /// </summary>
        void AddLog();


        string ToYmlString();
        bool FromYmlString(string ymlString);






        TaskStatus TaskStatus { get; set; }

        string TaskName { get; set; }
        string TaskCmd { get; set; }
        string TaskDescription { get; set; }
        string[] ExcuteInfos { get; set; }
    }
}
