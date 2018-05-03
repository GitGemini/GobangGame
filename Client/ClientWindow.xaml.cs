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
using System.Windows.Shapes;
using Client.GobangServiceReference;
using System.Threading;
using System.Xml.Linq;

namespace Client
{
    /// <summary>
    /// ClientWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ClientWindow : Window, IGobangServiceCallback
    {
        //Random r = new Random();
        public string UserName
        {
            get { return textBoxUserName.Text; }
            set { textBoxUserName.Text = value; }
        }

        private int nextColor = -1;        //该哪一方放置棋子（-1:不允许，0：黑方，1：白方）
        private bool isGameStart = false;  //是否已开始游戏
        private const int max = 4;       //棋盘行列最大值（0～16）
        private int maxTables;            //服务端开设的最大房间号
        private int tableIndex = -1;      //房间号（所坐的游戏桌号）
        private int tableSide = -1;       //座位号
        private Border[,] gameTables;        //开设的房间（每个房间一桌）
        private int[,] grid = new int[max, max];  //保存棋盘上每个棋子的颜色(16*16的交叉点）
        private Image[,] images = new Image[max, max];  //保存棋盘上每个棋子
        //private bool isFromServer;          //是否为服务端发送过来的操作
        private GobangServiceClient client;  //客户端实例

        private int nextX = -1, nextY = -1; //第二次点击的坐标
        private int numClick = 0; //第几次点击
        private string tag;
        //private int[] num = new int[16];
        //int tim = 0;
        private Image[,] bgImages = new Image[max, max];
        private int[,] suiji = new int[4, 4] { { 0, 6, 2, 12 }, { 8, 5, 1, 13 }, { 4, 9, 15, 11 }, { 3, 7, 14, 10 } };
        private bool isfirst = true;
        private bool myFrist = true;
        private List<MyClass.GifModel> list = new List<MyClass.GifModel>(); // 用于 保存表情中listBox的 Items 


        public ClientWindow()
        {
            InitializeComponent();

            //确保关闭窗口时关闭客户端
            this.Closing += ClientWindow_Closing;
            this.Loaded += ClientWindow_Loaded;
            ChangeRoomsInfoVisible(false);
            ChangeRoomVisible(false);
            SetNextColor(-1);

       
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    images[i, j] = new Image() { Source = new BitmapImage(new Uri(@"images\" + suiji[i, j].ToString() + ".jpg", UriKind.Relative)) };
                    bgImages[i, j] = new Image() { Source = new BitmapImage(new Uri(@"images\allbg.jpg", UriKind.Relative)) };
                    Canvas.SetLeft(bgImages[i, j], (i + 1) * 80 - 60);
                    Canvas.SetTop(bgImages[i, j], (j + 1) * 80 - 60);
                    canvas1.Children.Add(bgImages[i, j]);
                }
            }

           
        }

        //  图片信息加载慢， 窗体 加载时就读取  ，但是问题没有解决
        private  void ClientWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AddQQExpression();
        }

        void ClientWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ChangeState(btnLogin, true, btnLogout, false);
            if (client != null)
            {
                if (tableIndex != -1) //如果在房间内，要求先返回大厅
                {
                    MessageBox.Show("请先返回大厅，然后再退出");
                    e.Cancel = true;
                }
                else
                {
                    client.Logout(UserName); //从大厅退出
                    client.Close();
                }
            }
        }
        private void ChangeRoomsInfoVisible(bool visible)
        {
            if (visible == false)
            {
                gridRooms.Visibility = System.Windows.Visibility.Collapsed;
                gridMessage.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                gridRooms.Visibility = System.Windows.Visibility.Visible;
                gridMessage.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void ChangeRoomVisible(bool visible)
        {
            if (visible == false)
            {
                gridRoom.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                gridRoom.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void SetNextColor(int next)
        {
            nextColor = next;
            if (nextColor == 0)
            {
                stackPanelGameTip.Visibility = System.Windows.Visibility.Visible;
                blackImage.Visibility = System.Windows.Visibility.Visible;
                whiteImage.Visibility = System.Windows.Visibility.Collapsed;
            }
            else if (nextColor == 1)
            {
                stackPanelGameTip.Visibility = System.Windows.Visibility.Visible;
                blackImage.Visibility = System.Windows.Visibility.Collapsed;
                whiteImage.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                stackPanelGameTip.Visibility = System.Windows.Visibility.Collapsed;
                blackImage.Visibility = System.Windows.Visibility.Collapsed;
                whiteImage.Visibility = System.Windows.Visibility.Collapsed;
            }
        }


        // 对话消息添加进 listBox中
        private void AddMessage(string name, string str)
        {
            char s = str[0];
            StackPanel sp = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Width = 240
            };
            switch (s)
            {
                case 'T':
                    TextBlock tbName = new TextBlock() { Text = name + ":" };
                    tbName.Foreground = new SolidColorBrush(Colors.Black);
                    TextBlock tbMsg = new TextBlock()
                    {
                        Text = str.Substring(1),
                        FontSize = 16,
                        TextWrapping = TextWrapping.Wrap
                    };
                    tbMsg.Foreground = new SolidColorBrush(Colors.LightBlue);
                    if (name == UserName)
                    {
                        sp.HorizontalAlignment = HorizontalAlignment.Right;
                        tbName.HorizontalAlignment = HorizontalAlignment.Right;
                        tbMsg.HorizontalAlignment = HorizontalAlignment.Right;
                    }
                    else
                    {
                        sp.HorizontalAlignment = HorizontalAlignment.Left;
                        tbName.HorizontalAlignment = HorizontalAlignment.Left;
                        tbMsg.HorizontalAlignment = HorizontalAlignment.Left;
                    }
                    sp.Children.Add(tbName);
                    sp.Children.Add(tbMsg);
                    break;
                case 'E':
                    TextBlock tbNameExpression = new TextBlock() { Text = name + ":" };
                    tbNameExpression.Foreground = new SolidColorBrush(Colors.Black);
                    MyClass.GifImage myGifImge = new MyClass.GifImage()
                    {
                        Width = 40
                    };
                    myGifImge.Source = str.Substring(1);

                    if (name == UserName)
                    {
                        sp.HorizontalAlignment = HorizontalAlignment.Right;
                        tbNameExpression.HorizontalAlignment = HorizontalAlignment.Right;
                        myGifImge.HorizontalAlignment = HorizontalAlignment.Right;
                    }
                    else
                    {
                        sp.HorizontalAlignment = HorizontalAlignment.Left;
                        tbNameExpression.HorizontalAlignment = HorizontalAlignment.Left;
                        myGifImge.HorizontalAlignment = HorizontalAlignment.Left;
                    }
                    sp.Children.Add(tbNameExpression);
                    sp.Children.Add(myGifImge);
                    break;
                default:
                    TextBlock t = new TextBlock();
                    t.Text = str;
                    t.Foreground = Brushes.Blue;
                    sp.Children.Add(t);
                    break;
            }
            listBoxMessage.Items.Add(sp);
            listBoxMessage.SelectedIndex = listBoxMessage.Items.Count - 1;
            listBoxMessage.ScrollIntoView(listBoxMessage.SelectedItem);
        }

        private void AddColorMessage(string str, SolidColorBrush color)
        {
            TextBlock t = new TextBlock();
            t.Text = str;
            t.Foreground = color;
            listBoxMessage.Items.Add(t);
        }

        private static void ChangeState(Button btnStart, bool isStart, Button btnStop, bool isStop)
        {
            btnStart.IsEnabled = isStart;
            btnStop.IsEnabled = isStop;
        }

        #region 鼠标和键盘事件
        //单击登录按钮引发的事件
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(textBlockBlackUserName.Text))
            {
                MessageBox.Show("用户名不能为空");
                return;
            }

            UserName = textBoxUserName.Text;
            this.Cursor = Cursors.Wait;
            client = new GobangServiceClient(new InstanceContext(this));
            try
            {
                client.Login(textBoxUserName.Text);
                serviceTextBlock.Text = "服务端地址：" + client.Endpoint.ListenUri.ToString();
                ChangeState(btnLogin, false, btnLogout, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("与服务端连接失败：" + ex.Message);
                return;
            }
            this.Cursor = Cursors.Arrow;
        }

        //单击退出按钮引发的事件
        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); //在窗口的Closing事件中处理退出操作
        }

