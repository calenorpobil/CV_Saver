using Microsoft.Data.SqlClient;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http.Headers;
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
using WPF_CV_RRHH.Modelos;

namespace WPF_CV_RRHH
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {


        private string _nombreEntrevistado, _bkNombreEntrevistado;
        private static string nombreEntrevistado = "CV-RRHH";
        private string _dni, _bkDni;
        private string _otro, _bkOtro;
        private string _direccion, _años, _experiencia;
        private string connectionString;
        private int _codSeleccionado;
        private Informe InformeMostrar;
        public ObservableCollection<Empleado> Empleados { get; set; }

        public string NombreEntrevistado

        {
            get => _nombreEntrevistado;
            set
            {
                _nombreEntrevistado = value;
                OnPropertyChanged(nameof(NombreEntrevistado));
            }
        }
        public string Dni
        {
            get => _dni;
            set
            {
                _dni = value;
                OnPropertyChanged(nameof(Dni));
            }
        }
        public string Otro
        {
            get => _otro;
            set
            {
                _otro = value;
                OnPropertyChanged(nameof(Otro));
            }
        }

        public string Direccion
        {
            get => _direccion;
            set
            {
                _direccion = value;
                OnPropertyChanged(nameof(Direccion));
            }
        }

        public string Años
        {
            get => _años;
            set
            {
                _años = value;
                OnPropertyChanged(nameof(Años));
            }
        }

        public string Experiencia
        {
            get => _experiencia;
            set
            {
                _experiencia = value;
                OnPropertyChanged(nameof(Experiencia));
            }
        }



        public MainWindow()
        {
            //Inicialización de los valores por Defecto para las consultas:
            _dni = "";
            _nombreEntrevistado = nombreEntrevistado = "";
            //_otro = "DESKTOP-NNKTF0L\\SQLEXPRESS";
            _otro = "DESKTOP-MDAC0QE\\SQLEXPRESS";


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
            string consultaEmpleado = consultaDataRow();

            Cargar(consultaEmpleado);
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string consultaEmpleado = consultaDataRow();
            Cargar(consultaEmpleado);
        }

        // HACER CLICK EN DATAGRID
        private void dataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            dataGrid.CommitEdit();

            var selectedCell = dataGrid.CurrentCell;
            if (!selectedCell.IsValid) return;
            int len = dataGrid.Columns.Count;
            // Obtener el valor de la columna "CODIGO"
            var column = dataGrid.Columns[0]; 
            //dataGrid.Columns["NombreColumna"]
            var cellContent = column.GetCellContent(selectedCell.Item);

            if (cellContent is TextBlock textBlock)
            {
                string valor = textBlock.Text;
                _codSeleccionado = int.Parse(valor);
            }
            CargarInforme();
        }


        //CARGAR EL DATAGRID
        private async void Cargar(string consulta)
        {

            connectionString = String.Concat("Server=", Otro, "; Database=CV-RRHH",
            "; Integrated Security=True; TrustServerCertificate=True");

            try
            {
                await CargarDatosAsyncDataGrid(consulta); // Carga asíncrona
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //CARGAR EL INFORME
        private async void CargarInforme()
        {

            connectionString = String.Concat("Server=", Otro, "; Database=CV-RRHH",
            "; Integrated Security=True; TrustServerCertificate=True");

            try
            {
                await CargarDatosAsyncInformes(); // Carga asíncrona
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //CARGAR EL DOCUMENTOS
        private async void CargarDocumentos()
        {

            connectionString = String.Concat("Server=", Otro, "; Database=CV-RRHH",
            "; Integrated Security=True; TrustServerCertificate=True");

            try
            {
                await CargarDatosAsyncDocumentos(); // Carga asíncrona
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
                if(!string.Equals(NombreEntrevistado, _bkNombreEntrevistado) || 
                    !string.Equals(Dni, _bkDni) || 
                    !string.Equals(Otro, _bkOtro))
                {
                    _bkNombreEntrevistado = NombreEntrevistado;
                    _bkDni = Dni;
                    _bkOtro = Otro;

                    //MessageBox.Show($"Texto guardado: {NombreEntrevistado}");
                    string consultaEmpleado = consultaDataRow();
                    Cargar(consultaEmpleado);
                }
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            bool borradoExitoso = await BorrarEmpleadoAsync(_codSeleccionado);

            if (borradoExitoso)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Empleado borrado correctamente");
                });
            }

        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show($"Texto guardado: {NombreEntrevistado}");
            Direccion = "1";
        }



        private string consultaDataRow()
        {
            string nombre = NombreEntrevistado;
            string dni = Dni;
            string consulta = "";

            //Actualización de strings para cuando se cambie la tabla:
            consulta = "SELECT * FROM EMPLEADOS";
            if (nombre.Length > 0 && dni.Length > 0)
            {
                consulta += String.Concat(
                    " WHERE NOMBRE like '%", nombre,
                    "%' and DNI like '%", dni, "%'");
            }
            else if (dni.Length > 0)
            {
                consulta += String.Concat(" WHERE DNI like '%", dni, "%'");
            }
            else if (nombre.Length > 0)
            {
                consulta += String.Concat(" WHERE NOMBRE like '%", nombre, "%'");
            }
            return consulta;

        }
        //MUESTRA DE DATOS
        private async Task CargarDatosAsyncDataGrid(string consultaEmpleado)
        {

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(); // Método asíncrono
                using (var command = new SqlCommand(consultaEmpleado, connection))
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

        //CONSULTA INFORMES
        private async Task CargarDatosAsyncInformes()
        {
            string consulta = "SELECT * FROM informe_a_fecha WHERE FK_CODIGO_EMP LIKE @Codigo";

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(consulta, connection))
                    {
                        // Parámetro seguro
                        command.Parameters.AddWithValue("@Codigo", $"%{_codSeleccionado}%");

                        // Ejecutar consulta
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var informes = new ObservableCollection<Informe>();

                            while (await reader.ReadAsync())
                            {
                                // Mapear manualmente los datos
                                var informe = new Informe
                                {
                                    CODIGO_INF = reader.GetInt32(reader.GetOrdinal("CODIGO_INF")),
                                    FECHA = reader.GetDateTime(reader.GetOrdinal("FECHA")),
                                    FK_CODIGO_EMP = reader.GetInt32(reader.GetOrdinal("FK_CODIGO_EMP"))
                                };
                                informes.Add(informe);
                            }

                            // Actualizar UI
                            Dispatcher.Invoke(() =>
                            {
                                listBox.ItemsSource = informes;
                                listBox.DisplayMemberPath = "FECHA";
                            });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error SQL: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error general: {ex.Message}");
            }

        }


        //CONSULTA INFORMES
        private async Task CargarDatosAsyncDocumentos()
        {
            string consulta = "SELECT * FROM CONTENIDOS_EN_EL_CV WHERE CODIGO_INF LIKE " +
                "(SELECT CODIGO_INF FROM INFORME_A_FECHA WHERE FK_CODIGO_EMP LIKE @Codigo)";

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(consulta, connection))
                    {
                        // Parámetro seguro
                        command.Parameters.AddWithValue("@Codigo", $"%{_codSeleccionado}%");

                        // Ejecutar consulta
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var documentos = new ObservableCollection<CONTENIDOS_EN_EL_CV>();

                            while (await reader.ReadAsync())
                            {
                                // Mapear manualmente los datos
                                var documento = new CONTENIDOS_EN_EL_CV
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    RutaArchivo = reader.GetString(reader.GetOrdinal("RutaArchivo")),
                                    TipoMime = reader.GetString(reader.GetOrdinal("TipoMime")),
                                    FK_CODIGO_INF = reader.GetInt32(reader.GetOrdinal("FK_CODIGO_INF"))
                                };
                                documentos.Add(documento);
                            }

                            // Actualizar UI
                            Dispatcher.Invoke(() =>
                            {
                                listBoxDocumentos.ItemsSource = documentos;
                                listBoxDocumentos.DisplayMemberPath = "DOCUMENTOS"; // Asegúrate de que existe esta propiedad
                            });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error SQL: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error general: {ex.Message}");
            }

        }

        //BORRAR EMPLEADO
        public async Task<bool> BorrarEmpleadoAsync(int codigoEmpleado)
        {
            string consulta = "DELETE FROM EMPLEADOS WHERE CODIGO_INF = @CodigoEmp";

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(consulta, connection))
                    {
                        // Parámetro para evitar inyección SQL
                        command.Parameters.AddWithValue("@CodigoEmp", codigoEmpleado);

                        // Ejecutar el comando y obtener filas afectadas
                        int filasAfectadas = await command.ExecuteNonQueryAsync();

                        // Devolver true si se eliminó al menos una fila
                        return filasAfectadas > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                // Manejar errores específicos de SQL
                MessageBox.Show($"Error al borrar el informe: {ex.Message}");
                return false;
            }
        }

        private void escribirInforme(Informe informe)
        {
            string resultado = "El empleado tiene ";
            Direccion = informe.FECHA.ToString();


        }
    }
}