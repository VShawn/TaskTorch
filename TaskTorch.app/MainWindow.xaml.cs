using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32.TaskScheduler;

namespace TaskTorch.app
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Get the service on the local machine
            using (TaskService ts = new TaskService())
            {
                // Create a new task
                const string taskName = "Test";
                Task t = ts.AddTask(taskName,
                    new TimeTrigger()
                    {
                        StartBoundary = DateTime.Now + TimeSpan.FromHours(1),
                        Enabled = false
                    },
                    new ExecAction("notepad.exe", "c:\\test.log", "C:\\"));

                // Edit task and re-register if user clicks Ok
                TaskEditDialog editorForm = new TaskEditDialog();
                editorForm.Editable = true;
                editorForm.RegisterTaskOnAccept = true;
                editorForm.Initialize(t);
                // ** The four lines above can be replaced by using the full constructor
                // TaskEditDialog editorForm = new TaskEditDialog(t, true, true);
                editorForm.ShowDialog();
            }
        }
    }
}
