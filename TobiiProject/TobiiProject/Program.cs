using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tobii.Interaction;

namespace TobiiProject
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            int xCord = 0;
            int yCord = 0;
            
            var host = new Host();
            var gazePointDataStream = host.Streams.CreateGazePointDataStream();

            gazePointDataStream.GazePoint((x, y, ts) => { xCord = (int)x; yCord = (int)y; });
            Hook.GlobalEvents().OnCombination(new Dictionary<Combination, Action>
                {
                    {Combination.FromString("Shift+Alt+Enter"), () => { gazePointDataStream.GazePoint((x, y, ts) => 
                            SetCursorPos(
                                  (int)x,
                                  (int)y
                                ));
                        }
                    },
                    {Combination.FromString("Shift+Alt+I"), () => {
                            SetCursorPos(xCord,yCord);
                        }
                    },
                    {Combination.FromString("Shift+Alt+F"), () => {
                            SetForegroundWindow(WindowFromPoint(new Point(xCord,yCord)));
                        }
                    }


        });
            /*SetCursorPos(
                          1300,
                          700
                        );*/

            // launch the WinForms application like normal
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

        }

        [DllImport("user32")]
        public static extern IntPtr WindowFromPoint(Point point);

        [DllImport("user32")]
        public static extern bool SetForegroundWindow(IntPtr process);

        [DllImport("user32")]
        public static extern bool SetCursorPos(int x, int y);
    }
}
