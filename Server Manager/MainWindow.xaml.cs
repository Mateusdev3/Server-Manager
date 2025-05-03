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

        string dayzexename = "DayZServer_x64.exe";
        

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


                
            
            }
            else
            {
                MessageBox.Show(".exe do servidor não encontrado...");
            }
            





        }
    }
}

