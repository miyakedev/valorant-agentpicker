using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using AutoIt;
using System.Reflection;
using System.Drawing;

namespace ValorantBot {
    class Program {
        class Config {
            public Agents[] agents { get; set; }
            public class Agents {
                public string name { set; get; }
                public bool recruited { set; get; }
                public bool disabled { set; get; }
            }
            public Settings settings { get; set; }
            public class Settings {
                public int agentIcon_mouseClick_count { set; get; }
                public int agentIcon_mouseClick_speed { set; get; }
                public int lockIn_mouseClick_count { set; get; }
                public int lockIn_mouseClick_speed { set; get; }
            }
            public Resolutions[] resolutions { get; set; }
            public class Resolutions {
                public string name { set; get; }
                public int screenWidth { set; get; }
                public int screenHeight { set; get; }
                public int agentIconWidth { set; get; }
                public int agentIconHeight { set; get; }
                public int spacesBetweenIcons { set; get; }
                public int topSpace { set; get; }
                public int bottomSpace { set; get; }
                public int lockInWidth { set; get; }
                public int lockInHeight { set; get; }
            }
        }

        static int agentId = 0;
        static int resolutionId = 0;
        static Point agentIconCoord = new Point();
        static Point lockInCoord = new Point();
        static Dictionary<string, int> a_agents = new Dictionary<string, int>(); // Reclutados
        static Dictionary<string, int> b_agents = new Dictionary<string, int>(); // No Reclutados
        static Dictionary<string, int> resolutions = new Dictionary<string, int>(); // Resoluciones
        static Config config = null;
        static bool hasRegistered = false;

        static void Main(string[] args) {
            Console.Title = "Valorant Bot Agent-Picker v1.2";
            WriteTitle();
            LoadConfig();
            SelectResolution();
        }

        static void WriteTitle() {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Valorant Bot Agent-Picker By Shirotzu Miyake");
            Console.WriteLine();
        }

        static void LoadConfig() {
            string pathConfig = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
            config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(pathConfig));
            int id = 0;
            foreach (var agent in config.agents.OrderBy(i => i.name).ToList()) {
                if (!agent.disabled) {
                    if (agent.recruited) {
                        id++;
                        a_agents.Add(agent.name, id);
                    } else {
                        b_agents.Add(agent.name, 255);
                    }
                }
            }
            int id2 = 0;
            foreach (var res in config.resolutions) {
                id2++;
                resolutions.Add(res.name, id2);
            }
        }

        static void UpdateCoords() {
            agentIconCoord.X = ((config.resolutions[resolutionId].screenWidth - (a_agents.Count + b_agents.Count) * (config.resolutions[resolutionId].agentIconWidth + config.resolutions[resolutionId].spacesBetweenIcons) - config.resolutions[resolutionId].spacesBetweenIcons) / 2) + a_agents.ElementAt(agentId).Value * (config.resolutions[resolutionId].agentIconWidth + config.resolutions[resolutionId].spacesBetweenIcons) - ((config.resolutions[resolutionId].agentIconWidth + config.resolutions[resolutionId].spacesBetweenIcons) / 2);
            agentIconCoord.Y = (config.resolutions[resolutionId].screenHeight - config.resolutions[resolutionId].bottomSpace) - (config.resolutions[resolutionId].agentIconHeight / 2);
            lockInCoord.X = ((config.resolutions[resolutionId].screenWidth - config.resolutions[resolutionId].lockInWidth) / 2) + (config.resolutions[resolutionId].lockInWidth / 2);
            lockInCoord.Y = ((((config.resolutions[resolutionId].screenHeight - config.resolutions[resolutionId].bottomSpace) - config.resolutions[resolutionId].agentIconHeight) - config.resolutions[resolutionId].topSpace) - (config.resolutions[resolutionId].lockInHeight / 2));
        }

        static void SelectResolution(bool hasError = false, bool isUpdate = false) {
            if (hasError) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("* Invalid Resolution.");
                Console.WriteLine();
            }
            Console.ForegroundColor = ConsoleColor.Blue;
            foreach (KeyValuePair<string, int> k in resolutions) {
                Console.WriteLine(string.Format("[{0}] {1}", k.Value, k.Key));
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.Write("Select your Resolution: ");
            var val = Console.ReadLine();
            if (int.TryParse(val, out _)) {
                int num = Int32.Parse(val);
                if (num >= 1 && num <= resolutions.Count()) {
                    resolutionId = num - 1;
                    if (isUpdate) {
                        Console.Clear();
                        WriteTitle();
                        WriteOptions();
                    } else {
                        Console.Clear();
                        WriteTitle();
                        SelectAgent();
                    }
                } else {
                    if (isUpdate) {
                        Console.Clear();
                        WriteTitle();
                        SelectResolution(true, true);
                    } else { 
                        Console.Clear();
                        WriteTitle();
                        SelectResolution(true, false);
                    }
                }
            } else {
                Console.Clear();
                WriteTitle();
                SelectResolution(true);
            }
        }

