using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tobii.Interaction;

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
                    {Combination.FromString("Shift+Alt+L"), () => {

                            IntPtr activeWindow = Process.GetCurrentProcess().MainWindowHandle;
                            IntPtr newWindow = WindowFromPoint(new Point(xCord,yCord));
                            
                            SetForegroundWindow(newWindow);
                            SetActiveWindow(newWindow);


                            PostMessage(newWindow, WM_KEYDOWN, VK_F5, 0);
                            PostMessage(newWindow, WM_KEYUP, VK_F5, 0);
                        }
                    },
                    {Combination.FromString("Shift+Alt+Z"), () => {
                            IntPtr childWindow = WindowFromPoint(new Point(xCord,yCord));
                            IntPtr parentWindow = GetParent(childWindow);
                            SetForegroundWindow(parentWindow);
                            SetActiveWindow(parentWindow);
                            //DrawRect(parentWindow, 5.0f);
                            DrawPoint(xCord,yCord);
                        }
                    },
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

        public static void DrawPoint(int xCord, int yCord) { 
            IntPtr desktopPtr = GetDC(IntPtr.Zero);
            Graphics g = Graphics.FromHdc(desktopPtr);

            SolidBrush b = new SolidBrush(Color.Red);
            //g.FillRectangle(b, new System.Drawing.Rectangle(0, 0, 1366, 768));
            g.FillEllipse(b, new System.Drawing.Rectangle(xCord-5, yCord - 5, xCord + 5, yCord + 5));
            g.Dispose();
            ReleaseDC(IntPtr.Zero, desktopPtr);
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
}