        //在某个座位坐下时引发的事件
        private void RoomSide_MouseDown(object sender, MouseButtonEventArgs e)
        {
            btnLogout.IsEnabled = false;
            Border border = e.Source as Border;
            if (border != null)
            {
                string s = border.Tag.ToString();
                tableIndex = int.Parse(s[0].ToString());
                tableSide = int.Parse(s[1].ToString());
                client.SitDown(UserName, tableIndex, tableSide);
            }
        }

        //单击发送按钮引发的事件
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            client.Talk(tableIndex, UserName, "T"+textBoxTalk.Text);
            textBoxTalk.Clear();
        }

        //在对话文本框中按回车键时引发的事件
        private void textBoxTalk_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                client.Talk(tableIndex, UserName, "T"+textBoxTalk.Text);
                textBoxTalk.Clear();
            }
        }

        //单击开始按钮引发的事件
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            client.Start(UserName, tableIndex, tableSide);
            btnStart.IsEnabled = false;
            SetNextColor(-1);
        }

        //单击返回大厅按钮引发的事件
        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            client.GetUp(tableIndex, tableSide);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    images[i, j] = new Image() { Source = new BitmapImage(new Uri(@"images\" + suiji[i, j].ToString() + ".jpg", UriKind.Relative)) };
                
                }
            }
        }

        //  用户点击 发送表情或者 点击震动引发的事件。
        private void sendMessage(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            switch (btn.Name)
            {
                case "sendExpression":   // 表情
                    Pop_up.IsOpen = true;

                    break;
                case "btn_shake":                  // 震动
                    client.Shake(tableIndex, tableSide);
                    break;
            }
        }

        // 发送表情需要调用的方法。
        private void AddQQExpression()
        {
            QQExpression.Items.Clear();
            List<MyClass.GifModel> list = new List<MyClass.GifModel>();

            string xmlpath = "../../XmlDoc/QQExpression.xml";
            XDocument xdoc = XDocument.Load(xmlpath);

            XElement element = xdoc.Root;

            IEnumerable<XElement> e = element.Elements("Emoticon");
            foreach (var item in e)
            {
                MyClass.GifModel gifmodel = new MyClass.GifModel();
                gifmodel.gifImg = "../../qqexpression/" + item.Value;
                list.Add(gifmodel);
            }

            QQExpression.ItemsSource = list;
         
        }
     


        //在棋盘上单击鼠标左键时引发的事件
        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isGameStart == false) return;
            Point point = e.GetPosition(canvas1);
            int x = (int)(point.X + 60) / 80;
            int y = (int)(point.Y + 60) / 80;
            if (!(x < 1 || x > max || y < 1 || y > max))
            {
                int i = x - 1;
                int j = y - 1;
                if (grid[i, j] == -1 && nextColor == tableSide)
                {

                    client.SetDot(tableIndex, i, j);
                    myFrist = false;
                }
                else if (grid[i, j] == tableSide && nextColor == tableSide)
                {

                    bool b1 = grid[i, j] == -1;
                    bool b2 = nextColor == tableSide;
                    nextX = i;
                    nextY = j;
                    //client.SetGetNext(i, j);
                    client.TalkNum(tableIndex, i, j);
                    numClick = 1;

                }
                else
                {
                    if (numClick == 1)
                    {
                        if (grid[i, j] != tableSide && nextColor == tableSide)
                        {
                            
                            client.SetDoubleDot(tableIndex, tableSide, i, j, nextX, nextY);
                        }
                        else
                        {
                            numClick = 0;
                            nextX = -1;
                            nextY = -1;
                        }
                    }

                }


                //else if (grid[i,j] != tableSide)
                //{
                //    //这里要判断谁大谁小，先不写
                //    numClick = 2;
                //    client.SetDot(tableIndex, i, j);
                //}


            }

        }     
        #endregion //鼠标和键盘事件

        #region 实现服务端指定的IRndGameServiceCallback接口

        /// <summary>有用户登录</summary>
        public void ShowLogin(string loginUserName, int maxTables)
        {
            if (loginUserName == UserName)
            {
                ChangeRoomsInfoVisible(true);
                this.maxTables = maxTables;
                this.CreateTables();
            }
            AddMessage(loginUserName, "【系统】:" + loginUserName + "进入大厅。");
        }

        /// <summary>其他用户退出</summary>
        public void ShowLogout(string userName)
        {
            AddMessage(userName, "【系统】:" + userName + "退出大厅。");
        }

        /// <summary>用户入座</summary>
        public void ShowSitDown(string userName, int side)
        {
            stackPanelGameTip.Visibility = System.Windows.Visibility.Collapsed;
            if (side == tableSide)
            {
                isGameStart = false;
                btnLogout.IsEnabled = false;
                btnStart.IsEnabled = true;
                listBoxRooms.IsEnabled = false;//返回大厅前不允许再坐到另一个位置
                textBlockRoomNumber.Text = "桌号：" + (tableIndex + 1);
                ChangeRoomVisible(true);
            }
            if (side == 0)
            {
                textBlockBlackUserName.Text +=  userName;
                AddMessage(userName, string.Format("{0}在房间{1}红方入座。", "【系统】:" + userName, tableIndex + 1));
            }
            else
            {
                textBlockWhiteUserName.Text  += userName;

                AddMessage(userName, string.Format("{0}在房间{1}蓝方入座。", "【系统】:" + userName, tableIndex + 1));
            
            }
        }

        /// <summary>用户离座</summary>
        public void ShowGetUp(int side)
        {
            stackPanelGameTip.Visibility = System.Windows.Visibility.Collapsed;
            if (side == tableSide)
            {
                isGameStart = false;
                btnLogout.IsEnabled = true;
                listBoxRooms.IsEnabled = true;//返回大厅后允许再坐到另一个位置
                AddMessage(UserName, "【系统】:" + UserName + "返回大厅。");
                this.tableIndex = -1;
                this.tableSide = -1;
                ChangeRoomVisible(false);
            }
            else
            {
                if (isGameStart)
                {
                    AddMessage("","对方回大厅了，游戏终止。");
                    isGameStart = false;
                    btnStart.IsEnabled = true;
                }
                else
                {
                    AddMessage("","对方返回大厅。");
                }
                if (side == 0) textBlockBlackUserName.Text = "";
                else textBlockWhiteUserName.Text = "";
            }
        }

        public void ShowStart(int side)
        {
            ResetGrid();
            isfirst = false;
            if (side == 0) AddMessage("","【系统】:红方已开始。");
            else AddMessage("", "【系统】:蓝方已开始。");
        }

        public void ShowTalk(string userName, string message)
        {
            //AddColorMessage(string.Format("{0}：{1}", userName, message), Brushes.Black);
            AddMessage(userName, message); 
        }

        /// <summary>设置棋子状态。参数：行，列，颜色</summary>
        public void ShowSetDot(int i, int j, int color)
        {
            if (suiji[i,j]>7) grid[i, j] = 1;
            else grid[i, j] = 0;
            //new BitmapImage(new Uri(@"daw\adw.jpg", UriKind.Relative));
            if (nextX == -1 && nextY == -1)
            {
                images[i, j] = new Image() { Source = new BitmapImage(new Uri(@"images\" + suiji[i, j].ToString() + ".jpg", UriKind.Relative)) };
                Canvas.SetLeft(images[i, j], (i + 1) * 80 - 60);
                Canvas.SetTop(images[i, j], (j + 1) * 80 - 60);
                canvas1.Children.Remove(bgImages[i, j]);
                canvas1.Children.Add(images[i, j]);
                if (myFrist)
                {
                    color = 0;
                }
                    SetNextColor((color + 1) % 2);
                    tag += "ShowSetDot" + color + "\n";
              //  textBlockWhiteUserName.Text +=grid[i,j];
            }
            //else if (numClick == 2)
            //{
            //    if (color == 0) images[i, j] = new Image() { Source = new BitmapImage(new Uri(@"images\" + t[nextX, nextY].ToString() + ".jpg", UriKind.Relative)) };
            //    else images[i, j] = new Image() { Source = new BitmapImage(new Uri(@"images\" + t[nextX, nextY].ToString() + ".jpg", UriKind.Relative)) };
            //    //tim++;
            //    Canvas.SetLeft(images[i, j], (i + 1) * 80 - 60);
            //    Canvas.SetTop(images[i, j], (j + 1) * 80 - 60);
            //    canvas1.Children.Add(images[i, j]);
            //    canvas1.Children.Remove(images[nextY, nextY]);
            //    SetNextColor((color + 1) % 2);
            //    numClick = 0;
            //    nextX = nextY = -1;
            //}
            //else
            //{
            //    if (color == 0) images[i, j] = new Image() { Source = new BitmapImage(new Uri(@"images\" + t[nextX, nextY].ToString() + ".jpg", UriKind.Relative)) };
            //    else images[i, j] = new Image() { Source = new BitmapImage(new Uri(@"images\" + t[nextX, nextY].ToString() + ".jpg", UriKind.Relative)) };
            //    //tim++;
            //    Canvas.SetLeft(images[i, j], (i + 1) * 80 - 60);
            //    Canvas.SetTop(images[i, j], (j + 1) * 80 - 60);
            //    canvas1.Children.Add(images[i, j]);
            //    canvas1.Children.Remove(images[nextY, nextY]);
            //    SetNextColor((color + 1) % 2);
            //    numClick = 0;
            //    nextX = nextY = -1;
            //}

        }
      
        public void ShowSetDoubleDot(int i, int j, int color, int nX, int nY)
        {
            if (Math.Abs(nX - i)+  Math.Abs(nY - j) == 1 )
            {
                if (suiji[nX, nY] % 8 <= suiji[i, j] % 8 )
                {

                    canvas1.Children.Remove(images[i, j]);
                    canvas1.Children.Remove(images[nX, nY]);
                    images[i, j] = new Image() { Source = new BitmapImage(new Uri(@"images\" + suiji[nX, nY].ToString() + ".jpg", UriKind.Relative)) };


                    //tim++;
                    Canvas.SetLeft(images[i, j], (i + 1) * 80 - 60);
                    Canvas.SetTop(images[i, j], (j + 1) * 80 - 60);
                    canvas1.Children.Add(images[i, j]);
                    grid[nX, nY] = -1;
                    int n = suiji[i, j];
                    suiji[i, j] = suiji[nX, nY];
                    suiji[nX, nY] = n;
                    if (suiji[i, j] > 7) grid[i, j] = 1;
                    else grid[i, j] = 0;
                    SetNextColor((color + 1) % 2);
                    //SetNextColor(color);
                    tag += "执行了ShowSetDot方法，其中color为" + color;
                    numClick = 0;
                    nextX = -1;
                    nextY = -1;

                }
            }
        }


        public void GameStart()
        {
            stackPanelGameTip.Visibility = System.Windows.Visibility.Visible;
            this.isGameStart = true;  //为true时才可以放棋子
            SetNextColor(0);
            blackImage.Visibility = System.Windows.Visibility.Visible;
        }

        public void GameWin(string message)
        {
            AddColorMessage("\n" + message + "\n", Brushes.Red);
            btnStart.IsEnabled = true;
            stackPanelGameTip.Visibility = System.Windows.Visibility.Collapsed;
            this.isGameStart = false;
            SetNextColor(-1);
            blackImage.Visibility = System.Windows.Visibility.Collapsed;
            whiteImage.Visibility = System.Windows.Visibility.Collapsed;
        }

        public void UpdateTablesInfo(string tablesInfo, int userCount)
        {
            textBlockMessage.Text = string.Format("在线人数：{0}", userCount);

            for (int i = 0; i < maxTables; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (tableIndex == -1)
                    {
                        if (tablesInfo[2 * i + j] == '0')
                        {
                            gameTables[i, j].Child.Visibility = System.Windows.Visibility.Hidden;
                            gameTables[i, j].Child.IsEnabled = true;
                        }
                        else
                        {
                            gameTables[i, j].Child.Visibility = System.Windows.Visibility.Visible;
                            gameTables[i, j].Child.IsEnabled = false;
                        }
                    }
                    else
                    {
                        gameTables[i, j].Child.IsEnabled = false;
                        if (tablesInfo[2 * i + j] == '0')
                        {
                            gameTables[i, j].Child.Visibility = System.Windows.Visibility.Hidden;
                        }
                        else gameTables[i, j].Child.Visibility = System.Windows.Visibility.Visible;
                    }
                }
            }
        }
        #endregion //实现服务端指定的IRndGameServiceCallback接口


        #region 接口调用的方法
        /// <summary>创建游戏桌</summary>
        private void CreateTables()
        {
            this.gameTables = new Border[maxTables, 2];
            //isFromServer = false;
            //创建游戏大厅中的房间（每房间一个游戏桌）
            for (int i = 0; i < maxTables; i++)
            {
                int j = i + 1;
                StackPanel sp = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(5)
                };
                TextBlock text = new TextBlock()
                {
                    Text = "房间" + (i + 1),
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    Width = 40
                };
                gameTables[i, 0] = new Border()
                {
                    Tag = i + "0",
                    Background = Brushes.White,
                    Child = new Image()
                    {
                        Source = ((Image)this.Resources["player"]).Source,
                        Height = 25
                    }
                };
                Image image = new Image()
                {
                    Source = ((Image)this.Resources["smallBoard"]).Source,
                    Height = 25
                };
                gameTables[i, 1] = new Border()
                {
                    Tag = i + "1",
                    Background = Brushes.White,
                    Child = new Image()
                    {
                        Source = ((Image)this.Resources["player"]).Source,
                        Height = 25
                    }
                };
                gameTables[i, 0].MouseDown += RoomSide_MouseDown;
                gameTables[i, 1].MouseDown += RoomSide_MouseDown;
                sp.Children.Add(text);
                sp.Children.Add(gameTables[i, 0]);
                sp.Children.Add(image);
                sp.Children.Add(gameTables[i, 1]);
                

                listBoxRooms.Items.Add(sp);
            }
        }

        /// <summary>重置棋盘</summary>
        private void ResetGrid()
        {
            for (int i = 0; i < max; i++)
            {
                for (int j = 0; j < max; j++)
                {
                    if (grid[i, j] != -1)
                    {
                        grid[i, j] = -1;
                        canvas1.Children.Remove(images[i, j]);
                        if (!isfirst)
                        {
                            canvas1.Children.Add(bgImages[i, j]);
                        }
                        suiji = new int[4, 4] { { 0, 6, 2, 12 }, { 8, 5, 1, 13 }, { 4, 9, 15, 11 }, { 3, 7, 14, 10 } };
                        images[i, j] = null;
                    }
                }
            }
        }

        public void ShowTalkNum(int index, int i, int j)
        {
            nextX = i;
            nextY = j;
           // textBlockBlackUserName.Text = "" + i + j+"选中了";
        }



        private void QQExpression_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ListBox listBox1 = sender as ListBox;
            MyClass.GifModel model = listBox1.SelectedItem as MyClass.GifModel;
            Pop_up.IsOpen = false;
            client.Talk(tableIndex, UserName, "E" + model.gifImg);
        }
 

        public void ShowShake()
        {
            Thread t = new Thread(() => Shake());
            t.IsBackground = true;
            t.Start();

        }

        private void Shake()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    this.Top += 10;
                    this.Left += 10;
                    Thread.Sleep(100);
                    this.Top -= 10;
                    this.Left -= 10;
                }
            });

        }
        #endregion //接口调用的方法
    }
}
