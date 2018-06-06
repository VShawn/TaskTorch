using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace TaskTorch.app.Class
{
    public class FileHelper
    {
        public static List<FileInfo> ListFiles(string dirPath, string searchPattern = "*.*")
        {
            if (Directory.Exists(dirPath))
            {
                DirectoryInfo folder = new DirectoryInfo(dirPath);
                return new List<FileInfo>(folder.GetFiles(searchPattern));
            }
            return new List<FileInfo>();
        }

        /// <summary>
        /// 打开一个文件
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static FileInfo OpenFile(string filter = "文件|*.*")
        {
            OpenFileDialog openFileDialog= new OpenFileDialog();
            openFileDialog.Filter = filter;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == true)
            {
                return new FileInfo(openFileDialog.FileName);
            }
            return null;
        }



        /// <summary>
        /// 打开多个文件，返回文件路径
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static string[] OpenFiles(string filter = "文件|*.*")
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = filter;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            openFileDialog.CheckFileExists = true;//检查文件是否存在
            openFileDialog.CheckPathExists = true;//检查路径是否存在
            openFileDialog.Multiselect = true;//是否允许多选，false表示单选

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileNames;
            }
            return null;
        }
    }
}
