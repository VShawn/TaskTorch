﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.Management;
using System.Collections.ObjectModel;
using System.IO;
using Command = System.Management.Automation.Runspaces.Command;

namespace TaskTorch.Loader.Runner
{
    public class Runner
    {
        /// <summary>
        /// 命令的前缀命令，由后缀指向一个前缀命令
        /// 如：".py" -> "python"
        /// </summary>
        public readonly Dictionary<string, string> CmdDecorator = new Dictionary<string, string>();

        /// <summary>
        /// 在cmd中执行命令，并返回命令输出的信息与命令返回码
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns>[0] = 命令输出信息，[1] = 返回码</returns>
        public static string[] RunCmd(string cmd)
        {
            var pro = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            //pro.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            pro.Start();
            pro.StandardInput.WriteLine(cmd);
            pro.StandardInput.WriteLine("echo %ERRORLEVEL%");// add a symble for exit code
            pro.StandardInput.WriteLine("exit");// add a symble for exit code
            pro.StandardInput.AutoFlush = true;
            //获取cmd窗口的输出信息
            var output = pro.StandardOutput.ReadToEnd();
            pro.WaitForExit();//等待程序执行完退出进程
            pro.Close();

            // 截取命令的输出文本
            var content = output.Substring(output.IndexOf(cmd + "\r\n") + (cmd + "\r\n").Length);
            content = content.Substring(0, content.IndexOf("echo %ERRORLEVEL%\r\n"));
            content = content.Substring(0, content.LastIndexOf("\r\n")).Trim(new[] { '\r', '\n', ' ' });

            // 获得命令执行的返回码，为0时表示允许正确
            var retCode = output.Substring(output.LastIndexOf("echo %ERRORLEVEL%\r\n") + "echo %ERRORLEVEL%\r\n".Length);
            retCode = retCode.Substring(0, retCode.IndexOf("\r\n")).Trim(new[] { '\r', '\n', ' ' });
            return new []{ content,retCode };
        }
        /// <summary>
        /// 在Powershell中执行命令，并返回命令输出的信息与命令返回码
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns>[0] = 命令输出信息，[1] = 返回码</returns>
        public static string[] RunPowershell(string scriptPath)
        {
            RunspaceConfiguration runspaceConfiguration = RunspaceConfiguration.Create();
            Runspace runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
            runspace.Open();
            RunspaceInvoke scriptInvoker = new RunspaceInvoke(runspace);
            Pipeline pipeline = runspace.CreatePipeline();
            Command scriptCommand = new Command(scriptPath);
            pipeline.Commands.Add(scriptCommand);
            var re = pipeline.Invoke();//在pipeline管道类线程上执行委托，并且获取到执行命令后的返回值
            var content = "";
            foreach (var a in re)
            {
                content = a.ToString() + content;//打印返回信息
            }
            var retCode = "0";
            if (pipeline.Error.Count > 0)
            {
                //throw new Exception("脚本执行失败");
                retCode = "9999";
            }
            runspace.Close();//关闭通信通道
            return new[] { content, retCode };
        }
        //public static string[] RunPowershell(string cmd)
        //{
        //    var pro = new System.Diagnostics.Process
        //    {
        //        StartInfo =
        //        {
        //            FileName = "powershell.exe",
        //            UseShellExecute = false,
        //            RedirectStandardError = true,
        //            RedirectStandardInput = true,
        //            RedirectStandardOutput = true,
        //            CreateNoWindow = true
        //        }
        //    };
        //    //pro.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        //    pro.Start();
        //    pro.StandardInput.WriteLine(cmd);
        //    pro.StandardInput.WriteLine("echo $LASTEXITCODE");
        //    pro.StandardInput.WriteLine("exit");
        //    pro.StandardInput.AutoFlush = true;
        //    //获取cmd窗口的输出信息
        //    var output = pro.StandardOutput.ReadToEnd();
        //    pro.WaitForExit();//等待程序执行完退出进程
        //    pro.Close();

        //    // 截取命令的输出文本
        //    var content = output.Substring(output.IndexOf(cmd + "\r\n") + (cmd + "\r\n").Length);
        //    content = content.Substring(0, content.IndexOf("echo $LASTEXITCODE\r\n"));
        //    if (content.LastIndexOf("\r\n") >= 0)
        //        content = content.Substring(0, content.LastIndexOf("\r\n")).Trim(new[] {'\r', '\n', ' '});
        //    else
        //        content = "";
        //    // 获得命令执行的返回码，为0时表示允许正确
        //    var retCode = output.Substring(output.LastIndexOf("echo $LASTEXITCODE\r\n") + "echo $LASTEXITCODE\r\n".Length);
        //    if (retCode.IndexOf("\r\n") >= 0)
        //        retCode = retCode.Substring(0, retCode.IndexOf("\r\n")).Trim(new[] {'\r', '\n', ' '});
        //    else
        //        retCode = "9999";
        //    if (!int.TryParse(retCode, out var i))
        //        retCode = "9999";
        //    return new[] { content, retCode };
        //}
    }
}
