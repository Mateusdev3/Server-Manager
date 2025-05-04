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
using System.Threading;
using System.Timers;
using System.IO.Packaging;
using System.Windows.Threading;

namespace Server_Manager {

    public partial class MainWindow : Window {

        //Variaveis globais

        string diretoriomain = AppDomain.CurrentDomain.BaseDirectory;
        string dayzexename = "DayZServer_x64.exe";
        string dayzcfgname = "serverDZ.cfg";
        string port = "2302";
        string mods = string.Empty;
        string arguments = string.Empty;
        string nomedayzprocess = "DayZServer_x64";
        bool minimized = false;
        bool clickini = false;
        private DispatcherTimer loopTimer;

        public MainWindow() {

            InitializeComponent();
            InciarLoop();
        }

        //Loops

        private void InciarLoop() {

            loopTimer = new DispatcherTimer();
            loopTimer.Interval = TimeSpan.FromSeconds(5);
            loopTimer.Tick += LoopTimer_Tick;
            loopTimer.Start();
        }

        private async void LoopTimer_Tick(object sender, EventArgs e) {
            while (Ch5.IsChecked == true)
            {
                if (!CheckProcess(nomedayzprocess))
                {
                    StartProces();
                    break;
                }
                else
                {
                    break;
                }
            }
        }

        //Gerenciamento da janela principal

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

        //Botões principais

        private async void Iniciar(object sender, RoutedEventArgs e) {

            StartProces();
            clickini = true;
        }
        private void Fechar(object sender, RoutedEventArgs e) {
            clickini = false;
            if (CheckProcess(nomedayzprocess))
            {
                KillProces(nomedayzprocess);
            }
        }

        //Gerenciamento de tarefas

        private async void StartProces() {
            if (clickini == true)
            {
                if (File.Exists(dayzexename))
                {

                    if (Ch2.IsChecked == true)
                    {
                        var pastas = Directory.GetDirectories(diretoriomain).Select(System.IO.Path.GetFileName).Where(nome => nome.StartsWith("@")).ToArray();
                        mods = string.Join(";", pastas);
                    }

                    string arguments = $"-config={dayzcfgname} -port={port} -mod={mods} -profiles=config -dologs -adminlog -netlog -freezecheck";

                    ProcessStartInfo info = new ProcessStartInfo
                    {
                        FileName = dayzexename,
                        Arguments = arguments,
                        UseShellExecute = true,
                        CreateNoWindow = minimized,
                        WindowStyle = minimized ? ProcessWindowStyle.Minimized : ProcessWindowStyle.Normal

                    };

                    Process process = Process.Start(info);
                    await Task.Delay(5000);
                    if (process == null)
                    {
                        MessageBox.Show("Erro ao iniciar");
                    }
                }
                else
                {
                    MessageBox.Show(".exe do servidor não encontrado...");
                }
            }
        }
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

