using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
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

            
            var host = new Host();
            var gazePointDataStream = host.Streams.CreateGazePointDataStream();

            Hook.GlobalEvents().OnCombination(new Dictionary<Combination, Action>
                {
                    {Combination.FromString("Shift+Alt+Enter"), () => { gazePointDataStream.GazePoint((x, y, ts) => Console.WriteLine("Timestamp: {0}\t X: {1} Y:{2}", ts, x, y)); }}
                });

            // launch the WinForms application like normal
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

        }
    }
}
