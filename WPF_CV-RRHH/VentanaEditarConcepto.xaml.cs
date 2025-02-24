﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPF_CV_RRHH
{
    /// <summary>
    /// Lógica de interacción para VentanaEditarConcepto.xaml
    /// </summary>
    public partial class VentanaEditarConcepto : Window
    {
        public VentanaEditarConcepto()
        {
            Resultado = "";
            InitializeComponent();
            txConcepto.Focus();
        }

        public string Resultado { get; private set; }


        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            Resultado = txConcepto.Text; // Capturar dato del TextBox
            this.DialogResult = true; // Cierra la ventana con "Aceptar"
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Cierra la ventana con "Cancelar"
        }
    }
}
