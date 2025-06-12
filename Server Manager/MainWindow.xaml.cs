using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.IO.Packaging;
using System.Windows.Threading;
using Path = System.IO.Path;
using System.Text.Json;

namespace Server_Manager {

    public partial class MainWindow : Window {

        //Variáveis Globais

        string pathcomplete = string.Empty;
        string mods = string.Empty;
        string arguments = string.Empty;
        bool minimized = false;
        bool clickini = false;
        private DispatcherTimer loopTimer;
        string diretoriomain;
        string dayzexename;
        string dayzcfgname;
        string becname;
        string processbec;
        string becpath;
        string nomedayzprocess;
        string port;
        string whitelistpath;


        public MainWindow() {

            InitializeComponent();
            VariaveisJson();
            InciarLoop();
        }
        public class Config {

            public string Diretoriomain { get; set; }
            public string Dayzexename { get; set; }
            public string Dayzcfgname { get; set; }
            public string Becname { get; set; }
            public string Processbec { get; set; }
            public string Becpath { get; set; }
            public string Nomedayzprocess { get; set; }
            public string Port { get; set; }
            


        }

        public void VariaveisJson() {
            string json = File.ReadAllText("ManagerConfig.json");
            var dados = JsonSerializer.Deserialize<Config>(json);

            diretoriomain = dados.Diretoriomain;
            dayzexename = dados.Dayzexename;
            dayzcfgname = dados.Dayzcfgname;
            becname = dados.Becname;
            processbec = dados.Processbec;
            becpath = dados.Becpath;
            nomedayzprocess = dados.Nomedayzprocess;
            port = dados.Port;

        }

        private void InciarLoop() {
            loopTimer = new DispatcherTimer();
            loopTimer.Interval = TimeSpan.FromSeconds(2);
            loopTimer.Tick += LoopTimer_Tick;
            loopTimer.Start();
        }

        //Auto Start
        private async void LoopTimer_Tick(object sender, EventArgs e) {
            while (Ch5.IsChecked == true)
            {
                if (!clickini) return;
                if (!CheckProcess(nomedayzprocess))
                {
                    string argumdayz = $"-config={dayzcfgname} -port={port} -profiles=profile -mod={mods} -dologs -adminlog -netlog -freezecheck";

                    StartProcess(dayzexename, argumdayz, true, diretoriomain);
                    break;
                }

                if (Ch1.IsChecked == true)
                {
                    if (!CheckProcess(processbec))
                    {
                        string pathcomplete = Path.Combine(becpath, becname);
                        string becargs = "-f Config.cfg --dsc";
                        StartProcess(pathcomplete, becargs, true, becpath);
                        break;
                    }
                    else
                    {
                        break;
                    }

                }
            }
        }

        // Gerenciamento da janela

        private void Mouse(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Fechar_Janela(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Minimizar_Janela(object sender, RoutedEventArgs e) {
            this.WindowState = WindowState.Minimized;
        }

        private void Iniciar(object sender, RoutedEventArgs e) {
            clickini = true;
            StartServer();
        }

        private void Fechar(object sender, RoutedEventArgs e) {
            clickini = false;
            if (CheckProcess(nomedayzprocess))
            {
                KillProces(nomedayzprocess);
            }
            if (CheckProcess(processbec))
            {
                KillProces(processbec);
            }
        }

        //Inicializador do servidor

        private async void StartServer() {
            if (!clickini) return;
            string argumdayz = $"-config={dayzcfgname} -port={port} -profiles=profile -mod={mods} -dologs -adminlog -netlog -freezecheck";

            if (Ch2.IsChecked == true)
            {
               mods = ObterMods();
               argumdayz = $"-config={dayzcfgname} -port={port} -profiles=profile -mod={mods} -dologs -adminlog -netlog -freezecheck";
               MessageBox.Show(argumdayz);
            }
            StartProcess(dayzexename, argumdayz, minimized, diretoriomain);

            if (Ch1.IsChecked == true)
            {
                string pathcomplete = Path.Combine(becpath, becname);
                string becargs = "-f Config.cfg --dsc";
                StartProcess(pathcomplete, becargs, minimized, becpath);
            }
        }
        private void StartProcess(string exename, string args, bool minimized, string dir) {

            if (File.Exists(exename))
            {

                ProcessStartInfo info = new ProcessStartInfo
                {
                    FileName = exename,
                    Arguments = args,
                    WorkingDirectory = dir,
                    UseShellExecute = true,
                    CreateNoWindow = minimized,
                    WindowStyle = minimized ? ProcessWindowStyle.Minimized : ProcessWindowStyle.Normal
                };
                Process process = Process.Start(info);
                if (process == null)
                {
                    MessageBox.Show("Erro ao iniciar");
                }
            }
            else
            {
                MessageBox.Show(becpath);
                MessageBox.Show("executavel não encontrado...");
            }
        }

        private string ObterMods() {

            string[] pastas = Directory.GetDirectories(diretoriomain).Select(System.IO.Path.GetFileName).Where(nome => nome.StartsWith("@")).ToArray();
            mods = string.Join(";", pastas) + ";";
            return mods;
        }

        //Checkker de processos

        private bool CheckProcess(string name) {
            foreach (var process in Process.GetProcessesByName(name))
            {
                if (!process.HasExited)
                {
                    return true;
                }
            }
            return false;
        }

        private void KillProces(string name) {
            foreach (Process process in Process.GetProcessesByName(name))
            {
                process.Kill();
                process.WaitForExit();
            }
        }
    }
}
