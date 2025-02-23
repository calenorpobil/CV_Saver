using Microsoft.Data.SqlClient;
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
        public ObservableCollection<Informe> Informes { get; set; }
        public ObservableCollection<CONTENIDOS_EN_EL_CV> Documentos { get; set; }
        Dictionary<int, int> cods_informes_en_listbox = new Dictionary<int, int>();
        Empleado empActual;
        Informe infActual;



        private Empleado _empleadoSeleccionado;
        public Empleado EmpleadoSeleccionado
        {
            get => _empleadoSeleccionado;
            set
            {
                _empleadoSeleccionado = value;
                OnPropertyChanged(nameof(Empleado));
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
            Documentos = new ObservableCollection<CONTENIDOS_EN_EL_CV>();
            Informes = new ObservableCollection<Informe>();

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

            InformeMostrar = null; // Importante

            int len = dataGrid.Columns.Count;
            // Obtener el CODIGO de Empleado:
            _codSeleccionado = getCodigoDataGrid();

            lbNombre.Content = getNombreDataGrid();
            empActual = new Empleado(getNombreDataGrid(), getDniDataGrid());

            if (!empActual.getDni().IsNullOrEmpty())
            {
                CargarInformes();
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
        //CARGAR DOCUMENTOS
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
                bool borradoExitoso = await BorrarEmpleadoAsync();

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
        private void lbInformes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int seleccionado = listBox.SelectedIndex;
            if (listBox != null) 
                InformeMostrar = Informes[seleccionado];
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
            Informe nuevo = null;
            if (empActual!=null && empActual.getNombre()!="")
            {
                VentanaNuevoInforme win2 = new VentanaNuevoInforme();
                win2.Owner=this;
                win2.lbEmpleadoInforme.Content = "Empleado: "+empActual.getNombre();

                if (win2.ShowDialog() == true) // Si el usuario aceptó
                {
                    string resultado = win2.Resultado;
                    MessageBox.Show($"Resultado: {resultado}");
                    DateTime fecha = DateTime.Parse(resultado);
                    nuevo = new Informe(fecha, empActual.getDni());
                    NuevoInformeAsync(fecha, empActual.getDni());
                    Informes.Add(nuevo);
                    CargarInformes();
                }
            }
        }
        private void btEditarInforme_Click(object sender, RoutedEventArgs e)
        {
            Informe nuevo = InformeMostrar;
            if (nuevo!=null && empActual != null && empActual.getNombre() != "")
            {
                VentanaEditarInforme win2 = new VentanaEditarInforme();
                win2.Owner = this;
                win2.lbEmpleadoInforme.Content = "Empleado: " + empActual.getNombre();
                win2.dpFecha.SelectedDate = InformeMostrar.FECHA;

                if (win2.ShowDialog() == true) // Si el usuario aceptó
                {
                    string resultado = win2.Resultado;
                    DateTime fecha = DateTime.Parse(resultado);
                    nuevo = new Informe(fecha, empActual.getDni());
                    Informes.Add(nuevo);
                    _ = EditarInformeAsync(fecha, empActual.getDni());
                    CargarInformes();
                }
            }


        }
        private void btBorrarInforme_Click(object sender, RoutedEventArgs e)
        {


        }

        private void txEntrevistado_GotFocus(object sender, RoutedEventArgs e)
        {
            lbNombre.Content = "Selecciona un entrevistado";


        }

        private void btNuevoDocumento_click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".png";
            dlg.Filter = "";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                string tipo = dlg.GetType().Name;
                CONTENIDOS_EN_EL_CV nuevoDoc = new CONTENIDOS_EN_EL_CV(filename, tipo, InformeMostrar.getFechaInf());
                _ = NuevoDocumentoAsync(nuevoDoc);
                CargarDocumentos();
            }
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
                MessageBox.Show($"Error cargando los DataGrid: {ex.Message}");
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
                            Informes = new ObservableCollection<Informe>();
                            int num = 0;
                            cods_informes_en_listbox.Clear();

                            Documentos?.Clear(); // Limpiar la colección de documentos
                            while (await reader.ReadAsync())
                            {
                                // Mapear manualmente los datos
                                var informe = new Informe
                                {
                                    FECHA = reader.GetDateTime(reader.GetOrdinal("FECHA")),
                                    FK_CODIGO_EMP = reader.GetString(reader.GetOrdinal("FK_CODIGO_EMP"))
                                };
                                Informes.Add(informe);
                                num++;
                            }

                            // Actualizar UI
                            Dispatcher.Invoke(() =>
                            {
                                listBox.ItemsSource = Informes;
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
                MessageBox.Show($"Error cargando los informes: {ex.Message}");
            }

        }


        /**
         * MUESTRA DOCUMENTOS
         */
        private async Task CargarDatosAsyncDocumentos()
        {
            if (InformeMostrar == null) // Si no hay informe seleccionado
            {
                Documentos?.Clear();
                Dispatcher.Invoke(() =>
                {
                    listBoxDocumentos.ItemsSource = null;
                    listBoxDocumentos.Items.Refresh();
                });
                return;
            }
            string consulta = "SELECT * FROM CONTENIDOS_EN_EL_CV WHERE FK_CODIGO_INF = '"+
                InformeMostrar.getFechaInf().ToString()+"'";

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(consulta, connection))
                    {
                        // Parámetro seguro
                        if (InformeMostrar != null)
                        {
                            //command.Parameters.AddWithValue("@Codigo", $"{InformeMostrar.getFechaInf().ToString()}");
                            //MessageBox.Show(InformeMostrar.getFechaInf().ToString());
                        }
                        else
                            command.Parameters.AddWithValue("@Codigo", "");


                        // Ejecutar consulta
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            Documentos = new ObservableCollection<CONTENIDOS_EN_EL_CV>();

                            while (await reader.ReadAsync())
                            {
                                // Mapear manualmente los datos
                                var documento = new CONTENIDOS_EN_EL_CV
                                {
                                    RutaArchivo = reader.GetString(reader.GetOrdinal("RutaArchivo")),
                                    TipoMime = reader.GetString(reader.GetOrdinal("TipoMime")),
                                    FK_CODIGO_INF = reader.GetDateTime(reader.GetOrdinal("FK_CODIGO_INF"))
                                };
                                Documentos.Add(documento);
                            }

                            // Actualizar UI
                            Dispatcher.Invoke(() =>
                            {
                                listBoxDocumentos.ItemsSource = Documentos;
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
                MessageBox.Show($"Error cargando los documentos: {ex.Message}");
            }

        }

        //BORRAR EMPLEADO
        public async Task<bool> BorrarEmpleadoAsync()
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
        //NUEVO INFORME
        public async Task<bool> NuevoInformeAsync(DateTime fecha, string foranea)
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
                        command.Parameters.AddWithValue("@Fecha", $"{fecha}");
                        command.Parameters.AddWithValue("@Foranea", $"{foranea}");
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
        //EDITAR INFORME
        public async Task<bool> EditarInformeAsync(DateTime fecha, string foranea)
        {
            string consulta = "UPDATE INFORME_A_FECHA " +
                "SET FECHA = @Fecha WHERE FK_CODIGO_EMP = @ForaneaEmp and FECHA = @id";
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(consulta, connection))
                    {
                        // Parámetro para evitar inyección SQL
                        int a = 0;
                        command.Parameters.AddWithValue("@Fecha", $"{fecha}");
                        command.Parameters.AddWithValue("@ForaneaEmp", $"{empActual.getDni()}");
                        try
                        {
                            a = command.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            if (ex.ErrorCode == -2146232060)
                            {
                                MessageBox.Show("El informe ya existe. ");
                                return false;
                            }
                            else
                            {
                                MessageBox.Show($"Error al registrar el informe: {ex.Message}. Código:  {ex.ErrorCode}");
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
        //NUEVO DOCUMENTO
        public async Task<bool> NuevoDocumentoAsync(CONTENIDOS_EN_EL_CV doc)
        {
            string consulta = "INSERT INTO CONTENIDOS_EN_EL_CV " +
                "(RutaArchivo, TipoMime, FK_CODIGO_INF) VALUES (@Ruta, @Tipo, @Informe)";
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(consulta, connection))
                    {
                        // Parámetro para evitar inyección SQL
                        int a = 0;
                        command.Parameters.AddWithValue("@Ruta", $"{doc.RutaArchivo}");
                        command.Parameters.AddWithValue("@Tipo", $"{doc.TipoMime}");
                        command.Parameters.AddWithValue("@Informe", $"{doc.FK_CODIGO_INF}");
                        try
                        {
                            a = command.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            if (ex.ErrorCode == -2146232060)
                            {
                                MessageBox.Show("Ya existe otro documento igual. ");
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
                            MessageBox.Show("Data add Sucessfully");
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