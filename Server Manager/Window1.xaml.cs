using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;


namespace Server_Manager {
   
    public partial class Window1 : Window {
        private bool isOpen = true;
        private const string dbPath = @"Data Source=C:\Projetos\Server Manager\Server Manager\players.db";
        private string tipoSelecionado = "ID";

        public Window1() {

            InitializeComponent();
           
        }

        private void Mouse(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Fechar_Janela(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Minimizar_Janela(object sender, RoutedEventArgs e) {
            this.WindowState = WindowState.Minimized;
        }

        private void comboBusca_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) {

        }

        private void SearchType_Click(object sender, RoutedEventArgs e) {
            if (sender is MenuItem item && item.Header != null)
            {
                tipoSelecionado = item.Header.ToString();
                SearchTypeButton.Content = tipoSelecionado;
            }
        }

        private void SearchTypeButton_Click(object sender, RoutedEventArgs e) {
            if (SearchTypeButton.ContextMenu != null)
            {
                SearchTypeButton.ContextMenu.PlacementTarget = SearchTypeButton;
                SearchTypeButton.ContextMenu.IsOpen = true;
            }
        }

        private void Pesquisar_Click(object sender, RoutedEventArgs e) {
            try
            {
                string valor = txtSearch.Text.Trim();

                if (string.IsNullOrWhiteSpace(valor))
                {
                    MessageBox.Show("Digite um valor para pesquisar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                using (var connection = new SQLiteConnection(dbPath))
                {
                    connection.Open();

                    string query = $"SELECT * FROM Players WHERE {tipoSelecionado} LIKE @valor";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@valor", $"%{valor}%");

                        var adapter = new SQLiteDataAdapter(command);
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Evita o erro de "coleção de itens deve estar vazia"
                        DataGridResultados.ItemsSource = null;
                        DataGridResultados.ItemsSource = dataTable.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao pesquisar: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DataGridResultados_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }
    }
}

