using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTorch.Loader.Model;
using TaskStatus = TaskTorch.Loader.Model.TaskStatus;

namespace TaskTorch.app.Presenter
{
    public class AddTaskPresenter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region 实现单例

        private static volatile AddTaskPresenter _ret;
        private static readonly object Obj = new object();
        private TaskV20180602 _task = new TaskV20180602();

        public static AddTaskPresenter Instance
        {
            get
            {
                if (null == _ret)
                {
                    lock (Obj)
                    {
                        if (null == _ret)
                        {
                            _ret = new AddTaskPresenter();
                        }
                    }

                }

                return _ret;
            }
        }

        #endregion

        public TaskV20180602 Task
        {
            get => _task;
            set
            {
                _task = value;
                TmpTaskName = _task.TaskName;
            }
        }

        public string TmpTaskName { get; set; }

        /// <summary>
        /// is add new task or edit old task
        /// </summary>
        public bool IsEditMode { get; set; } = false;

        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
