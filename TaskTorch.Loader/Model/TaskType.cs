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
        Success,
        Failure,
        Exception,
    }
}
