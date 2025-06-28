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
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.IO.Packaging;
using System.Windows.Threading;
using Path = System.IO.Path;
using System.Text.Json;
using System.Windows.Media.Animation;


namespace Server_Manager {

    public partial class MainWindow : Window {

        //Variáveis Globais

        string pathcomplete = string.Empty;
        string mods = string.Empty;
        string arguments = string.Empty;
        bool minimized = false;
        bool clickini = false;
        private DispatcherTimer loopTimer;
        string diretoriomain = string.Empty;
        string dayzexename = string.Empty;
        string dayzcfgname = string.Empty;
        string becname = string.Empty;
        string processbec = string.Empty;
        string becpath = string.Empty;
        string nomedayzprocess = string.Empty;
        string port = string.Empty;
        string whitelistpath = string.Empty;
        string configdayzpath = string.Empty;

        public MainWindow() {
            InitializeComponent();
            VariaveisJson();
            IniciarLoop();
        }

        public class Config {

            // Classe para mapear as variáveis do arquivo JSON de configuração

            public string Diretoriomain { get; set; } = string.Empty;
            public string Dayzexename { get; set; } = string.Empty;
            public string Dayzcfgname { get; set; } = string.Empty;
            public string Becname { get; set; } = string.Empty;
            public string Processbec { get; set; } = string.Empty;
            public string Becpath { get; set; } = string.Empty;
            public string Nomedayzprocess { get; set; } = string.Empty;
            public string Port { get; set; } = string.Empty;
            public string whitelistpath { get; set; } = string.Empty;
            public string configdayzpath { get; set; } = string.Empty;
        }

        public class PlayerInfo {
            public string NICK { get; set; }
            public string HWID { get; set; }
            public string RAMID { get; set; }
            public string COMPUTERID { get; set; }
            public string CPUID { get; set; }
            public string GPUID { get; set; }
            public string BOARDID { get; set; }

        }


        // Classe para o servidor HTTP

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            await Task.Run(() => ServerHttp.Httpserver(whitelistpath));
        }

