using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TaskTorch.app.Presenter
{
    public class MainPresenter: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region 实现单例
        private static volatile MainPresenter _ret;
        private static readonly object Obj = new object();
        public static MainPresenter Instance
        {
            get
            {
                if (null == _ret)
                {
                    lock (Obj)
                    {
                        if (null == _ret)
                        {
                            _ret = new MainPresenter();
                        }
                    }

                }
                return _ret;
            }
        }
        #endregion


        public Visibility ShowTaskListPage { get;private set; } = Visibility.Collapsed;
        public Visibility ShowAddTaskPage { get; private set; } = Visibility.Collapsed;


        public enum MainPage
        {
            TaskList,
            AddTask,
        }
        public void ShowPage(MainPage p)
        {
            ShowTaskListPage = ShowAddTaskPage = Visibility.Collapsed;
            switch (p)
            {
                case MainPage.TaskList:
                    ShowTaskListPage = Visibility.Visible;
                    break;
                case MainPage.AddTask:
                    ShowAddTaskPage = Visibility.Visible;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(p), p, null);
            }
        }
    }
}
