using Microsoft.Data.SqlClient;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
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
    public partial class MainWindow : Window, INotifyPropertyChanged
    {


        private string _baseDeDatos, _bkBaseDeDatos;
        private static string baseDeDatos = "CV-RRHH";
        private string _tabla, _bkTabla;
        private string _ordenador, _bkOrdenador;
        private string _selectQuery;
        private string connectionString;

        public string BaseDeDatos
        {
            get => _baseDeDatos;
            set
            {
                _baseDeDatos = value;
                OnPropertyChanged(nameof(BaseDeDatos));
            }
        }
        public string Tabla
        {
            get => _tabla;
            set
            {
                _tabla = value;
                OnPropertyChanged(nameof(Tabla));
            }
        }
        public string Otro
        {
            get => _ordenador;
            set
            {
                _ordenador = value;
                OnPropertyChanged(nameof(Otro));
            }
        }



        public MainWindow()
        {
            //Inicialización de los valores por Defecto para las consultas:
            _tabla = "EMPLEADOS";
            _selectQuery = "SELECT * FROM " + _tabla;
            _baseDeDatos = baseDeDatos = "CV-RRHH";
            _ordenador = "DESKTOP-MDAC0QE\\SQLEXPRESS";


            //init:
            InitializeComponent();
            Loaded += Window_Loaded; // Carga de datos después de inicializar la UI
            //binding:
            DataContext = this;
        }




        //CONTROLADOR DE BINDING
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Cargar();
        }


        private async void Cargar()
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

        //AL PULSAR ENTER
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {


                // Lógica con el dato (ej: enviar a base de datos)

                if(!string.Equals(BaseDeDatos, _bkBaseDeDatos) || 
                    !string.Equals(Tabla, _bkTabla) || 
                    !string.Equals(Otro, _bkOrdenador))
                {
                    _bkBaseDeDatos = BaseDeDatos;
                    _bkTabla = Tabla;
                    _bkOrdenador = Otro;

                    MessageBox.Show($"Texto guardado: {BaseDeDatos}");
                    Cargar();


                }




            }
        }



        //RECOGIDA DE DATOS
        private async Task CargarDatosAsync()
        {
            //Actualización de strings para cuando se cambie la tabla:
            _selectQuery = "SELECT * FROM " + _tabla;
            connectionString = String.Concat("Server=",_ordenador,"; Database=",
            _baseDeDatos, "; Integrated Security=True; TrustServerCertificate=True");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(); // 👈 Método asíncrono

                using (var command = new SqlCommand(_selectQuery, connection))
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