        public class ServerHttp {
            public static async Task Httpserver(string wlfile) {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:5000/");
                listener.Start();
                Debug.WriteLine("Servidor HTTP iniciado. Aguardando requisições...");

                while (true)
                {
                    var context = await listener.GetContextAsync();

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            string path = context.Request.Url.AbsolutePath;
                            string method = context.Request.HttpMethod;
                            context.Response.ContentType = "application/json";
                            string resposta;

                            // Whitelist management routes

                            if (path.StartsWith("/add/") && method == "GET")
                            {
                                string id = path.Substring("/add/".Length);
                                File.AppendAllLines(wlfile, new[] { id });
                                resposta = JsonSerializer.Serialize(new { mensagem = $"Steam ID {id} adicionado com sucesso." });
                            }
                            else if (path.StartsWith("/remove/") && method == "GET")
                            {
                                string id = path.Substring("/remove/".Length);
                                var linhas = File.ReadAllLines(wlfile).Where(l => l != id).ToList();
                                File.WriteAllLines(wlfile, linhas);
                                resposta = JsonSerializer.Serialize(new { mensagem = $"Steam ID {id} removido com sucesso." });
                            }

                            // Conecxão do banco de dados

                            else if (path == "/status" && method == "POST")
                            {
                                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                                string json = await reader.ReadToEndAsync();

                                if (string.IsNullOrWhiteSpace(json))
                                    throw new Exception("JSON vazio");

                                var player = JsonSerializer.Deserialize<PlayerInfo>(json);
                                if (player == null || string.IsNullOrWhiteSpace(player.NICK))
                                    throw new Exception("Dados do jogador inválidos ou NICK vazio.");

                                string dbPath = @"C:\Projetos\Server Manager\Server Manager\players.db";
                                using var conn = new SQLiteConnection($"Data Source={dbPath}");
                                conn.Open();

                                string checkSql = @"
                            SELECT STATUS FROM Players 
                            WHERE
                              
                                CPUID = @cpuid OR
                                GPUID = @gpu OR 
                                COMPUTERID = @comp OR
                                BOARDID = @board
                            LIMIT 1;";

                                using var checkCmd = new SQLiteCommand(checkSql, conn);
                                checkCmd.Parameters.AddWithValue("@hwid", player.HWID);
                                checkCmd.Parameters.AddWithValue("@cpuid", player.CPUID);
                                checkCmd.Parameters.AddWithValue("@gpu", player.GPUID);
                                checkCmd.Parameters.AddWithValue("@ram", player.RAMID);
                                checkCmd.Parameters.AddWithValue("@comp", player.COMPUTERID);
                                checkCmd.Parameters.AddWithValue("@board", player.BOARDID);

                                object result = checkCmd.ExecuteScalar();
                                string status;

                                if (result == null)
                                {
                                    Debug.WriteLine("Novo jogador. Inserindo...");
                                    string insertSql = @"
                                INSERT INTO Players (NICK, HWID, RAMID, COMPUTERID, CPUID, GPUID, BOARDID, STATUS) 
                                VALUES (@nick, @hwid, @ramid, @compid, @cpuid, @gpuid, @boardid, 'ATIVO');";

                                    using var insertCmd = new SQLiteCommand(insertSql, conn);
                                    insertCmd.Parameters.AddWithValue("@nick", player.NICK);
                                    insertCmd.Parameters.AddWithValue("@hwid", player.HWID);
                                    insertCmd.Parameters.AddWithValue("@ramid", player.RAMID);
                                    insertCmd.Parameters.AddWithValue("@compid", player.COMPUTERID);
                                    insertCmd.Parameters.AddWithValue("@cpuid", player.CPUID);
                                    insertCmd.Parameters.AddWithValue("@gpuid", player.GPUID);
                                    insertCmd.Parameters.AddWithValue("@boardid", player.BOARDID);
                                    insertCmd.ExecuteNonQuery();

                                    status = "ATIVO";
                                }
                                else
                                {
                                    Debug.WriteLine("Jogador já existe. Status: " + result.ToString());
                                    status = result.ToString().ToUpper();
                                }

                                resposta = JsonSerializer.Serialize(new { status = status });
                            }
                            else
                            {
                                context.Response.StatusCode = 404;
                                resposta = JsonSerializer.Serialize(new { erro = "Rota não encontrada." });
                            }

                            byte[] buffer = Encoding.UTF8.GetBytes(resposta);
                            context.Response.ContentLength64 = buffer.Length;
                            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                            context.Response.OutputStream.Close();
                        }
                        catch (Exception ex)
                        {
                            string erro = JsonSerializer.Serialize(new { erro = ex.Message });
                            byte[] buffer = Encoding.UTF8.GetBytes(erro);
                            context.Response.StatusCode = 500;
                            context.Response.ContentLength64 = buffer.Length;
                            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                            context.Response.OutputStream.Close();
                        }
                    });
                }
            }
        }

        // Carrega as variáveis do arquivo JSON de configuração
        public void VariaveisJson() {
            string json = File.ReadAllText("ManagerConfig.json");
            var dados = JsonSerializer.Deserialize<Config>(json);
            if (dados == null)
            {
                MessageBox.Show("Erro ao carregar o arquivo de configuração. Verifique se o arquivo ManagerConfig.json existe e está formatado corretamente.");
                return;
            }

            diretoriomain = dados.Diretoriomain;
            dayzexename = dados.Dayzexename;
            dayzcfgname = dados.Dayzcfgname;
            becname = dados.Becname;
            processbec = dados.Processbec;
            becpath = dados.Becpath;
            nomedayzprocess = dados.Nomedayzprocess;
            port = dados.Port;
            whitelistpath = dados.whitelistpath;
            configdayzpath = dados.configdayzpath;
        }

        private void IniciarLoop() {
            loopTimer = new DispatcherTimer();
            loopTimer.Interval = TimeSpan.FromSeconds(2);
            loopTimer.Tick += LoopTimer_Tick;
            loopTimer.Start();
        }

        private async void LoopTimer_Tick(object sender, EventArgs e) {
            if (!clickini || Ch5.IsChecked != true) return;

            mods = Ch2.IsChecked == true ? ObterMods() : string.Empty;

            if (!CheckProcess(nomedayzprocess))
            {
                string argumdayz = $"-config={dayzcfgname} -port={port} -profiles=profile";
                if (!string.IsNullOrWhiteSpace(mods))
                    argumdayz += $" -mod={mods}";
                argumdayz += " -dologs -adminlog -netlog -freezecheck";

                StartProcess(dayzexename, argumdayz, true, diretoriomain);
            }

            if (Ch1.IsChecked == true && !CheckProcess(processbec))
            {
                string pathcomplete = Path.Combine(becpath, becname);
                string becargs = "-f Config.cfg --dsc";
                StartProcess(pathcomplete, becargs, true, becpath);
            }
        }

        private void Mouse(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void Fechar_Janela(object sender, RoutedEventArgs e) => this.Close();

        private void Minimizar_Janela(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;

        private void Iniciar(object sender, RoutedEventArgs e) {
            clickini = true;
            StartServer();
        }

        private void Fechar(object sender, RoutedEventArgs e) {
            clickini = false;
            if (CheckProcess(nomedayzprocess)) KillProces(nomedayzprocess);
            if (CheckProcess(processbec)) KillProces(processbec);
        }

        private void Windowbanmanager(object sender, RoutedEventArgs e) {
            Window1 window1 = new Window1();
            window1.Show();
        }

        private async void StartServer() {
            if (!clickini) return;

            Whitelist();

            // Obtém mods e deixa no formato exato para o DayZ
            mods = (Ch2.IsChecked == true) ? ObterMods() : string.Empty;
            string argumdayz = $"-config={dayzcfgname} -port={port} -profiles=profile";

            if (!string.IsNullOrWhiteSpace(mods))
            {
                argumdayz += $" \"-mod={mods}\"";
            }

            argumdayz += " -dologs -adminlog -netlog -freezecheck";
            MessageBox.Show($"Args usados:\n{argumdayz}");

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
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    WindowStyle = minimized ? ProcessWindowStyle.Minimized : ProcessWindowStyle.Normal
                };

                try
                {
                    Process process = Process.Start(info);
                    if (process == null) MessageBox.Show("Erro ao iniciar o processo.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao iniciar o processo:\n" + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Executável não encontrado:\n" + exename);
            }
        }

        // Função que busca todas as pastas de mods no diretório principal e retorna no formato usado pelo parâmetro -mod do DayZ
        private string ObterMods() {
            if (!Directory.Exists(diretoriomain))
            {
                MessageBox.Show($"Diretório principal '{diretoriomain}' não encontrado.");
                return string.Empty;
            }

            var modsList = Directory.GetDirectories(diretoriomain)
                .Select(Path.GetFileName)
                .Where(nome => nome != null && nome.StartsWith("@"))
                .ToList();

            if (modsList.Count == 0) return string.Empty;

            
            return string.Join(";", modsList) + ";";
        }

        private bool CheckProcess(string name) {
            foreach (var process in Process.GetProcessesByName(name))
            {
                if (!process.HasExited)
                    return true;
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

        public void Whitelist() {
            if (Ch4.IsChecked == true)
            {
                if (File.Exists(configdayzpath))
                {
                    string[] linhas = File.ReadAllLines(configdayzpath);
                    for (int i = 0; i < linhas.Length; i++)
                    {
                        if (linhas[i].Trim().StartsWith("enableWhitelist"))
                        {
                            linhas[i] = "enableWhitelist = 1;        // Enable/disable whitelist (value 0-1)";
                            break;
                        }
                    }
                    File.WriteAllLines(configdayzpath, linhas);
                }
                else
                {
                    MessageBox.Show("Arquivo cfg do dayz não encontrado.");
                    return;
                }
            }
            else
            {
                if (File.Exists(configdayzpath))
                {
                    string[] linhas = File.ReadAllLines(configdayzpath);
                    for (int i = 0; i < linhas.Length; i++)
                    {
                        if (linhas[i].Trim().StartsWith("enableWhitelist"))
                        {
                            linhas[i] = "enableWhitelist = 0;        // Enable/disable whitelist (value 0-1)";
                            break;
                        }
                    }
                    File.WriteAllLines(configdayzpath, linhas);
                }
                else
                {
                    MessageBox.Show("Arquivo cfg do dayz não encontrado.");
                    return;
                }
            }
        }
    }
}
