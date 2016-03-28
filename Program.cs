using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using Half_Life2_GH;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using WindowsInput;

namespace MakeAmericaGreatAgain_Trump2016
{
    class Program
    {
        const UInt32 WM_KEYDOWN = 0x0100;
        const int VK_F5 = 0x4F;

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        static void Main(string[] args)
        {
        Start:
           var processes = Process.GetProcesses();
            ProcessModule engineModule = null;
            Process hl2Process = processes.First(x => x.ProcessName.Contains("hl2"));
            for (var i = 0; i < hl2Process.Modules.Count; i++)
            {
                if (hl2Process.Modules[i].ModuleName == "engine.dll") engineModule = hl2Process.Modules[i];
            }
            while (true)
            {
                var tick = Memory.ReadInt32(hl2Process, new IntPtr(0x0017E37C));
                var endtick = Memory.ReadInt32(hl2Process, new IntPtr(engineModule.BaseAddress.ToInt32() + 0x36D93C)); 
                    if ((tick == endtick-1)&&(endtick != 0))
                    {
                        Console.Title = ("STOPPED DEMO");
                        InputSimulator.SimulateKeyPress(VirtualKeyCode.F2);
                        Thread.Sleep(10);
                        break;
                    }
                Console.Clear();
                Console.WriteLine("Time: " + tick + "/" + endtick);;
            }
            Console.WriteLine("Press any key to restart...");
            Console.ReadLine();
            Console.Title = "Waiting for demo to play!";
            goto Start;
        }

    }
}
