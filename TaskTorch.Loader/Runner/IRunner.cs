using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTorch.Loader.Runner
{
    public class Runner
    {
        /// <summary>
        /// 命令的前缀命令，由后缀指向一个前缀命令
        /// 如：".py" -> "python"
        /// </summary>
        public readonly Dictionary<string, string> CmdDecorator = new Dictionary<string, string>();

        public static string RunCmd(string cmd)
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
            pro.StandardInput.WriteLine("exit");
            pro.StandardInput.AutoFlush = true;
            //获取cmd窗口的输出信息
            var output = pro.StandardOutput.ReadToEnd();
            pro.WaitForExit();//等待程序执行完退出进程
            pro.Close();
            return output;
        }

        public static string RunPowershell(string cmd)
        {
            var pro = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = "powershell.exe",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            //pro.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            pro.Start();
            pro.BeginOutputReadLine();
            pro.StandardInput.WriteLine(cmd);
            pro.StandardInput.WriteLine("exit");
            pro.StandardInput.AutoFlush = true;
            //获取cmd窗口的输出信息
            var output = pro.StandardOutput.ReadToEnd();
            pro.WaitForExit();//等待程序执行完退出进程
            pro.Close();
            return output;
        }
    }
}
