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
using Interceptor;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using Keys = Interceptor.Keys;
using System.Runtime.InteropServices;

namespace BMS2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            second_start();

            ni = new NotifyIcon();
            ni.Icon = Properties.Resources.Robot;
            ni.Visible = true;
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                this.Hide();

            if (WindowState == WindowState.Normal)
            {
                this.Show();
                this.Activate();
            }

            base.OnStateChanged(e);
        }

        NotifyIcon ni;

        private IntPtr context;
        private int device, devk = 3, devm = 11;

        bool m = false, work = true, pause_exit = false, mss = false;

        bool mt = false;

        bool mini = false;

        bool b1, b2, b3, b4, bf, bl, bp, bpw, bss, bv, bs1, bsz, bs3, bs4;

        int b11 = 30, b12 = 30, b21 = 30, b22 = 30, b31 = 30, b32 = 30, b41 = 30, b42 = 30, bf1 = 30, bf2 = 30, bl1 = 30, bl2 = 30, bp1 = 30, bp2 = 30, bv1 = 30, bv2 = 30;

        Color color_green= Color.FromRgb(190, 255, 190);
        Color color_red = Color.FromRgb(230, 160, 230);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

        enum ShowWindowCommands
        {
            /// <summary>
            /// Hides the window and activates another window.
            /// </summary>
            Hide = 0,
            /// <summary>
            /// Activates and displays a window. If the window is minimized or 
            /// maximized, the system restores it to its original size and position.
            /// An application should specify this flag when displaying the window 
            /// for the first time.
            /// </summary>
            Normal = 1,
            /// <summary>
            /// Activates the window and displays it as a minimized window.
            /// </summary>
            ShowMinimized = 2,
            /// <summary>
            /// Maximizes the specified window.
            /// </summary>
            Maximize = 3, // is this the right value?
                          /// <summary>
                          /// Activates the window and displays it as a maximized window.
                          /// </summary>       
            ShowMaximized = 3,
            /// <summary>
            /// Displays a window in its most recent size and position. This value 
            /// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except 
            /// the window is not activated.
            /// </summary>
            ShowNoActivate = 4,
            /// <summary>
            /// Activates the window and displays it in its current size and position. 
            /// </summary>
            Show = 5,
            /// <summary>
            /// Minimizes the specified window and activates the next top-level 
            /// window in the Z order.
            /// </summary>
            Minimize = 6,
            /// <summary>
            /// Displays the window as a minimized window. This value is similar to
            /// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the 
            /// window is not activated.
            /// </summary>
            ShowMinNoActive = 7,
            /// <summary>
            /// Displays the window in its current size and position. This value is 
            /// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the 
            /// window is not activated.
            /// </summary>
            ShowNA = 8,
            /// <summary>
            /// Activates and displays the window. If the window is minimized or 
            /// maximized, the system restores it to its original size and position. 
            /// An application should specify this flag when restoring a minimized window.
            /// </summary>
            Restore = 9,
            /// <summary>
            /// Sets the show state based on the SW_* value specified in the 
            /// STARTUPINFO structure passed to the CreateProcess function by the 
            /// program that started the application.
            /// </summary>
            ShowDefault = 10,
            /// <summary>
            ///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread 
            /// that owns the window is not responding. This flag should only be 
            /// used when minimizing windows from a different thread.
            /// </summary>
            ForceMinimize = 11
        }

        private void second_start()
        {            
        Process proc = Process.GetCurrentProcess();
            int curProc = proc.Id;
            Process[] procs = Process.GetProcessesByName("BMS2");
            foreach (Process pr in procs)
            {
                if (pr.Id != curProc)
                {
                    IntPtr hwnd = pr.MainWindowHandle; 
                    if (hwnd.ToInt32() == 0) hwnd = FindWindowByCaption(IntPtr.Zero, Window_Form.Title);
                    ShowWindow(hwnd, ShowWindowCommands.Restore);
                    
                    Process.GetCurrentProcess().Kill();
                }
            }
        }

        private Thread callbackThread;        

        private event EventHandler<KeyPressedEventArgs> OnKeyPressed;
        private event EventHandler<MousePressedEventArgs> OnMousePressed;

        private void hook()
        {
            bool s = true;
            bool l1 = true;
            bool l2 = true;

            Stroke stroke = new Stroke();
            InterceptionDriver.SetFilter(context, InterceptionDriver.IsKeyboard, (Int32)KeyboardFilterMode.All);
            InterceptionDriver.SetFilter(context, InterceptionDriver.IsMouse, (Int32)MouseFilterMode.All);

            //keyboard: device=3
            //mouse: device=11

            while (InterceptionDriver.Receive(context, device = InterceptionDriver.Wait(context), ref stroke, 1) > 0)
            {
                s = true;
                if (InterceptionDriver.IsMouse(device) > 0)
                {
                    if (l1)
                    {
                        l1 = false;
                        devm = device;
                        Dispatcher.Invoke(() => { label1.Content = device; });
                    }

                    if (OnMousePressed != null)
                    {
                        var args = new MousePressedEventArgs() { X = stroke.Mouse.X, Y = stroke.Mouse.Y, State = stroke.Mouse.State, Rolling = stroke.Mouse.Rolling };
                        OnMousePressed(this, args);

                        if (args.Handled)
                        {
                            continue;
                        }
                        stroke.Mouse.X = args.X;
                        stroke.Mouse.Y = args.Y;
                        stroke.Mouse.State = args.State;
                        stroke.Mouse.Rolling = args.Rolling;
                    }
                }

                if (InterceptionDriver.IsKeyboard(device) > 0)
                {
                    if (l2)
                    {
                        l2 = false;
                        devk = device;
                        Dispatcher.Invoke(() => { label2.Content = device; });
                    }

                    if (stroke.Key.Code == Keys.Tilde)
                    {
                        if (work)
                        {
                            if (bss)
                            {
                                if (stroke.Key.State == KeyState.Down)
                                {
                                    if (mss == false)
                                    {
                                        m = false;
                                        mss = true;
                                        new Thread(macross).Start();
                                    }
                                }
                                else
                                if (stroke.Key.State == KeyState.Up)
                                {
                                    if (mss == true) { mss = false; }
                                }

                                s = false;
                            }
                        }
                    }

                    if (stroke.Key.Code == Keys.One)
                    {
                        if (work)
                        {
                            if (bs1)
                            {
                                if (stroke.Key.State == KeyState.Down)
                                {
                                    m = false;
                                }
                            }
                        }
                    }

                    if (stroke.Key.Code == Keys.Z)
                    {
                        if (work)
                        {
                            if (bsz)
                            {
                                if (stroke.Key.State == KeyState.Down)
                                {
                                    m = false;
                                }
                            }
                        }
                    }

                    if (stroke.Key.Code == Keys.Three)
                    {
                        if (work)
                        {
                            if (bs3)
                            {
                                if (stroke.Key.State == KeyState.Down)
                                {
                                    m = false;
                                }
                            }
                        }
                    }

                    if (stroke.Key.Code == Keys.Four)
                    {
                        if (work)
                        {
                            if (bs4)
                            {
                                if (stroke.Key.State == KeyState.Down)
                                {
                                    m = false;
                                }
                            }
                        }
                    }

                    if (stroke.Key.Code == Keys.R)
                    {
                        if (work)
                        {
                            if (bpw)
                            {
                                if (stroke.Key.State == KeyState.Up)
                                {
                                    if (m == false)
                                    {
                                        m = true;
                                        if (mt) new Thread(macro_text).Start();
                                        else
                                            new Thread(macro).Start();
                                    }
                                    else
                                    {
                                        m = false;
                                    }
                                }
                            }
                            else
                            {
                                if (stroke.Key.State == KeyState.Down)
                                {
                                    if (m == false)
                                    {
                                        m = true;
                                        if (mt) new Thread(macro_text).Start();
                                        else
                                            new Thread(macro).Start();
                                    }
                                }
                                else
                                if (stroke.Key.State == KeyState.Up)
                                {
                                    if (m == true) { m = false; }
                                }
                            }

                            s = false;
                        }
                    }

                    if (stroke.Key.Code == Keys.Insert)
                    {
                        if (stroke.Key.State == KeyState.E0)
                        {
                            if (pause_exit) Stop();
                            new Thread(timer).Start();
                            new Thread(ch_work).Start();
                            s = false;
                        }
                    }

                    if (stroke.Key.Code == Keys.Enter)
                    {
                        if (stroke.Key.State == KeyState.Down)
                        {
                            if (work == true)
                            {
                                new Thread(ch_work).Start();
                                m = false;
                            }
                        }
                    }

                    if (OnKeyPressed != null)
                    {
                        var args = new KeyPressedEventArgs() { Key = stroke.Key.Code, State = stroke.Key.State };
                        OnKeyPressed(this, args);

                        if (args.Handled)
                        {
                            continue;
                        }
                        stroke.Key.Code = args.Key;
                        stroke.Key.State = args.State;
                    }
                }

                if (s) InterceptionDriver.Send(context, device, ref stroke, 1);
            }

            Stop();
        }

        private void macro()
        {
            while (m)
            {

                if (m)
                    if (bp)
                    {
                        SendMouseEvent(MouseState.RightDown);
                        time(bp1);
                        SendMouseEvent(MouseState.RightUp);
                        time(bp2);
                    }

                if (m)
                    if (bl)
                    {
                        SendMouseEvent(MouseState.LeftDown);
                        time(bl1);
                        SendMouseEvent(MouseState.LeftUp);
                        time(bl2);
                    }

                if (m)
                    if (bf)
                    {
                        SendKey(Keys.F, KeyState.Down);
                        time(bf1);
                        SendKey(Keys.F, KeyState.Up);
                        time(bf2);
                    }

                if (m)
                    if (b1)
                    {
                        SendKey(Keys.One, KeyState.Down);
                        time(b11);
                        SendKey(Keys.One, KeyState.Up);
                        time(b12);
                    }

                if (m)
                    if (b2)
                    {
                        SendKey(Keys.Two, KeyState.Down);
                        time(b21);
                        SendKey(Keys.Two, KeyState.Up);
                        time(b22);
                    }

                if (m)
                    if (b3)
                    {
                        SendKey(Keys.Three, KeyState.Down);
                        time(b31);
                        SendKey(Keys.Three, KeyState.Up);
                        time(b32);
                    }

                if (m)
                    if (b4)
                    {
                        SendKey(Keys.Four, KeyState.Down);
                        time(b41);
                        SendKey(Keys.Four, KeyState.Up);
                        time(b42);
                    }

                if (m)
                    if (bv)
                    {
                        SendKey(Keys.V, KeyState.Down);
                        time(bv1);
                        SendKey(Keys.V, KeyState.Up);
                        time(bv2);
                    }
            }
        }

        private void macross()
        {
            while (mss)
            {
                if (mss)
                {
                    if (bss)
                    {
                        SendKey(Keys.S, KeyState.Down);
                        time(30);
                        SendKey(Keys.S, KeyState.Up);
                        time(30);
                    }
                }
            }
        }

        private void macro_text()
        {
            try
            {
                string text = "";
                Dispatcher.Invoke(() => { text = textBox.Text; });
                int t = 20;

                while (m)
                {
                    for (int i = 0; i < text.Length; i++)
                    {
                        switch (text[i])
                        {
                            case 'l':
                                if (!m) break;
                                SendMouseEvent(MouseState.LeftDown);
                                time(t);
                                SendMouseEvent(MouseState.LeftUp);
                                time(t);
                                break;
                            case 'p':
                                if (!m) break;
                                SendMouseEvent(MouseState.RightDown);
                                time(t);
                                SendMouseEvent(MouseState.RightUp);
                                time(t);
                                break;
                            case '.':
                                if (!m) break;
                                time(10);
                                break;
                            case 't':
                                if (!m) break;
                                SendKey(Keys.Tab, KeyState.Down);
                                time(t);
                                SendKey(Keys.Tab, KeyState.Up);
                                time(t);
                                break;
                            case 'q':
                            case 'e':
                            case 'f':
                            case 'z':
                            case 'x':
                            case 'c':
                            case 'v':
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                                if (!m) break;
                                SendKey(param[text[i]], KeyState.Down);
                                time(t);
                                SendKey(param[text[i]], KeyState.Up);
                                time(t);
                                break;
                            default:
                                break;
                        }
                    }

                }
            }
            catch { Stop(); }
        }

        private void ch_work()
        {
            if (work)
            {
                work = false;
                m = false;
                Dispatcher.Invoke(() => { grid.Background = new SolidColorBrush(color_red); });
                ni.Icon = Properties.Resources.Robot;

                ni.BalloonTipText = "BMS off-line";
                ni.ShowBalloonTip(1000);
            }
            else
            {
                work = true;
                Dispatcher.Invoke(() => { grid.Background = new SolidColorBrush(color_green); });
                ni.Icon = Properties.Resources.Robot_green;
                
                ni.BalloonTipText = "BMS on-line";
                ni.ShowBalloonTip(1000);
            }
        }

        private void timer()
        {
            pause_exit = true;
            DateTime dt = DateTime.Now;
            while ((DateTime.Now - dt).TotalMilliseconds < 500)
            {
                Thread.Sleep(100);
            }
            pause_exit = false;
        }

        private void time(int p)
        {
            Thread.Sleep(p);
        }
        
        private void Start()
        {            
            context = InterceptionDriver.CreateContext();

            ni.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.None;
            ni.BalloonTipTitle = "BNS2";

            if (context != IntPtr.Zero)
            {
                callbackThread = new Thread(new ThreadStart(hook));
                callbackThread.Priority = ThreadPriority.Highest;
                callbackThread.IsBackground = true;
                callbackThread.Start();                
            }
            ch_work();
        }
        
        private void mi_cat_Click(object sender, RoutedEventArgs e)
        {
            _checkboxclear();            
            checkBox_l.IsChecked = true;
            checkBox_p.IsChecked = true;
            checkBox_PW.IsChecked = true;
            checkBox_s4.IsChecked = true;
        }

        private void mi_lock_Click(object sender, RoutedEventArgs e)
        {
            _checkboxclear();
            checkBox_p.IsChecked = true;
            checkBox_4.IsChecked = true;
            checkBox_f.IsChecked = true;            
            checkBox_s1.IsChecked = true;
            checkBox_sz.IsChecked = true;
            checkBox_s3.IsChecked = true;            
            checkBox_PW.IsChecked = true;
            textBox.Text = "p.f.p4lv";
        }

        private void mi_sin_Click(object sender, RoutedEventArgs e)
        {
            _checkboxclear();
            checkBox_f.IsChecked = true;
            checkBox_4.IsChecked = true;
            checkBox_p.IsChecked = true;
            textBox.Text = "p.4.pf.";
        }

        private void mi_fm_Click(object sender, RoutedEventArgs e)
        {
            _checkboxclear();
            checkBox_l.IsChecked = true;
            checkBox_p.IsChecked = true;
            checkBox_2.IsChecked = true;            
        }

        private void mi_ci_Click(object sender, RoutedEventArgs e)
        {
            _checkboxclear();
            checkBox_l.IsChecked = true;
            checkBox_p.IsChecked = true;
            checkBox_f.IsChecked = true;            
        }

        private void _checkboxclear()
        {
            textBox.Text = "";
            checkBox_1.IsChecked = false;
            checkBox_2.IsChecked = false;
            checkBox_3.IsChecked = false;
            checkBox_4.IsChecked = false;
            checkBox_p.IsChecked = false;
            checkBox_l.IsChecked = false;
            checkBox_f.IsChecked = false;
            checkBox_v.IsChecked = false;
            checkBox_s1.IsChecked = false;
            checkBox_sz.IsChecked = false;
            checkBox_s3.IsChecked = false;
            checkBox_s4.IsChecked = false;
            checkBox_PW.IsChecked = false;
        }
        
        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textBox.Text.Length > 0)
            {
                mt = true;
                checkBox_1.IsEnabled = false;
                checkBox_2.IsEnabled = false;
                checkBox_3.IsEnabled = false;
                checkBox_4.IsEnabled = false;
                checkBox_p.IsEnabled = false;
                checkBox_l.IsEnabled = false;
                checkBox_f.IsEnabled = false;
                checkBox_v.IsEnabled = false;
            }
            else
            {
                mt = false;
                checkBox_1.IsEnabled = true;
                checkBox_2.IsEnabled = true;
                checkBox_3.IsEnabled = true;
                checkBox_4.IsEnabled = true;
                checkBox_p.IsEnabled = true;
                checkBox_l.IsEnabled = true;
                checkBox_f.IsEnabled = true;
                checkBox_v.IsEnabled = true;
            }
        }

        private void checkBox_1_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            win_pause _p = new win_pause(b11, b12);
            _p.Owner = this;
            _p.ShowDialog();
            b11 = _p.p1;
            b12 = _p.p2;
        }

        private void checkBox_2_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            win_pause _p = new win_pause(b21, b22);
            _p.Owner = this;
            _p.ShowDialog();
            b21 = _p.p1;
            b22 = _p.p2;
        }

        private void checkBox_3_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            win_pause _p = new win_pause(b31, b32);
            _p.Owner = this;
            _p.ShowDialog();
            b31 = _p.p1;
            b32 = _p.p2;
        }

        private void checkBox_4_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            win_pause _p = new win_pause(b41, b42);
            _p.Owner = this;
            _p.ShowDialog();
            b41 = _p.p1;
            b42 = _p.p2;
        }

        private void grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            mini = true;
        }

        private void grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (mini) WindowState = WindowState.Minimized;
        }

        private void grid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            mini = false;
        }
        
        private void checkBox_v_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            win_pause _p = new win_pause(bv1, bv2);
            _p.Owner = this;
            _p.ShowDialog();
            bv1 = _p.p1;
            bv2 = _p.p2;
        }

        private void checkBox_p_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            win_pause _p = new win_pause(bp1, bp2);
            _p.Owner = this;
            _p.ShowDialog();
            bp1 = _p.p1;
            bp2 = _p.p2;
        }

        private void checkBox_l_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            win_pause _p = new win_pause(bl1, bl2);
            _p.Owner = this;
            _p.ShowDialog();
            bl1 = _p.p1;
            bl2 = _p.p2;
        }
                
        private void checkBox_f_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            win_pause _p = new win_pause(bf1, bf2);
            _p.Owner = this;
            _p.ShowDialog();
            bf1 = _p.p1;
            bf2 = _p.p2;
        }

        private void Stop()
        {
            if (context != IntPtr.Zero)
            {               
                InterceptionDriver.DestroyContext(context);                
            }
            ni.Dispose();

            Process.GetCurrentProcess().Kill();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Stop();
        }

        private Dictionary<char, Interceptor.Keys> param = new Dictionary<char, Interceptor.Keys>();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Start();

            param.Add('1', Keys.One);
            param.Add('2', Keys.Two);
            param.Add('3', Keys.Three);
            param.Add('4', Keys.Four);
            param.Add('q', Keys.Q);
            param.Add('e', Keys.E);
            param.Add('z', Keys.Z);
            param.Add('x', Keys.X);
            param.Add('c', Keys.C);
            param.Add('v', Keys.V);
            param.Add('f', Keys.F);

        }

        private void SendKey(Keys key, KeyState state)
        {
            try
            {
                Stroke stroke = new Stroke();
                KeyStroke keyStroke = new KeyStroke();

                keyStroke.Code = key;
                keyStroke.State = state;

                stroke.Key = keyStroke;

                InterceptionDriver.Send(context, devk, ref stroke, 1);
            }
            catch { Stop(); }
        }

        private void SendMouseEvent(MouseState state)
        {
            try
            {
                Stroke stroke = new Stroke();
                MouseStroke mouseStroke = new MouseStroke();

                mouseStroke.State = state;

                if (state == MouseState.ScrollUp)
                {
                    mouseStroke.Rolling = 120;
                }
                else if (state == MouseState.ScrollDown)
                {
                    mouseStroke.Rolling = -120;
                }

                stroke.Mouse = mouseStroke;

                InterceptionDriver.Send(context, devm, ref stroke, 1);
            }
            catch { Stop(); }
        }

        private void checkBox_1_Checked(object sender, RoutedEventArgs e)
        {
            b1 = true;
        }

        private void checkBox_1_Unchecked(object sender, RoutedEventArgs e)
        {
            b1 = false;
        }

        private void checkBox_2_Checked(object sender, RoutedEventArgs e)
        {
            b2 = true;
        }

        private void checkBox_2_Unchecked(object sender, RoutedEventArgs e)
        {
            b2 = false;
        }

        private void checkBox_3_Checked(object sender, RoutedEventArgs e)
        {
            b3 = true;
        }

        private void checkBox_3_Unchecked(object sender, RoutedEventArgs e)
        {
            b3 = false;
        }

        private void checkBox_4_Checked(object sender, RoutedEventArgs e)
        {
            b4 = true;
        }

        private void checkBox_4_Unchecked(object sender, RoutedEventArgs e)
        {
            b4 = false;
        }

        private void checkBox_p_Checked(object sender, RoutedEventArgs e)
        {
            bp = true;
        }

        private void checkBox_p_Unchecked(object sender, RoutedEventArgs e)
        {
            bp = false;
        }

        private void checkBox_l_Checked(object sender, RoutedEventArgs e)
        {
            bl = true;
        }

        private void checkBox_l_Unchecked(object sender, RoutedEventArgs e)
        {
            bl = false;
        }

        private void checkBox_f_Checked(object sender, RoutedEventArgs e)
        {
            bf = true;
        }

        private void checkBox_f_Unchecked(object sender, RoutedEventArgs e)
        {
            bf = false;
        }

        private void checkBox_ss_Checked(object sender, RoutedEventArgs e)
        {
            bss = true;
        }

        private void checkBox_ss_Unchecked(object sender, RoutedEventArgs e)
        {
            bss = false;
        }

        private void checkBox_v_Checked(object sender, RoutedEventArgs e)
        {
            bv = true;
        }

        private void checkBox_v_Unchecked(object sender, RoutedEventArgs e)
        {
            bv = false;
        }

        private void checkBox_s1_Checked(object sender, RoutedEventArgs e)
        {
            bs1 = true;
        }

        private void checkBox_s1_Unchecked(object sender, RoutedEventArgs e)
        {
            bs1 = false;
        }

        private void checkBox_sz_Checked(object sender, RoutedEventArgs e)
        {
            bsz = true;
        }

        private void checkBox_sz_Unchecked(object sender, RoutedEventArgs e)
        {
            bsz = false;
        }
                
        private void checkBox_s3_Checked(object sender, RoutedEventArgs e)
        {
            bs3 = true;
        }

        private void checkBox_s3_Unchecked(object sender, RoutedEventArgs e)
        {
            bs3 = false;
        }

        private void checkBox_s4_Checked(object sender, RoutedEventArgs e)
        {
            bs4 = true;
        }

        private void checkBox_s4_Unchecked(object sender, RoutedEventArgs e)
        {
            bs4 = false;
        }

        private void checkBox_PW_Checked(object sender, RoutedEventArgs e)
        {
            bpw = true;
        }

        private void checkBox_PW_Unchecked(object sender, RoutedEventArgs e)
        {
            bpw = false;
        }

        
    }
}
/*проверка повторного запуска
развернуть по клику
меню скриптов по пкм в трее
*/