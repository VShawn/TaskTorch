using ServiceStack.OrmLite;

namespace TaskTorch.Loader.Model
{
    public interface ITask
    {
        TaskStatus Excute();
        string GetExcuteInfo();
        string ToYmlString();
        bool FromYmlString(string ymlString);
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

        OrmLiteConnectionFactory GetDbFactory();
        void AddLog();









        TaskStatus TaskStatus { get; set; }

        string TaskName { get; set; }
        string TaskCmd { get; set; }
        string TaskDescription { get; set; }
        string[] ExcuteInfos { get; set; }
    }
}
