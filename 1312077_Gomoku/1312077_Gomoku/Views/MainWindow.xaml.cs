using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;
using System.Windows.Threading;

namespace _1312077_Gomoku.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            worker.DoWork += calculate;
            worker.RunWorkerCompleted += AIPlay;            
        }

        int mode = 0;
        double m_BoardWidthFactor, m_BoardHeightFactor;
        int[,] m_board = new int[12, 12];
        int turn = 1;
        Point lastPlay = new Point();
        bool AI = false;
        private readonly BackgroundWorker worker = new BackgroundWorker();

        bool turnAI = false;
        void calculate(object sender, DoWorkEventArgs e)
        {           
            //Thread.Sleep(3000);
            Random ran = new Random();
            int x, y;
            do
            {
                x = ran.Next(0, 12);
                y = ran.Next(0, 12);
            } while (m_board[x, y] != 0);
            e.Result = new Point(x, y);            
        }        
        void AIPlay(object sender, RunWorkerCompletedEventArgs e)
        {
            Point t = (Point)e.Result;
            if (mode == 4)
                MyStep((int)t.X, (int)t.Y);
            else
                playAt((int)t.X, (int)t.Y);
            turnAI = false;            
        }
        void computerThinking()
        {                       
            int dem = 0, old;            
            // Phương Ngang
            old = dem = check(0, 1) + check(0, -1);            
            //Phương Dọc
            dem = check(1, 0) + check(-1, 0);
            if (old < dem)
            {
                old = dem;
            }
            //Phương chéo Trên trái => Dưới phải
            dem = check(-1, -1) + check(1, 1);            
            //Phương chéo Trên phải => Dưới trái
            dem = check(-1, 1) + check(1, -1);

            turnAI = true;
            worker.RunWorkerAsync();            
        }
        void gameOver()
        {
            int win = m_board[(int)lastPlay.X, (int)lastPlay.Y];
            string mess = "";
            if (win == 1)
                mess = "Người chơi màu xanh thắng";
            if(win ==2)
            {
                if (AI == true)
                    mess = "Máy chiến thắng";
                else
                    mess = "Người chơi màu đỏ thắng";
            }            
            MessageBox.Show(mess);
            board.IsHitTestVisible = false;
            change_start.Content = "Start";
        }
        int check(int dx, int dy)
        {
            int result = 0;
            int hang = (int)lastPlay.X + dx, cot = (int)lastPlay.Y + dy;
            int x = (int)lastPlay.X, y = (int)lastPlay.Y;
            while((hang>=0 && hang<12) && (cot>=0 && cot<12))
            {
                if (m_board[x, y] == m_board[hang, cot])
                {
                    result++;                    
                }
                else
                    break;
                hang = hang + dx;
                cot = cot + dy;
            }
            return result + 1;
        }
        private void checkWin()
        {
            int m_turn = m_board[(int)lastPlay.X, (int)lastPlay.Y];
            int dem = 0;
            // Phương Ngang
            dem = check(0, 1) + check(0, -1);            
            if (dem>5)
            {
                gameOver();
                return;
            }
            //Phương Dọc
            dem = check(1, 0) + check(-1, 0);
            if (dem > 5)
            {
                //MessageBox.Show("Test1");
                gameOver();
                return;
            }
            //Phương chéo Trên trái => Dưới phải
            dem = check(-1, -1) + check(1, 1);
            if (dem > 5)
            {
                //MessageBox.Show("Test2");
                gameOver();
                return;
            }
            //Phương chéo Trên phải => Dưới trái
            dem = check(-1, 1) + check(1, -1);
            if (dem > 5)
            {
                //MessageBox.Show("Test3");
                gameOver();
                return;
            }
        }
        void putACheckOnBoard(int row, int col, int type)
        {
            Ellipse ball = new Ellipse();
            double left, top;
            left = col * m_BoardWidthFactor;
            top = row * m_BoardWidthFactor;

            ball.Width = ball.Height = m_BoardWidthFactor;
            ball.SetValue(Canvas.TopProperty, top);
            ball.SetValue(Canvas.LeftProperty, left);

            m_board[row, col] = type;

            if (type == 1)
                ball.Fill = this.FindResource("ballBlue") as Brush;
            else
                ball.Fill = this.FindResource("whiteStoneBrush") as Brush;
            board.Children.Add(ball);
        }
        private void playAt(int x, int y)
        {
            if (m_board[x,y]!=0)
            {
                MessageBox.Show("Vị trí này đã được đánh!");
                return;
            }
            putACheckOnBoard(x, y, turn);
            if (turn == 1)
                turn = 2;
            else
                turn = 1;
          
            lastPlay.X = x;
            lastPlay.Y = y;
            
            checkWin();
        }
        private void board_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (turnAI)
            {
                MessageBox.Show("Chưa đến lượt đánh của bạn!");
                return;
            }
            Point point = e.GetPosition((IInputElement)sender);
            int x = (int)(point.Y / m_BoardWidthFactor);
            int y = (int)(point.X / m_BoardWidthFactor);

            switch (mode)
            {
                case 1:
                case 2:
                    playAt(x, y);
                    if (AI == true && turn == 2)
                        computerThinking();
                    break;
                case 3:
                    MyStep(x, y);
                    break;
                default:
                    break;
            }
        }

        #region Review
        Ellipse dot = new Ellipse();
        bool IsDot = false;
        private void board_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsDot)
            {
                board.Children.Remove(dot);
                IsDot = false;
            }
            Point point = e.GetPosition((IInputElement)sender);
            int x = (int)(point.X / m_BoardWidthFactor);
            int y = (int)(point.Y / m_BoardWidthFactor);
            if (x > 11 || y > 11)
                return;
            if (m_board[y, x] != 0)
                return;
            double top, left;
                        
            left = x * m_BoardWidthFactor;
            top = y * m_BoardWidthFactor;
            dot.Width = dot.Height = m_BoardWidthFactor;
            dot.Fill = this.FindResource("ballDot") as Brush;
            dot.SetValue(Canvas.TopProperty, top);
            dot.SetValue(Canvas.LeftProperty, left);
            board.Children.Add(dot);
            IsDot = true;
        }
        private void board_MouseLeave(object sender, MouseEventArgs e)
        {
            if (IsDot)
            {
                board.Children.Remove(dot);
                IsDot = false;
            }
        }
        #endregion

        private void writeAMess(string _name, string _mess)
        {
            Grid mess = new Grid();
            mess.Width = 300;

            Label name = new Label();
            name.FontSize = 14;
            name.FontWeight = FontWeights.Bold;
            name.Content = _name;
            name.HorizontalAlignment = HorizontalAlignment.Left;
            mess.Children.Add(name);

            Label time = new Label();
            time.Content = DateTime.Now.ToString("HH:mm:ss");
            time.HorizontalAlignment = HorizontalAlignment.Right;
            mess.Children.Add(time);

            StackPanel stackMess = new StackPanel();
            stackMess.Margin = new Thickness(0, 30, 0, 10);
            TextBlock messText = new TextBlock();
            messText.Text = _mess;
            messText.TextWrapping = TextWrapping.Wrap;
            stackMess.Children.Add(messText);
            Rectangle deco = new Rectangle();
            deco.StrokeDashArray = new DoubleCollection(new double[2] { 1, 2 });
            deco.Stroke = Brushes.Gray;
            deco.Margin = new Thickness(0, 10, 0, 0);
            stackMess.Children.Add(deco);
            mess.Children.Add(stackMess);

            ListBoxItem item = new ListBoxItem();
            item.Content = mess;
            chat.Items.Add(item);         
            chat.ScrollIntoView(chat.Items[chat.Items.Count - 1]);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Messages.Text != "")
            {
                switch (mode)
                {
                    case 1:
                    case 2:
                        writeAMess(player.Text, Messages.Text);
                        break;
                    case 3:
                    case 4:
                        socket.Emit("ChatMessage", Messages.Text);
                        break;
                }             
                Messages.Text = "";
            }
        }

        private void change_start_Click(object sender, RoutedEventArgs e)
        {
            if (change_start.Content.ToString() == "Start")
            {
                Option dlg = new Option();
                dlg.Owner = this;
                if (dlg.ShowDialog() == false)
                    return;
                mode = dlg.m_option;
                switch(mode)
                {
                    case 1:
                        AI = false;
                        turn = 1;
                        break;
                    case 2:
                        AI = true;
                        turn = 1;
                        break;
                    case 3:
                        if (socket != null)
                            socket.Emit("ConnectToOtherPlayer");
                        else
                            connectToServer();
                        break;
                    case 4:
                        if (socket != null)
                            socket.Emit("ConnectToOtherPlayer");
                        else
                            connectToServer();
                        break;
                }
                                
                board.IsHitTestVisible = true;
                change_start.Content = "Change";
                m_board = new int[12, 12];
                UpdateBoard();                
            }
            if (change_start.Content.ToString() == "Change")
            {
                if (socket != null)
                    socket.Emit("MyNameIs", player.Text);
            }
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (socket != null)
                socket.Close();
        }
        void UpdateBoard()
        {
            m_BoardHeightFactor = m_BoardWidthFactor = board.ActualWidth / 12;
            board.Children.Clear();           
            int m_BoardSize = 12;
            for (int x = 0; x < m_BoardSize; x++)
            {
                for (int y = 0; y < m_BoardSize; y++)
                {
                    double top, left;
                    top = x * m_BoardWidthFactor;
                    left = y * m_BoardWidthFactor;
                    Rectangle rect = new Rectangle();
                    rect.Width = rect.Height = m_BoardWidthFactor;
                    rect.SetValue(Canvas.TopProperty, top);
                    rect.SetValue(Canvas.LeftProperty, left);
                    rect.Fill = Brushes.White;
                    if ((x + y) % 2 == 0)
                        rect.Fill = Brushes.Black;
                    board.Children.Add(rect);
                    if (m_board[x, y] != 0)
                    {
                        Ellipse ball = new Ellipse();
                        ball.Width = ball.Height = m_BoardWidthFactor;
                        if (m_board[x, y] == 1)
                            ball.Fill = this.FindResource("ballBlue") as Brush;
                        else
                            ball.Fill = this.FindResource("whiteStoneBrush") as Brush;
                        ball.SetValue(Canvas.TopProperty, top);
                        ball.SetValue(Canvas.LeftProperty, left);
                        board.Children.Add(ball);
                    }
                }
            }
        }
        private void board_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateBoard();
        }

        #region Mode Online
        Socket socket = null;
        void InvokeOrExecute(Action action)
        {
            var d = Application.Current.Dispatcher;
            if (d.CheckAccess())
                action();
            else
                d.BeginInvoke(DispatcherPriority.Normal, action);
        }
        void MyStep(int hang, int cot)
        {
            socket.Emit("MyStepIs", JObject.FromObject(new { row = hang, col = cot }));
        }
        private void connectToServer()
        {
            string _nameIO, _messIO;
            //socket = IO.Socket("ws://127.0.0.1:8000");
            socket = IO.Socket("ws://gomoku-lajosveres.rhcloud.com:8000");
            //Sự kiện kết nối thành công
            socket.On(Socket.EVENT_CONNECT, () =>
            {
                InvokeOrExecute(()=>writeAMess("Notifications", "Connected"));
            });
            //Sự kiện kết nối bị lỗi
            socket.On(Socket.EVENT_CONNECT_ERROR, (data) =>
            {
                InvokeOrExecute(() => writeAMess("Errors", data.ToString()));               
            });
            //Sự kiện nhận tin nhắn
            socket.On("ChatMessage", (data) =>
            {
                //Kết nối và được server chấp thuận      
                if (((Newtonsoft.Json.Linq.JObject)data)["message"].ToString() == "Welcome!")
                {
                    InvokeOrExecute(() => writeAMess("Server", ((JObject)data)["message"].ToString()));

                    InvokeOrExecute(() => socket.Emit("MyNameIs", player.Text));
                    //socket.Emit("MyNameIs", player.Text);
                    socket.Emit("ConnectToOtherPlayer");
                    socket.Emit("3");
                }
                else
                {
                    _messIO = ((JObject)data)["message"].ToString();
                    //Nhận tin nhắn từ người chơi kia
                    if (((JObject)data)["from"] != null)
                        _nameIO = ((JObject)data)["from"].ToString();
                    //Nhận tin nhắn từ server
                    else
                    {
                        _nameIO = "Server";
                        if (mode == 4)
                        {
                            if (_messIO.EndsWith("You are the first player!"))
                            {                               
                                computerThinking();
                            }                           
                        }
                    }
                    
                    if (_messIO.StartsWith("Guest is now called"))
                        socket.Emit("");
                    InvokeOrExecute(() => writeAMess(_nameIO, _messIO));
                }
            });
            //Bắt sự kiện kết thúc game
            socket.On("EndGame", (data) =>
            {
                _messIO = ((JObject)data)["message"].ToString();
                _nameIO = "Server";
                var d = Application.Current.Dispatcher;                
                InvokeOrExecute(() => writeAMess(_nameIO, _messIO));
                //if (_messIO.EndsWith("left the game!"))
                    InvokeOrExecute(gameOver);
            });
            //Bắt sự kiện nước chơi
            socket.On("NextStepIs", (data) =>
            {
                var player = ((JObject)data)["player"];
                var row = ((JObject)data)["row"];
                var col = ((JObject)data)["col"];                
                InvokeOrExecute(() => putACheckOnBoard((int)row, (int)col, (int)player+1));
                if (mode==4)
                {
                    if ((int)player == 1)
                        computerThinking();
                }
            });
            //Sự kiện lỗi phát sinh trong quá trình trao đổi
            socket.On(Socket.EVENT_ERROR, (data) =>
            {
                InvokeOrExecute(() => writeAMess("Notifications", data.ToString()));
            });
        }
        #endregion
    }
}