        static void SelectAgent(bool hasError = false) {
            if (hasError) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("* Invalid Agent.");
                Console.WriteLine();
            }
            Console.ForegroundColor = ConsoleColor.Blue;
            foreach (KeyValuePair<string, int> k in a_agents) {
                Console.WriteLine(string.Format("[{0}] {1}", k.Value, k.Key));
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.Write("Select your Agent: ");
            var val = Console.ReadLine();
            if (int.TryParse(val, out _)) {
                int num = Int32.Parse(val);
                if (num >= 1 && num <= a_agents.Count()) {
                    agentId = num - 1;
                    Console.Clear();
                    WriteTitle();
                    WriteOptions();
                } else {
                    Console.Clear();
                    WriteTitle();
                    SelectAgent(true);
                }
            } else {
                Console.Clear();
                WriteTitle();
                SelectAgent(true);
            }
        }

        static void WriteOptions(bool hasError = false) {
            if (hasError) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("* Invalid Option.");
                Console.WriteLine();
            }
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Resolution: {0}", config.resolutions[resolutionId].name);
            Console.WriteLine("Agent to Lock In: {0}", a_agents.ElementAt(agentId).Key);
            Console.WriteLine("Auto-Pick HotKey: F5");
            UpdateCoords();
            if (!hasRegistered) {
                hasRegistered = true;
                HotKeyManager.RegisterHotKey(Keys.F5, KeyModifiers.NoRepeat);
                HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(HotKeyManager_HotKeyPressed);
            }
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine();
            Console.WriteLine("[1] Change Agent\n[2] Change Resolution");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.Write("Select an Option: ");
            var value = Console.ReadLine();
            switch (value) {
                case "1":
                    Console.Clear();
                    WriteTitle();
                    SelectAgent();
                    break;
                case "2":
                    Console.Clear();
                    WriteTitle();
                    SelectResolution(false, true);
                    break;
                default:
                    Console.Clear();
                    WriteTitle();
                    WriteOptions(true);
                    break;
            }
        }

        static void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e) {
            if (AutoItX.WinWaitActive("VALORANT") == 1) {
                AutoItX.MouseClick("LEFT", agentIconCoord.X, agentIconCoord.Y, config.settings.agentIcon_mouseClick_count, config.settings.agentIcon_mouseClick_speed);
                AutoItX.MouseClick("LEFT", lockInCoord.X, lockInCoord.Y, config.settings.lockIn_mouseClick_count, config.settings.lockIn_mouseClick_speed);
            }
        }
    }

    public static class HotKeyManager {
        public static event EventHandler<HotKeyEventArgs> HotKeyPressed;

        public static int RegisterHotKey(Keys key, KeyModifiers modifiers) {
            _windowReadyEvent.WaitOne();
            int id = System.Threading.Interlocked.Increment(ref _id);
            _wnd.Invoke(new RegisterHotKeyDelegate(RegisterHotKeyInternal), _hwnd, id, (uint)modifiers, (uint)key);
            return id;
        }

        public static void UnregisterHotKey(int id) {
            _wnd.Invoke(new UnRegisterHotKeyDelegate(UnRegisterHotKeyInternal), _hwnd, id);
        }

        delegate void RegisterHotKeyDelegate(IntPtr hwnd, int id, uint modifiers, uint key);
        delegate void UnRegisterHotKeyDelegate(IntPtr hwnd, int id);

        private static void RegisterHotKeyInternal(IntPtr hwnd, int id, uint modifiers, uint key) {
            RegisterHotKey(hwnd, id, modifiers, key);
        }

        private static void UnRegisterHotKeyInternal(IntPtr hwnd, int id) {
            UnregisterHotKey(_hwnd, id);
        }

        private static void OnHotKeyPressed(HotKeyEventArgs e) {
            if (HotKeyManager.HotKeyPressed != null) {
                HotKeyManager.HotKeyPressed(null, e);
            }
        }

        private static volatile MessageWindow _wnd;
        private static volatile IntPtr _hwnd;
        private static ManualResetEvent _windowReadyEvent = new ManualResetEvent(false);
        static HotKeyManager() {
            Thread messageLoop = new Thread(delegate () {
                Application.Run(new MessageWindow());
            });
            messageLoop.Name = "MessageLoopThread";
            messageLoop.IsBackground = true;
            messageLoop.Start();
        }

        private class MessageWindow : Form {
            public MessageWindow() {
                _wnd = this;
                _hwnd = this.Handle;
                _windowReadyEvent.Set();
            }

            protected override void WndProc(ref Message m) {
                if (m.Msg == WM_HOTKEY) {
                    HotKeyEventArgs e = new HotKeyEventArgs(m.LParam);
                    HotKeyManager.OnHotKeyPressed(e);
                }

                base.WndProc(ref m);
            }

            protected override void SetVisibleCore(bool value) {
                base.SetVisibleCore(false);
            }

            private const int WM_HOTKEY = 0x312;
        }

        [DllImport("user32", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private static int _id = 0;
    }


    public class HotKeyEventArgs : EventArgs {
        public readonly Keys Key;
        public readonly KeyModifiers Modifiers;

        public HotKeyEventArgs(Keys key, KeyModifiers modifiers) {
            this.Key = key;
            this.Modifiers = modifiers;
        }

        public HotKeyEventArgs(IntPtr hotKeyParam) {
            uint param = (uint)hotKeyParam.ToInt64();
            Key = (Keys)((param & 0xffff0000) >> 16);
            Modifiers = (KeyModifiers)(param & 0x0000ffff);
        }
    }

    [Flags]
    public enum KeyModifiers {
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8,
        NoRepeat = 0x4000
    }
}