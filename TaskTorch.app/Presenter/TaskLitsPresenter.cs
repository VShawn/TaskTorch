using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTorch.app.Presenter
{
    public class TaskLitsPresenter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region 实现单例
        private static volatile TaskLitsPresenter _ret;
        private static readonly object Obj = new object();
        public static TaskLitsPresenter Instance
        {
            get
            {
                if (null == _ret)
                {
                    lock (Obj)
                    {
                        if (null == _ret)
                        {
                            _ret = new TaskLitsPresenter();
                        }
                    }

                }
                return _ret;
            }
        }
        #endregion
    }
}
