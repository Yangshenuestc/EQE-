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
using System.Windows.Threading;
using WPFAddDemo.ViewModels;

namespace WPFAddDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer autoScrollTimer;
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
            // 初始化计时器
            autoScrollTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            autoScrollTimer.Tick += AutoScrollTimer_Tick;
            autoScrollTimer.Start();
        }

        private void AutoScrollTimer_Tick(object sender, EventArgs e)
        {
            // 滚动到底部
            scrollViewer.ScrollToEnd();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            autoScrollTimer.Stop(); // 停止计时器
        }
    }
}
