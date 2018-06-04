namespace TaskTorch.Loader.Model
{
    public enum TaskType
    {
        Cmd,
        Powershell,
        //Exe,
    }
    public enum TaskStatus
    {
        NotRun = 0,
        Run,
        Success,
        Failure,
        Exception,
    }
}
