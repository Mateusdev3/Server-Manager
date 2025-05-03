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

namespace Server_Manager {

    public partial class MainWindow : Window {

        //Variaveis globais

        string diretoriomain = AppDomain.CurrentDomain.BaseDirectory;
        string dayzexename = "DayZServer_x64.exe";
        string dayzcfgname = "serverDZ.cfg";
        string mods = "";
        

        public MainWindow() {
            InitializeComponent();

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

        private void Iniciar(object sender, RoutedEventArgs e) {
            if (File.Exists(dayzexename)){
                if (Ch2.IsChecked == true){
                  
                    var pastas = Directory.GetDirectories(diretoriomain).Select(System.IO.Path.GetFileName).Where(nome => nome.StartsWith("@")).ToArray();
                    mods = string.Join(";", pastas);
                    MessageBox.Show(mods);

                }

                else { MessageBox.Show("Iniciando sem mods"); }

            }
            else
            {
                MessageBox.Show(".exe do servidor não encontrado...");
            }
            
        }

        private void Ch2_Checked(object sender, RoutedEventArgs e) {

        }
    }
}

