using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tobii.Interaction;
using System.Windows.Input;

namespace TobiiProject
{
    static class Program
    {
        struct Rect { public int left, top, right, bottom; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            const UInt32 WM_KEYDOWN = 0x0100;
            const UInt32 WM_KEYUP = 0x0101;
            const int VK_F5 = 0x74;

            int xCord = 0;
            int yCord = 0;

            Form circleForm;

            var host = new Host();
            var gazePointDataStream = host.Streams.CreateGazePointDataStream();

            gazePointDataStream.GazePoint((x, y, ts) => { xCord = (int)x; yCord = (int)y; });




            Hook.GlobalEvents().OnCombination(new Dictionary<Combination, Action>
                {
                    /*{Combination.FromString("Shift+Alt+Enter"), () => { gazePointDataStream.GazePoint((x, y, ts) => 
                            SetCursorPos(
                                  (int)x,
                                  (int)y
                                ));
                        }
                    },*/
                    // Sets the cursor position the the currently gazed at point
                    {Combination.FromString("Shift+Alt+I"), () => {
                            SetCursorPos(xCord,yCord);
                        }
                    },

                    // Sets the currently gazed at window active
                    {Combination.FromString("Shift+Alt+F"), () => {
                            SetForegroundWindow(WindowFromPoint(new Point(xCord,yCord)));
                        }
                    },
                    // Send input to message without changing focus
                    {Combination.FromString("Shift+Alt+L"), () => {

                            IntPtr activeWindow = Process.GetCurrentProcess().MainWindowHandle;
                            IntPtr newWindow = WindowFromPoint(new Point(xCord,yCord));
                            
                            SetForegroundWindow(newWindow);
                            SetActiveWindow(newWindow);


                            PostMessage(newWindow, WM_KEYDOWN, VK_F5, 0);
                            PostMessage(newWindow, WM_KEYUP, VK_F5, 0);
                        }
                    },
                    // Feedback for the user (red circle)
                    {Combination.FromString("Shift+Alt+Z"), () => {
                            //IntPtr childWindow = WindowFromPoint(new Point(xCord,yCord));
                            //IntPtr parentWindow = GetParent(childWindow);
                            //SetForegroundWindow(parentWindow);
                            //SetActiveWindow(parentWindow);
                            //DrawRect(parentWindow, 5.0f);

                            circleForm = new Form2();
                            circleForm.Show();
                            circleForm.Location = new Point(Cursor.Position.X, Cursor.Position.Y);


                           //DrawPoint(Cursor.Position.X,Cursor.Position.Y, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height / 10);
                        }
                    },
                    // clicking at gaze position then returning mouse to old position
                    {Combination.FromString("Alt+P"), () => {
                            int oldX = Cursor.Position.X;
                            int oldY = Cursor.Position.Y;
                            SetCursorPos(xCord,yCord);
                            DoMouseClick();
                            SetCursorPos(oldX, oldY);
                        }
                    }


        });

            // launch the WinForms application like normal
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            

        }

        public static void DrawPoint(int xCord, int yCord, int pointR) { 
            IntPtr desktopPtr = GetDC(IntPtr.Zero);
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);

            Console.WriteLine("g X size: " + g.DpiX);
            Console.WriteLine("g Y size: " + g.DpiY);

            Console.WriteLine("Screen width: " + System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width);
            Console.WriteLine("Screen height: " + System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);

            Console.WriteLine("Mouse X: " + Cursor.Position.X);
            Console.WriteLine("Mouse Y: " + Cursor.Position.Y);

            SolidBrush b = new SolidBrush(Color.Red);
            //g.FillRectangle(b, new System.Drawing.Rectangle(0, 0, 1366, 768));
            g.FillEllipse(b, new System.Drawing.Rectangle(xCord+20, yCord+30, pointR, pointR));
 
        }

        public static void DrawRect(IntPtr hWnd, float penWidth)
        {

            Rect rc = new Rect();
            GetWindowRect(hWnd, out rc);

            IntPtr hDC = GetWindowDC(hWnd);

            if (hDC != IntPtr.Zero)
            {
                using (Pen pen = new Pen(Color.Red, penWidth))
                {
                    using (Graphics g = Graphics.FromHdc(hDC))
                    {
                        Color blueTransp1 = Color.FromArgb(50, 0, 0, 255);
                        Color blueTransp2 = Color.Transparent;
                        System.Drawing.Rectangle rect = new
                            System.Drawing.Rectangle(0, 0, rc.right - rc.left - (int)penWidth,
                            rc.bottom - rc.top - (int)penWidth);

                        LinearGradientBrush transBrush =
                            new LinearGradientBrush(rect, blueTransp2, blueTransp1, (float)45, true);
                        //GraphicsState gs = g.Save();
                        g.DrawRectangle(pen, rect);
                        //System.Threading.Thread.Sleep(1000);
                        //g.Restore(gs);
                    }
                }
            }
            ReleaseDC(hWnd, hDC);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        
        //Mouse actions
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        public static void DoMouseClick()
        {
            //Call the imported function with the cursor's current position
            uint X = (uint)Cursor.Position.X;
            uint Y = (uint)Cursor.Position.Y;
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
        }

        [DllImport("User32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("User32.dll")]
        public static extern void ReleaseDC(IntPtr hwnd, IntPtr dc);

        [DllImport("user32")]
        static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32")]
        static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32")]
        static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);


        [DllImport("user32")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        [DllImport("user32")]
        public static extern IntPtr WindowFromPoint(Point point);

        [DllImport("user32")]
        public static extern bool SetActiveWindow(IntPtr hWnd);

        [DllImport("user32")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32")]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern uint SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);


    }


    public partial class Form2 : Form
    {

        public Form2()
        {
            this.Opacity = .9;
            this.TopMost = true;
            this.BackColor = Color.Red;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Size = new System.Drawing.Size(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / 20, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / 20);

            // Makes the form circular:
            System.Drawing.Drawing2D.GraphicsPath GP = new System.Drawing.Drawing2D.GraphicsPath();
            GP.AddEllipse(this.ClientRectangle);
            this.Region = new Region(GP);

            System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();
            timer1.Interval = 20;//5 seconds
            timer1.Tick += new System.EventHandler(timer1_Tick);
            timer1.Start();

        }

        const int WS_EX_TRANSPARENT = 0x20;

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle = cp.ExStyle | WS_EX_TRANSPARENT;
                return cp;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if ((GetKeyState(Keys.LMenu) & (1 << 16)) == 0){
                this.DestroyHandle();
            }
            Point pt = Cursor.Position;
            pt.Offset(-1 * this.Width / 5, -1 * this.Height / 5);
            this.Location = pt;
        }

        [DllImport("user32.dll")]
        public static extern short GetKeyState(Keys key);
    }
}
