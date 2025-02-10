using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF_CV_RRHH
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private const string selectQuery = "SELECT * FROM artista";
        private const string ordenador = "DESKTOP - NNKTF0L\\SQLEXPRESS";
        private const string nomTabla = "BBDD2EVAL";


        public MainWindow()
        {
            InitializeComponent();
            Loaded += Window_Loaded; // Carga de datos después de inicializar la UI
        }

        

        //string connectionString = String.Concat("Data Source =", ordenador,
        //";Initial Catalog=", nomTabla, ";Integrated Security = True");
        //string connectionString = String.Concat("Server=", ordenador,
        //          ";Database=", nomTabla, ";Trusted_Connection = True");

        string connectionString = "Server=DESKTOP-NNKTF0L\\SQLEXPRESS; Database=BBDD2EVAL; " +
            "Integrated Security=True; TrustServerCertificate=True";
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await CargarDatosAsync(); // Carga asíncrona
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }



        }
        private async Task CargarDatosAsync()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(); // 👈 Método asíncrono

                using (var command = new SqlCommand(selectQuery, connection))
                {
                    var dataTable = new DataTable();
                    var adapter = new SqlDataAdapter(command);
                    adapter.Fill(dataTable);

                    // Asignar datos al DataGrid en el hilo de la UI
                    Dispatcher.Invoke(() =>
                    {
                        dataGrid.ItemsSource = dataTable.DefaultView;
                    });
                }
            }
        }

    
    }
}