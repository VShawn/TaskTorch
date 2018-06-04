namespace TaskTorch.Loader.Model
{
    public interface ITask
    {
        TaskStatus Excute();
        string ToYmlString();
        bool FromYmlString(string ymlString);
        string GetVersion();

        string GetNexTaskName();

        TaskStatus TaskStatus { get; set; }

        string TaskName { get; set; }
}
}
