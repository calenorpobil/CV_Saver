﻿using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
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
        private string _direccion, _años, _experiencia, _empleado;
        private string connectionString;
        private int _codSeleccionado, _informeSeleccionado;
        private Informe InformeMostrar;
        public ObservableCollection<Empleado> Empleados { get; set; }
        Dictionary<int, int> cods_informes_en_listbox = new Dictionary<int, int>();
        Empleado empActual;



        private Empleado _empleadoSeleccionado;
        public Empleado EmpleadoSeleccionado
        {
            get => _empleadoSeleccionado;
            set
            {
                _empleadoSeleccionado = value;
                OnPropertyChanged(nameof(Empleado));
                // Lógica cuando cambia la selección
            }
        }
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
            _otro = "DESKTOP-NNKTF0L\\SQLEXPRESS";
            //_otro = "DESKTOP-MDAC0QE\\SQLEXPRESS";


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
            // Obtener el CODIGO de Empleado:
            _codSeleccionado = getCodigoDataGrid();

            lbNombre.Content = getNombreDataGrid();
            empActual = new Empleado(getNombreDataGrid(), getDniDataGrid());

            if (!empActual.getDni().IsNullOrEmpty())
            {
                CargarInformes();
                CargarDocumentos();
            }
        }

        private string getNombreDataGrid()
        {
            string nombre = "";
            var column = dataGrid.Columns[0];
            var cellContent = column.GetCellContent(dataGrid.CurrentCell.Item);

            if (cellContent is TextBlock textNombre)
            {
                string valor = textNombre.Text;
                try
                {
                    nombre = valor;
                }
                catch (FormatException)
                {
                }
            }
            return nombre;
        }
        private string getDniDataGrid()
        {
            string nombre = "";
            var column = dataGrid.Columns[1];
            var cellContent = column.GetCellContent(dataGrid.CurrentCell.Item);

            if (cellContent is TextBlock textNombre)
            {
                string valor = textNombre.Text;
                try
                {
                    nombre = valor;
                }
                catch (FormatException)
                {
                }
            }
            return nombre;
        }

        private int getCodigoDataGrid()
        {
            int result = 0;
            var column = dataGrid.Columns[1];
            var cellContent = column.GetCellContent(dataGrid.CurrentCell.Item);

            if (cellContent is TextBlock textBlock)
            {
                string valor = textBlock.Text;
                try
                {
                    result = int.Parse(valor);

                }
                catch (FormatException)
                {
                }
            }
            return result;
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
        private async void CargarInformes()
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

        private async void btBorrar_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¿Estás seguro de que quieres borrar a "+empActual.getNombre()+
                " con DNI "+empActual.getDni()+"?",
                                "Save file",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                bool borradoExitoso = await BorrarEmpleadoAsync(_codSeleccionado);

                if (borradoExitoso)
                {
                    Dispatcher.Invoke(() =>
                    {
                        //MessageBox.Show("Empleado borrado correctamente");
                    });
                    string consultaEmpleado = consultaDataRow();
                    Cargar(consultaEmpleado);
                }

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
            consulta = "SELECT * FROM EMPLEADO";
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


        /**
         * INFORME SELECCIONADO, BUSCAR DOCUMENTOS
         */
        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int seleccionado = listBox.SelectedIndex;
            if (cods_informes_en_listbox.ContainsKey(seleccionado))
            {
                _informeSeleccionado = cods_informes_en_listbox[seleccionado];
            }
            CargarDocumentos();

            /*
            var selectedCell = listBox.SelectedItem;
            if (selectedCell != null) return;
            // Obtener los Documentos del informe
            using (var connect = new SqlConnection(connectionString))
            {
                connect.Open();

                string readString = "SELECT * FROM CONTENIDOS_EN_EL_CV WHERE FK_CODIGO_INF LIKE @Codigo";
                SqlCommand readCommand = new SqlCommand(readString, connect);

                using (SqlDataReader dataRead = readCommand.ExecuteReader())
                {
                    if (dataRead != null)
                    {
                        while (dataRead.Read())
                        {



                        
                        }
                    }
                }

                connect.Close();
            }*/


        }

        /**
         * BOTÓN REGISTRAR
         */
        private async void btRegistrar_Click(object sender, RoutedEventArgs e)
        {
            string nombre = txEntrevistado.Text, dni = txDni.Text;
            // ¿Campos vacíos?
            if (nombre.IsNullOrEmpty() && dni.IsNullOrEmpty())
            {
                MessageBox.Show("Rellena los campos de arriba. ");
            }
            else
            {
                //MOSTRAR MENSAJE
                if (MessageBox.Show("¿Estás seguro de que quieres registrar a " +
                txEntrevistado.Text + " con DNI " + txDni.Text + "?",
                    "Save file",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    bool borradoExitoso = await RegistarEmpleadoAsync();

                    if (borradoExitoso)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            //MessageBox.Show("Empleado registrado correctamente");
                        });
                    }
                    string consultaEmpleado = consultaDataRow();
                    Cargar(consultaEmpleado);

                }
            }
        }
        

        private void listBoxDocumentos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


        private void btNuevoInforme_Click(object sender, RoutedEventArgs e)
        {
            Window1 win2 = new Window1();
            win2.Show();


        }
        private void btEditarInforme_Click(object sender, RoutedEventArgs e)
        {
            Window1 win2 = new Window1();
            MainWindow window = new MainWindow();
            
            win2.ShowDialog();


        }
        private void btBorrarInforme_Click(object sender, RoutedEventArgs e)
        {
            Window1 win2 = new Window1();
            win2.Show();


        }

        private void txEntrevistado_GotFocus(object sender, RoutedEventArgs e)
        {
            lbNombre.Content = "Selecciona un entrevistado";


        }


        //MUESTRA DE DATOS
        private async Task CargarDatosAsyncDataGrid(string consultaEmpleado)
        {

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(consultaEmpleado, connection))
                    {
                        var dataTable = new DataTable();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            dataTable.Load(reader);
                        }

                        Dispatcher.Invoke(() =>
                        {
                            dataGrid.ItemsSource = dataTable.DefaultView;
                        });
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error de SQL: {ex.Message}\nNúmero de error: {ex.Number}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error general: {ex.Message}");
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
                        //TODO
                        // Parámetro seguro
                        if (!_codSeleccionado.Equals(null))
                        {
                            command.Parameters.AddWithValue("@Codigo", $"%{empActual.getDni()}%");
                        }

                        // Ejecutar consulta
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var informes = new ObservableCollection<Informe>();
                            int num=0;
                            cods_informes_en_listbox.Clear();
                            while (await reader.ReadAsync())
                            {
                                // Mapear manualmente los datos
                                int codigo = reader.GetInt32(reader.GetOrdinal("CODIGO_INF"));
                                var informe = new Informe
                                {
                                    CODIGO_INF = codigo,
                                    FECHA = reader.GetDateTime(reader.GetOrdinal("FECHA")),
                                    FK_CODIGO_EMP = reader.GetString(reader.GetOrdinal("FK_CODIGO_EMP"))
                                    
                                };
                                cods_informes_en_listbox.Add(num, codigo);
                                informes.Add(informe);
                                num++;
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


        /**
         * MUESTRA DOCUMENTOS
         */
        private async Task CargarDatosAsyncDocumentos()
        {
            string consulta = "SELECT * FROM CONTENIDOS_EN_EL_CV WHERE FK_CODIGO_INF LIKE " +
                "(SELECT CODIGO_INF FROM INFORME_A_FECHA WHERE FK_CODIGO_EMP LIKE @Codigo)";

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(consulta, connection))
                    {
                        // Parámetro seguro
                        command.Parameters.AddWithValue("@Codigo", $"%{empActual.getDni()}%");

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
                                listBoxDocumentos.DisplayMemberPath = "RutaArchivo"; 
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
            string consulta = "DELETE FROM EMPLEADO WHERE DNI = @CodigoEmp";

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(consulta, connection))
                    {

                        string dni = getDniDataGrid();
                        // Parámetro para evitar inyección SQL
                        command.Parameters.AddWithValue("@CodigoEmp", empActual.getDni());


                        int a = command.ExecuteNonQuery();
                        if (a == 1)
                        {
                            //MessageBox.Show("Data add Sucessfully");
                            return true;
                        }
                        return false;
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
        //BORRAR EMPLEADO
        public async Task<bool> RegistarEmpleadoAsync()
        {
            string consulta = "INSERT INTO EMPLEADO (NOMBRE, DNI) VALUES (@Nombre, @Dni)";

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(consulta, connection))
                    {
                        // Parámetro para evitar inyección SQL
                        int a = 0;
                        command.Parameters.AddWithValue("@Nombre", $"{txEntrevistado.Text}");
                        command.Parameters.AddWithValue("@Dni", $"{txDni.Text}");
                        try
                        {
                            a = command.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            if (ex.ErrorCode == -2146232060)
                            {
                                MessageBox.Show("El empleado ya existe. Elige otro DNI. ");
                                return false;
                            }
                            else
                            {
                                MessageBox.Show($"Error al registrar el informe: {ex.Message}, error:  {ex.ErrorCode}");
                                return false;
                            }
                        }
                        if (a == 1)
                        {
                            //MessageBox.Show("Data add Sucessfully");
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (SqlException ex)
            {
                // Manejar errores específicos de SQL
                MessageBox.Show($"Error al registrar el informe: {ex.Message}");
                return false;
            }
        }
        //BORRAR EMPLEADO
        public async Task<bool> NuevoInformeAsync()
        {
            string consulta = "INSERT INTO INFORME_A_FECHA " +
                "(FECHA, FK_CODIGO_EMP) VALUES (@Fecha, @Foranea)";

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(consulta, connection))
                    {
                        // Parámetro para evitar inyección SQL
                        int a = 0;
                        //command.Parameters.AddWithValue("@Codigo", $"{txEntrevistado.Text}");
                        command.Parameters.AddWithValue("@Fecha", $"{txDni.Text}");
                        command.Parameters.AddWithValue("@Foranea", $"{txDni.Text}");
                        try
                        {
                            a = command.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            if (ex.ErrorCode == -2146232060)
                            {
                                MessageBox.Show("El empleado ya existe. Elige otro DNI. ");
                                return false;
                            }
                            else
                            {
                                MessageBox.Show($"Error al registrar el informe: {ex.Message}, error:  {ex.ErrorCode}");
                                return false;
                            }
                        }
                        if (a == 1)
                        {
                            //MessageBox.Show("Data add Sucessfully");
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (SqlException ex)
            {
                // Manejar errores específicos de SQL
                MessageBox.Show($"Error al registrar el informe: {ex.Message}");
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