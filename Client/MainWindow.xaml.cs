using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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
using Client.GobangServiceReference;
namespace Client
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            StartWindow("西西", 580, 300);
            StartWindow("瓜瓜", 0, 0);
        }

        private void StartWindow(string userName, int left, int top)
        {
            ClientWindow w = new ClientWindow();
            w.Left = left;
            w.Top = top;
            w.UserName = userName;
            w.Owner = this;
            w.Closed += (sender, e) => this.Activate();
            w.Show();
        }
    }
}
