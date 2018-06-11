using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using TaskTorch.app.Presenter;

namespace TaskTorch.app
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public MainWindow()
        {
            InitializeComponent();
            Grid.DataContext = this;

            MainPresenter.Instance.ShowPage(MainPresenter.MainPage.TaskList);
        }
    }
}
