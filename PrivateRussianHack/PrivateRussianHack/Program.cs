using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using  Half_Life2_GH;



/*
TODO:
- GLOBAL KEY HOOK -> DONE
- GETPOS -> DONE
- STRING THING -> DONE
- HL2 BIND -> DONE
- PREVENT CHEAT ->
- CHECK FOR OOB ->
    */
namespace PrivateRussianHack
{   
    class Program
    {
        #region DLLIMPORTS
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        const int SAVE_HOTKEY = 1;
        const int LOAD_HOTKEY = 1;
        private const int WhKeyboardLl = 13;
        private const int WmKeydown = 0x0100;
        private static readonly LowLevelKeyboardProc Proc = HookCallback;
        private static IntPtr _hookId = IntPtr.Zero;

        [DllImport("User32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("User32.dll")]
        public static extern void ReleaseDC(IntPtr hwnd, IntPtr dc);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        #endregion

        private static bool _saved = false;
        static Process _hl2Process = null;
        private const int Units = 12; //UNITS to move the player up
        private static string SaveLoadFile = " "; // DONT use "\" use "/"-s
        private static bool _slStarted = false;
        private static float _x = 0f;
        private static float _y = 0f;
        private static float _z = 0f;
        private static ProcessModule _engineModule = null;

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                SaveLoadFile = File.ReadAllLines("config.txt")[0];
            }
            catch (Exception)
            {
                Console.WriteLine(@"Config file not found!
1. Please create a file named config.txt.
2. put your hl2 cfg directory into it and \pos.cfg.
Eg.: D:\Speedrun\OE\hl2\cfg\pos.cfg");
                Console.ReadKey();
                Environment.Exit(0x0001);
            }
            try
            {
                _hl2Process = Process.GetProcesses().First(x => x.ProcessName.Contains("hl2"));
            }
            catch (Exception)
            {
                if(_hl2Process == null)
                {
                    Console.WriteLine("Please launch hl2 Old Engine and restart this app.");
                    Console.ReadKey();
                    Environment.Exit(0x0001);
                }
            }
            GetEngineDll(_hl2Process);
            /*            
] bind h
"h" = "" // SAVE
] bind g
"g" = "unpause;w10;exec pos.cfg;pause;" // LOAD
] bind h
"h" = "" //RESTART SL
            */
            Console.Title = "HL2 Found - H:Save J:Load";
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _hookId = SetHook(Proc);
            Application.Run();

            UnhookWindowsHookEx(_hookId);
            Console.Title = "HL2 OE SaveLoader thing idk";
        }

        static void Save()
        {
            if (!_saved)
            {
                _saved = true;
            }
            if (!_slStarted)
            {
                Console.Clear();
                Console.WriteLine("Saveload started!");
                _slStarted = true;
                _x = Memory.ReadFloat(_hl2Process,
                    new IntPtr(_engineModule.BaseAddress.ToInt32() + 0x5B6CAC));
                _z = Memory.ReadFloat(_hl2Process,
                    new IntPtr(_engineModule.BaseAddress.ToInt32() + 0x5B6CB4));
                _y = Memory.ReadFloat(_hl2Process,
                    new IntPtr(_engineModule.BaseAddress.ToInt32() + 0x5B6CB0));
                _z += Units;
            }
            else
            {
                _z += Units;
            }
            Console.WriteLine("Saved..");
            Console.WriteLine($@"
X: {_z}
Y: {_y}
Z: {_z}
");
            string pos =  ("setpos " + _x + " " + _y + " " + _z + ";");
            if (File.Exists(SaveLoadFile))
            {
                File.WriteAllLines(SaveLoadFile, new List<string>() { pos });
            }
            else
            {
                File.Create(SaveLoadFile);
                File.WriteAllLines(SaveLoadFile, new List<string>() { pos });
            }

            #region old
            /* string currentPos = ReadPos();
             Console.Title = currentPos;
             string angle = currentPos.Split(';').Last();
             string posNoang = currentPos.Split(';')[0];
             posNoang = posNoang.Substring(6, posNoang.Length - 6);
             float x;

             float y;
             float z;
             posNoang = Regex.Replace(posNoang, " - ", " -");
             var fields = posNoang.Split(' ');
             x = float.Parse(fields[0]);
             y = float.Parse(fields[1]);
             z = float.Parse(fields[2]);
             y += UNITS;
             File.WriteAllLines();*/
#endregion
        }
        static void Load()
        {
            if (!_saved)
                return;
            Console.WriteLine("Loaded...");          
            _saved = false;
        }
        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WhKeyboardLl, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WmKeydown)
            {
                var key = (Keys)Marshal.ReadInt32(lParam);
                if (key == Keys.H)
                {
                    Save();
                }
                if (key == Keys.F)
                {
                    Console.Clear();
                    Console.WriteLine("Saveload started!");
                    _slStarted = false;
                }
            }
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        static void GetEngineDll(Process hlProcess)
        {
            for (var i = 0; i < hlProcess.Modules.Count; i++)
            {
                if (hlProcess.Modules[i].ModuleName == "engine.dll")
                    _engineModule = hlProcess.Modules[i];
            }
        }
    }
}
