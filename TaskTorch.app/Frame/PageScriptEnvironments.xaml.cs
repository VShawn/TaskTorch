using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TaskTorch.app.Presenter;

namespace TaskTorch.app.Frame
{
    /// <summary>
    /// PageScriptEnvironments.xaml 的交互逻辑
    /// </summary>
    public partial class PageScriptEnvironments : Page
    {
        public PageScriptEnvironments()
        {
            InitializeComponent();
        }

        private void BtnDeleteTask_OnClick(object sender, RoutedEventArgs e)
        {
            var curItem = ((ListBoxItem)LvList.ContainerFromElement((Button)sender)).Content;
            var t = curItem as Script;
            ScriptEnvironmentsPresenter.Instance.Scripts.RemoveAll(script => script.Extension == t.Extension);
            ScriptEnvironmentsPresenter.SerializeMethod(ScriptEnvironmentsPresenter.Instance.Scripts);
            LvList.Items.Refresh();
        }

        private void BtnAdd_OnClick(object sender, RoutedEventArgs e)
        {
            var s = new Script
            {
                Extension = TbEx.Text.Trim().ToLower(),
                CmdTemplate = TbPr.Text,
            };

            if (s.Extension.Length < 2)
                return;
            if (s.Extension == "")
                return;

            if (!s.Extension.StartsWith("."))
            {
                MessageBox.Show("后缀名应以“.”开始！");
                return;
            }

            if (s.CmdTemplate.IndexOf("%scriptname%") == -1)
            {
                MessageBox.Show("执行模板必须包含%scriptname%");
                return;
            }


            ScriptEnvironmentsPresenter.Instance.Scripts.RemoveAll(script => script.Extension == s.Extension);
            ScriptEnvironmentsPresenter.Instance.Scripts.Add(s);
            ScriptEnvironmentsPresenter.SerializeMethod(ScriptEnvironmentsPresenter.Instance.Scripts);
            LvList.Items.Refresh();
        }

        private void TbEx_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (TbPr.Text == "" && TbEx.Text != "")
            {
                TbPr.Text = ScriptEnvironmentsPresenter.SCRIPT_NAME + TbEx.Text;
            }
        }
    }
}
