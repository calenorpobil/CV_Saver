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
        public ObservableCollection<Documento> Documentos { get; set; }
        public ObservableCollection<Concepto> Conceptos { get; set; }
        Dictionary<int, int> cods_informes_en_listbox = new Dictionary<int, int>();
        Empleado empActual;
        Informe infActual;
        Documento docActual;
        Concepto conceptoActual;



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
            Empleados = new ObservableCollection<Empleado>();
            Documentos = new ObservableCollection<Documento>();
            Informes = new ObservableCollection<Informe>();
            Conceptos = new ObservableCollection<Concepto>();

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
            int fila = dataGrid.SelectedIndex;
            if (!selectedCell.IsValid) return;

            Dispatcher.Invoke(() =>
            {
                listBox.SelectedIndex = -1; // Resetear el índice seleccionado
            });



            InformeMostrar = null; // Importante
            tbDescripcionConcepto.Text = "";
            tbDescripcionConcepto.IsEnabled = false;

            int len = dataGrid.Columns.Count;
            // Obtener el CODIGO del Empleado seleccionado:
            _codSeleccionado = getCodigoDataGrid();
            lbNombre.Content = getNombreDataGrid();

            
            empActual = new Empleado(getNombreDataGrid(), getDniDataGrid());
            

            if (!empActual.getDni().IsNullOrEmpty())
            {
                CargarInformes();
                CargarDocumentos();
                CargarConceptos();
                tbDescripcionConcepto.Text = "";
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
            if(empActual != null)
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
                listBox.SelectedIndex = 0;
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
            listBoxDocumentos.SelectedIndex = 0;
        }

        //CARGAR CONCEPTOS
        private async void CargarConceptos()
        {
            if (empActual.getNombre() != "")
            {
                connectionString = String.Concat("Server=", Otro, "; Database=CV-RRHH",
                "; Integrated Security=True; TrustServerCertificate=True");

                try
                {
                    await CargarDatosAsyncConceptos(); // Carga asíncrona
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                lbConceptos.SelectedIndex = 0;
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
            if (empActual!=null && empActual.getNombre()!="")
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
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show($"Texto guardado: {NombreEntrevistado}");
            Direccion = "1";
        }



        /**
         * HACER BÚSQUEDAS
         */
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
            if (empActual.getNombre() != "")
            {
                int seleccionado = listBox.SelectedIndex;


                if (seleccionado >= 0 && seleccionado < Informes.Count)
                {
                    InformeMostrar = Informes[seleccionado];
                    CargarDocumentos();
                    CargarConceptos();
                    txContenido.Content = "Selecciona un concepto";
                    tbDescripcionConcepto.Text = "";
                }
                else
                {
                    InformeMostrar = null; // Limpiar si el índice no es válido
                }
            }

        }

        /**
         * SELECCIONAR CONCEPTO
         */
        private void listBoxConceptos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int seleccionado = lbConceptos.SelectedIndex;

            if (InformeMostrar != null && empActual != null)
            {
                if (seleccionado >= 0 && seleccionado < Conceptos.Count)
                {
                    tbDescripcionConcepto.IsEnabled = true;
                    conceptoActual = Conceptos[seleccionado];
                    txContenido.Content = conceptoActual.getNombre();
                    tbDescripcionConcepto.Text = conceptoActual.Descripcion;
                }
                else
                {
                    tbDescripcionConcepto.IsEnabled = false;
                    conceptoActual = null; // Limpiar si el índice no es válido
                }
            }

        }

        /**
         * BOTÓN REGISTRAR
         */
        private async void btRegistrar_Click(object sender, RoutedEventArgs e)
        {
            string nombre = txEntrevistado.Text, dni = txDni.Text;
            // ¿Campos vacíos?
            if (nombre.IsNullOrEmpty() || dni.IsNullOrEmpty())
            {
                MessageBox.Show("Rellena los campos de arriba. ");
            }
            else
            {
                //MOSTRAR MENSAJE
                if (txDni.Text.IsNullOrEmpty())
                {
                    MessageBox.Show("El DNI no puede estar vacío. ");
                }
                else if (nombre.IsNullOrEmpty())
                {
                    MessageBox.Show("El nombre no puede estar vacío. ");
                }
                else{
                    if (MessageBox.Show("¿Estás seguro de que quieres registrar a " +
                    txEntrevistado.Text + " con DNI " + txDni.Text + "?",
                        "Save file",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        bool registroExitoso = await RegistarEmpleadoAsync();

                        if (registroExitoso)
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
                txDni.Text = "";
                txEntrevistado.Text = "";
            }
        }


        private void listBoxDocumentos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int seleccionado = listBoxDocumentos.SelectedIndex;

            if (InformeMostrar != null  && empActual!=null)
            {
                if (seleccionado >= 0 && seleccionado < Documentos.Count)
                {
                    docActual = Documentos[seleccionado];
                }
                else
                {
                    docActual = null; // Limpiar si el índice no es válido
                }
            }
            

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
                    DateTime fecha = DateTime.Parse(resultado);
                    nuevo = new Informe(fecha, empActual.getDni());
                    NuevoInformeAsync(fecha, empActual.getDni());
                    Informes.Add(nuevo);
                    CargarInformes();
                    CargarDocumentos();
                }
            }
        }
        private void btEditarInforme_Click(object sender, RoutedEventArgs e)
        {
            Informe nuevo = InformeMostrar;
            if (nuevo!=null && empActual != null && empActual.getNombre() != "")
            {
                var viejaFecha = InformeMostrar.FECHA;
                VentanaEditarInforme win2 = new VentanaEditarInforme();
                win2.Owner = this;
                win2.lbEmpleadoInforme.Content = "Empleado: " + empActual.getNombre();
                win2.dpFecha.SelectedDate = viejaFecha;

                if (win2.ShowDialog() == true) // Si el usuario aceptó
                {
                    string resultado = win2.Resultado;
                    DateTime fecha = DateTime.Parse(resultado);
                    _ = EditarInformeAsync(fecha, empActual.getDni(), viejaFecha);
                    Informes.Remove(nuevo);
                    nuevo = new Informe(fecha, empActual.getDni());
                    Informes.Add(nuevo);
                    CargarInformes();
                }
            }


        }
        private async void btBorrarInforme_Click(object sender, RoutedEventArgs e)
        {
            if (empActual != null && empActual.getNombre() != "")
            {
                if (MessageBox.Show("¿Estás seguro de que quieres borrar el informe del día " + InformeMostrar.getFechaInf() +
                ", de " + empActual.getNombre() + "?",
                    "Save file",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    bool borradoExitoso = await BorrarInformeAsync(InformeMostrar.getFechaInf());

                    if (borradoExitoso)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            //MessageBox.Show("Empleado borrado correctamente");
                            CargarInformes();
                        });
                    }

                }
            }



        }


        //BORRAR INFORME
        private async Task<bool> BorrarInformeAsync(DateTime viejaFecha)
        {
            string consulta = "DELETE FROM INFORME_A_FECHA " +
                            "WHERE (FK_CODIGO_EMP = @ForaneaEmp and FECHA = @id)";
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(consulta, connection))
                    {
                        // Parámetro para evitar inyección SQL
                        int a = 0;
                        command.Parameters.AddWithValue("@ForaneaEmp", $"{empActual.getDni()}");
                        command.Parameters.AddWithValue("@id", $"{viejaFecha}");
                        try
                        {
                            a = command.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show($"Error al borrar el informe: {ex.Message}. Código:  {ex.ErrorCode}");
                            return false;
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
                MessageBox.Show($"Error al borrar el informe: {ex.Message}");
                return false;
            }
        }

        private void txEntrevistado_GotFocus(object sender, RoutedEventArgs e)
        {
            lbNombre.Content = "Selecciona un entrevistado";


        }

        private void btNuevoDocumento_click(object sender, RoutedEventArgs e)
        {
            if (InformeMostrar != null)
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
                    Documento nuevoDoc = new Documento(filename, tipo, InformeMostrar.getFechaInf());
                    _ = NuevoDocumentoAsync(nuevoDoc);
                    CargarDocumentos();
                }

            }
        }

        private void btEditarDocumento_click(object sender, RoutedEventArgs e)
        {

            if (InformeMostrar != null)
            {
                string rutaAntigua = docActual.RutaArchivo;
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
                    Documento nuevoDoc = new Documento(filename, tipo, InformeMostrar.getFechaInf());
                    _ = EditarDocumentoAsync(nuevoDoc, rutaAntigua);
                    CargarDocumentos();
                }
            

            }
        }

        private async void btBorrarDocumento_click(object sender, RoutedEventArgs e)
        {
            if (InformeMostrar != null)
            {
                if (MessageBox.Show("¿Estás seguro de que quieres borrar el documento " + docActual.RutaArchivo +
                    " del empleado " + empActual.getNombre() + "?",
                                    "Save file",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    bool borradoExitoso = await BorrarDocumentoAsync(docActual);

                    if (borradoExitoso)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            //MessageBox.Show("Empleado borrado correctamente");
                        });
                        CargarDocumentos();
                    }
                }
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
            string consulta = "SELECT * FROM informe_a_fecha WHERE FK_CODIGO_EMP = '"+ empActual.getDni()+"'";

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
                            //command.Parameters.AddWithValue("@Codigo", $"%{empActual.getDni()}%");
                        }

                        // Ejecutar consulta
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int num = 0;
                            Informes = new ObservableCollection<Informe>();
                            var informesTemporales = new List<Informe>(); // Lista temporal para evitar problemas de sincronización
                            // Limpiar la colección de documentos
                            Documentos = new ObservableCollection<Documento>(); 
                            while (await reader.ReadAsync())
                            {
                                // Mapear manualmente los datos
                                var informe = new Informe
                                {
                                    FECHA = reader.GetDateTime(reader.GetOrdinal("FECHA")),
                                    FK_CODIGO_EMP = reader.GetString(reader.GetOrdinal("FK_CODIGO_EMP"))
                                };
                                informesTemporales.Add(informe);
                                num++;
                            }

                            // Actualizar UI
                            Dispatcher.Invoke(() =>
                            {
                                Informes.Clear();
                                Informes = new ObservableCollection<Informe>(informesTemporales);
                                if (Informes!=null && Informes.Count > 0)
                                    InformeMostrar = Informes[0];
                                else
                                    InformeMostrar = null;

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
            string consulta = "SELECT * FROM CONTENIDOS_EN_EL_CV WHERE FK_CODIGO_INF = '" +
                InformeMostrar.getFechaInf().ToString() + "' and FK_CODIGO_EMP_C = '"+empActual.getDni()+"'";

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
                            Documentos = new ObservableCollection<Documento>();

                            while (await reader.ReadAsync())
                            {
                                // Mapear manualmente los datos
                                var documento = new Documento
                                {
                                    RutaArchivo = reader.GetString(reader.GetOrdinal("RutaArchivo")),
                                    TipoMime = reader.GetString(reader.GetOrdinal("TipoMime")),
                                    FK_CODIGO_INF = reader.GetDateTime(reader.GetOrdinal("FK_CODIGO_INF"))
                                };
                                Documentos.Add(documento);
                                docActual = Documentos[0];
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
        /**
         * MUESTRA CONCEPTOS
         */
        private async Task CargarDatosAsyncConceptos()
        {
            if (InformeMostrar == null) // Si no hay informe seleccionado
            {
                Conceptos?.Clear();
                Dispatcher.Invoke(() =>
                {
                    lbConceptos.ItemsSource = null;
                    lbConceptos.Items.Refresh();
                });
                return;
            }
            string consulta = "SELECT * FROM CONCEPTOS_PARA_CV WHERE FK_CODIGO_INF = @CodigoInf";
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
                            command.Parameters.AddWithValue("@CodigoInf", InformeMostrar.getFechaInf().ToString());
                            //MessageBox.Show(InformeMostrar.getFechaInf().ToString());
                        }
                        else
                            command.Parameters.AddWithValue("@Codigo", "");


                        // Ejecutar consulta
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            Conceptos = new ObservableCollection<Concepto>();

                            while (await reader.ReadAsync())
                            {
                                // Mapear manualmente los datos
                                var concepto = new Concepto(
                                    reader.GetString(reader.GetOrdinal("NOMBRE")),
                                    reader.GetString(reader.GetOrdinal("DESCRIPCION")),
                                    reader.GetDateTime(reader.GetOrdinal("FK_CODIGO_INF"))
                                    );
                                
                                Conceptos.Add(concepto);
                            }

                            // Actualizar UI
                            Dispatcher.Invoke(() =>
                            {
                                lbConceptos.ItemsSource = Conceptos;
                                lbConceptos.DisplayMemberPath = "Nombre";
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

        private void btNuevoConcepto_click(object sender, RoutedEventArgs e)
        {
            if (InformeMostrar != null && empActual.getNombre()!="")
            {
                VentanaNuevoConcepto v = new VentanaNuevoConcepto();
                v.Owner = this;

                Concepto nuevo;

                if (v.ShowDialog() == true) // Si el usuario aceptó
                {
                    string resultado = v.Resultado;
                    if (resultado != "")
                    {
                        nuevo = new Concepto(resultado, "", InformeMostrar.getFechaInf());
                        _ = NuevoConceptoAsync(nuevo);
                        Conceptos.Add(nuevo);
                        CargarConceptos();
                    }
                    else
                    {
                        MessageBox.Show("Escribe un nombre de concepto adecuado. ");
                    }
                
                }
            }


        }
        private void btEditarConcepto_click(object sender, RoutedEventArgs e)
        {
            if (InformeMostrar != null)
            {
                VentanaEditarConcepto v = new VentanaEditarConcepto();
                v.Owner = this;

                v.txConcepto.Text = conceptoActual.getNombre();
                string nombreViejo = conceptoActual.getNombre();
                string desc = conceptoActual.getDescripcion();
                Concepto nuevo;

                if (v.ShowDialog() == true) // Si el usuario aceptó
                {

                    string resultado = v.Resultado;
                    Conceptos.Remove(conceptoActual);
                    nuevo = new Concepto(resultado, desc, InformeMostrar.getFechaInf());
                    _ = EditarConceptoAsync(nuevo, nombreViejo);
                    Conceptos.Add(nuevo);
                    CargarConceptos();
                    tbDescripcionConcepto.Text = "";
                }
            }

        }

        private async void btBorrarConcepto_click(object sender, RoutedEventArgs e)
        {
            if (InformeMostrar != null)
            { 
                if (MessageBox.Show("¿Estás seguro de que quieres borrar el concepto " + conceptoActual.getNombre() +
                " de  " + empActual.getNombre() + "?",
                                "Save file",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    bool borradoExitoso = await BorrarConceptoAsync(conceptoActual);

                    if (borradoExitoso)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            //MessageBox.Show("Empleado borrado correctamente");
                        });
                        CargarConceptos();
                        tbDescripcionConcepto.Text = "";
                    }

                }
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
                                MessageBox.Show("El empleado ya tiene otro informe con la misma fecha. Elige otra. ");
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
        public async Task<bool> EditarInformeAsync(DateTime nuevaFecha, 
            string foranea, DateTime viejaFecha)
        {
            string consulta = "UPDATE INFORME_A_FECHA " +
                "SET FECHA = @Fecha, FK_CODIGO_EMP = @ForaneaEmp " +
                "WHERE (FK_CODIGO_EMP = @ForaneaEmp and FECHA = @id)";
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(consulta, connection))
                    {
                        // Parámetro para evitar inyección SQL
                        int a = 0;
                        command.Parameters.AddWithValue("@Fecha", $"{nuevaFecha}");
                        command.Parameters.AddWithValue("@ForaneaEmp", $"{empActual.getDni()}");
                        command.Parameters.AddWithValue("@id", $"{viejaFecha}");
                        try
                        {
                            a = command.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show($"Error al registrar el informe: {ex.Message}. Código:  {ex.ErrorCode}");
                            return false;
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
        public async Task<bool> NuevoDocumentoAsync(Documento doc)
        {
            string consulta = "INSERT INTO CONTENIDOS_EN_EL_CV " +
                "(RutaArchivo, TipoMime, FK_CODIGO_INF, FK_CODIGO_EMP_C) VALUES (@Ruta, @Tipo, @Informe, @Emp)";
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
                        command.Parameters.AddWithValue("@Emp", $"{empActual.getDni()}");
                        try
                        {
                            a = command.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            if(ex.ErrorCode == -2146232060)
                            {
                                MessageBox.Show($"Este empleado ya tiene ese documento añadido en esa fecha. Elige otro. ");
                                return false;
                            }
                            else
                            {
                                MessageBox.Show($"Error al registrar el documento: {ex.Message}, error:  {ex.ErrorCode}");
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
        //EDITAR DOCUMENTO
        public async Task<bool> EditarDocumentoAsync(Documento doc, string rutaAntigua)
        {
            string consulta = "UPDATE CONTENIDOS_EN_EL_CV " +
                "SET RutaArchivo = @Ruta WHERE (FK_CODIGO_INF = @ForaneaInf and RutaArchivo = @RutaAntigua)";
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
                        command.Parameters.AddWithValue("@RutaAntigua", $"{rutaAntigua}");
                        command.Parameters.AddWithValue("@ForaneaInf", $"{doc.FK_CODIGO_INF}");
                        try
                        {
                            a = command.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show($"Error al registrar el informe: {ex.Message}, error:  {ex.ErrorCode}");
                            return false;
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

        //BORRAR DOCUMENTO
        private async Task<bool> BorrarDocumentoAsync(Documento doc)
        {
            string consulta = "DELETE FROM CONTENIDOS_EN_EL_CV " +
                            "WHERE (FK_CODIGO_INF = @ForaneaInf and RutaArchivo = @id)";
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(consulta, connection))
                    {
                        // Parámetro para evitar inyección SQL
                        int a = 0;
                        command.Parameters.AddWithValue("@ForaneaInf", $"{InformeMostrar.getFechaInf()}");
                        command.Parameters.AddWithValue("@id", $"{doc.RutaArchivo}");
                        try
                        {
                            a = command.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show($"Error al borrar el documento: {ex.Message}. Código:  {ex.ErrorCode}");
                            return false;
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
                MessageBox.Show($"Error al borrar el informe: {ex.Message}");
                return false;
            }
        }



        //NUEVO CONCEPTO
        public async Task<bool> NuevoConceptoAsync(Concepto con)
        {
            string consulta = "INSERT INTO CONCEPTOS_PARA_CV " +
                "(FK_CODIGO_INF, FK_CODIGO_EMP_C, NOMBRE, DESCRIPCION) VALUES (@Codigo, @CodEmp, @Nombre, @Desc)";
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(consulta, connection))
                    {
                        // Parámetro para evitar inyección SQL
                        int a = 0;
                        command.Parameters.AddWithValue("@Codigo", $"{con.getCodigo()}");
                        command.Parameters.AddWithValue("@CodEmp", $"{empActual.getDni()}");
                        command.Parameters.AddWithValue("@Nombre", $"{con.getNombre()}");
                        command.Parameters.AddWithValue("@Desc", $"{con.getDescripcion()}");
                        try
                        {
                            a = command.ExecuteNonQuery();
                            
                        }
                        catch (SqlException ex)
                        {
                            if (ex.ErrorCode == -2146232060)
                            {
                                MessageBox.Show("Ya existe otro concepto igual. ");
                                return false;
                            }
                            else
                            {
                                MessageBox.Show($"Error al registrar el concepto: {ex.Message}, error:  {ex.ErrorCode}");
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

        //EDITAR CONCEPTO
        public async Task<bool> EditarConceptoAsync(Concepto doc, string nombreViejo)
        {
            string consulta = "UPDATE CONCEPTOS_PARA_CV " +
                "SET Nombre = @Nombre, DESCRIPCION = @Desc WHERE (FK_CODIGO_INF = @ForaneaInf and Nombre = @NombreViejo)";
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(consulta, connection))
                    {
                        // Parámetro para evitar inyección SQL
                        int a = 0;
                        command.Parameters.AddWithValue("@Nombre", $"{doc.Nombre}");
                        command.Parameters.AddWithValue("@Desc", $"{doc.Descripcion}");
                        command.Parameters.AddWithValue("@ForaneaInf", $"{doc.FK_CODIGO_INF}");
                        command.Parameters.AddWithValue("@NombreViejo", $"{nombreViejo}");
                        try
                        {
                            a = command.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            if (ex.ErrorCode == -2146232060)
                            {
                                MessageBox.Show("Ya existe otro concepto igual. ");
                                return false;
                            }
                            else
                            {
                                MessageBox.Show($"Error al editar el concepto: {ex.Message}, error:  {ex.ErrorCode}");
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

        //BORRAR CONCEPTO
        private async Task<bool> BorrarConceptoAsync(Concepto doc)
        {
            string consulta = "DELETE FROM CONCEPTOS_PARA_CV " +
                            "WHERE (FK_CODIGO_INF = @ForaneaInf and Nombre = @id)";
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(consulta, connection))
                    {
                        // Parámetro para evitar inyección SQL
                        int a = 0;
                        command.Parameters.AddWithValue("@ForaneaInf", $"{InformeMostrar.getFechaInf()}");
                        command.Parameters.AddWithValue("@id", $"{doc.Nombre}");
                        try
                        {
                            a = command.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show($"Error al borrar el concepto: {ex.Message}. Código:  {ex.ErrorCode}");
                            return false;
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
                MessageBox.Show($"Error al borrar el informe: {ex.Message}");
                return false;
            }
        }



        private void tbDescripcionGuardar(object sender, RoutedEventArgs e)
        {
            if (InformeMostrar != null)
            {
                if (lbConceptos.SelectedItems.Count != 0)
                {
                    Concepto nuevaDesc = new Concepto(conceptoActual.getNombre(), tbDescripcionConcepto.Text, conceptoActual.getCodigo());
                    Conceptos.Remove(conceptoActual);
                    Conceptos.Add(nuevaDesc);
                    _ = EditarConceptoAsync(nuevaDesc, nuevaDesc.getNombre());
                }
            }


        }


        private void escribirInforme(Informe informe)
        {
            string resultado = "El empleado tiene ";
            Direccion = informe.FECHA.ToString();


        }
    }
}