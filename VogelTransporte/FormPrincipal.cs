/*
 * =====================================================================
 *  Proyecto : Método de Transporte – Aproximación de Vogel (VAM)
 *  Archivo  : FormPrincipal.cs
 *  Autor    : Salgado Tirado Luis Gerardo
 *  No. Ctrl : 25213085
 *  Carrera  : Ingeniería en Sistemas Computacionales
 *  Materia  : Investigación de Operaciones – Grupo SC3D (8:00 a 9:00)
 *  Docente  : Sergio Armando Bueno Martínez
 *  Periodo  : Semestre 3, Agosto–Diciembre 2026
 *  Escuela  : Instituto Tecnológico de Tijuana
 *
 *  Descripción: Interfaz gráfica: captura de datos, resolución, resultados, exportación y "Acerca de".
 * =====================================================================
 */

using System.Globalization;

namespace VogelTransporte
{
    /// <summary>
    /// Ventana principal: captura de datos, resolución por Vogel y presentación de resultados.
    /// La interfaz se construye por código para que el proyecto no dependa del diseñador.
    /// </summary>
    public class FormPrincipal : Form
    {
        // ---- Controles ----
        private NumericUpDown nudOrigenes, nudDestinos;
        private Button btnGenerar, btnEjemplo, btnResolver, btnLimpiar, btnExportar, btnAcerca;
        private DataGridView dgvDatos, dgvResultado;
        private TextBox txtPasos;
        private Label lblCosto, lblBalanceo;

        // Último resultado calculado (se usa para exportar)
        private ResultadoVogel ultimoResultado;

        // Colores de la interfaz
        private readonly Color colorPrincipal = Color.FromArgb(31, 78, 121);
        private readonly Color colorTotales = Color.FromArgb(226, 239, 218);
        private readonly Color colorAsignada = Color.FromArgb(255, 230, 153);

        public FormPrincipal()
        {
            // Configuración de la ventana
            Text = $"Método de Vogel – {DatosAutor.Nombre} – {DatosAutor.Materia}";
            Size = new Size(1150, 780);
            MinimumSize = new Size(900, 650);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10F);
            BackColor = Color.White;

            ConstruirInterfaz();
            GenerarTabla();
        }

        // =====================================================================
        //  Construcción de la interfaz
        // =====================================================================
        private void ConstruirInterfaz()
        {
            // Encabezado
            var lblTitulo = new Label
            {
                Text = "Método de Aproximación de Vogel (VAM)",
                Dock = DockStyle.Top,
                Height = 50,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = colorPrincipal,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Barra de controles
            var barra = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(10, 10, 10, 5),
                WrapContents = false
            };

            nudOrigenes = new NumericUpDown { Minimum = 1, Maximum = 15, Value = 3, Width = 60 };
            nudDestinos = new NumericUpDown { Minimum = 1, Maximum = 15, Value = 4, Width = 60 };

            btnGenerar = CrearBoton("Generar tabla", colorPrincipal);
            btnEjemplo = CrearBoton("Cargar ejemplo", Color.FromArgb(84, 130, 53));
            btnResolver = CrearBoton("Resolver", Color.FromArgb(192, 80, 0));
            btnLimpiar = CrearBoton("Limpiar", Color.Gray);

            btnGenerar.Click += (s, e) => GenerarTabla();
            btnEjemplo.Click += (s, e) => CargarEjemplo();
            btnResolver.Click += (s, e) => Resolver();
            btnLimpiar.Click += (s, e) => { GenerarTabla(); };

            btnExportar = CrearBoton("Exportar", Color.FromArgb(112, 48, 160));
            btnAcerca = CrearBoton("Acerca de", Color.FromArgb(64, 64, 64));
            btnExportar.Enabled = false; // se habilita al resolver
            btnExportar.Click += (s, e) => Exportar();
            btnAcerca.Click += (s, e) => MostrarAcercaDe();

            barra.Controls.Add(CrearEtiqueta("Orígenes:"));
            barra.Controls.Add(nudOrigenes);
            barra.Controls.Add(CrearEtiqueta("   Destinos:"));
            barra.Controls.Add(nudDestinos);
            barra.Controls.Add(new Label { Width = 20 });
            barra.Controls.Add(btnGenerar);
            barra.Controls.Add(btnEjemplo);
            barra.Controls.Add(btnResolver);
            barra.Controls.Add(btnLimpiar);
            barra.Controls.Add(btnExportar);
            barra.Controls.Add(btnAcerca);

            // Pie de página con los datos del autor
            var lblPie = new Label
            {
                Text = $"Elaborado por: {DatosAutor.Nombre}   |   No. de control: {DatosAutor.NumeroControl}   |   " +
                       $"{DatosAutor.Carrera}   |   {DatosAutor.Materia}, Grupo {DatosAutor.Grupo}   |   {DatosAutor.Escuela}",
                Dock = DockStyle.Bottom,
                Height = 30,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.White,
                BackColor = colorPrincipal,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Contenedor principal: datos arriba, resultados abajo
            var principal = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(10)
            };
            principal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            principal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            principal.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            principal.RowStyles.Add(new RowStyle(SizeType.Percent, 55));

            // Tabla de captura
            dgvDatos = CrearTabla(editable: true);
            var grpDatos = CrearGrupo("1. Captura de costos, oferta y demanda", dgvDatos);

            // Procedimiento paso a paso
            txtPasos = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9.5F),
                BackColor = Color.FromArgb(248, 248, 248)
            };
            var grpPasos = CrearGrupo("3. Procedimiento (penalizaciones e iteraciones)", txtPasos);

            // Tabla de resultados + costo total
            dgvResultado = CrearTabla(editable: false);
            lblBalanceo = new Label { Dock = DockStyle.Bottom, Height = 45, ForeColor = Color.DimGray };
            lblCosto = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = colorPrincipal,
                Text = "Costo total Z = —"
            };
            var panelRes = new Panel { Dock = DockStyle.Fill };
            panelRes.Controls.Add(dgvResultado);
            panelRes.Controls.Add(lblBalanceo);
            panelRes.Controls.Add(lblCosto);
            var grpRes = CrearGrupo("2. Solución inicial (asignación [costo unitario])", panelRes);

            principal.Controls.Add(grpDatos, 0, 0);
            principal.Controls.Add(grpPasos, 1, 0);
            principal.SetRowSpan(grpPasos, 2);
            principal.Controls.Add(grpRes, 0, 1);

            Controls.Add(principal);
            Controls.Add(lblPie);
            Controls.Add(barra);
            Controls.Add(lblTitulo);
        }

        private Button CrearBoton(string texto, Color color) => new Button
        {
            Text = texto,
            AutoSize = true,
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(5, 0, 5, 0),
            Cursor = Cursors.Hand
        };

        private Label CrearEtiqueta(string texto) => new Label
        {
            Text = texto,
            AutoSize = true,
            Margin = new Padding(0, 6, 3, 0)
        };

        private GroupBox CrearGrupo(string titulo, Control contenido)
        {
            var grp = new GroupBox
            {
                Text = titulo,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = colorPrincipal,
                Padding = new Padding(8)
            };
            contenido.Font = contenido is TextBox ? contenido.Font : new Font("Segoe UI", 10F);
            contenido.ForeColor = Color.Black;
            grp.Controls.Add(contenido);
            return grp;
        }

        private DataGridView CrearTabla(bool editable)
        {
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = !editable,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                BackgroundColor = Color.White,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
            };
            dgv.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = colorPrincipal;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            return dgv;
        }

        // =====================================================================
        //  Tabla de captura
        // =====================================================================

        /// <summary>
        /// Crea la tabla vacía: una fila por origen + fila "Demanda";
        /// una columna por destino + columna "Oferta".
        /// </summary>
        private void GenerarTabla()
        {
            int m = (int)nudOrigenes.Value;
            int n = (int)nudDestinos.Value;

            dgvDatos.Columns.Clear();
            dgvDatos.Rows.Clear();

            dgvDatos.Columns.Add("Origen", "Origen / Destino");
            for (int j = 0; j < n; j++)
                dgvDatos.Columns.Add($"D{j + 1}", $"D{j + 1}");
            dgvDatos.Columns.Add("Oferta", "Oferta");

            for (int i = 0; i < m; i++)
                dgvDatos.Rows.Add($"O{i + 1}");

            int filaDem = dgvDatos.Rows.Add("Demanda");
            var filaDemanda = dgvDatos.Rows[filaDem];
            filaDemanda.DefaultCellStyle.BackColor = colorTotales;
            filaDemanda.Cells[0].ReadOnly = true;
            filaDemanda.Cells[n + 1].ReadOnly = true;
            filaDemanda.Cells[n + 1].Style.BackColor = Color.LightGray;

            dgvDatos.Columns[0].DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvDatos.Columns[n + 1].DefaultCellStyle.BackColor = colorTotales;

            LimpiarResultados();
        }

        /// <summary>Ejemplo clásico (Taha): 3 orígenes, 4 destinos. Resultado esperado por Vogel: Z = 475.</summary>
        private void CargarEjemplo()
        {
            nudOrigenes.Value = 3;
            nudDestinos.Value = 4;
            GenerarTabla();

            double[,] costos = { { 10, 2, 20, 11 }, { 12, 7, 9, 20 }, { 4, 14, 16, 18 } };
            double[] oferta = { 15, 25, 10 };
            double[] demanda = { 5, 15, 15, 15 };

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 4; j++)
                    dgvDatos.Rows[i].Cells[j + 1].Value = costos[i, j];
                dgvDatos.Rows[i].Cells[5].Value = oferta[i];
            }
            for (int j = 0; j < 4; j++)
                dgvDatos.Rows[3].Cells[j + 1].Value = demanda[j];
        }

        private void LimpiarResultados()
        {
            dgvResultado.Columns.Clear();
            dgvResultado.Rows.Clear();
            txtPasos.Clear();
            lblCosto.Text = "Costo total Z = —";
            lblBalanceo.Text = "";
            ultimoResultado = null;
            btnExportar.Enabled = false;
        }

        // =====================================================================
        //  Lectura de datos y resolución
        // =====================================================================
        private void Resolver()
        {
            dgvDatos.EndEdit();
            int m = dgvDatos.Rows.Count - 1;      // sin la fila "Demanda"
            int n = dgvDatos.Columns.Count - 2;   // sin "Origen" ni "Oferta"

            double[,] costos = new double[m, n];
            double[] oferta = new double[m];
            double[] demanda = new double[n];
            string[] nombresO = new string[m];
            string[] nombresD = new string[n];

            try
            {
                for (int i = 0; i < m; i++)
                {
                    nombresO[i] = Convert.ToString(dgvDatos.Rows[i].Cells[0].Value)?.Trim();
                    if (string.IsNullOrEmpty(nombresO[i])) nombresO[i] = $"O{i + 1}";

                    for (int j = 0; j < n; j++)
                        costos[i, j] = LeerNumero(i, j + 1, $"costo ({nombresO[i]}, D{j + 1})");

                    oferta[i] = LeerNumero(i, n + 1, $"oferta de {nombresO[i]}");
                }
                for (int j = 0; j < n; j++)
                {
                    nombresD[j] = dgvDatos.Columns[j + 1].HeaderText;
                    demanda[j] = LeerNumero(m, j + 1, $"demanda de {nombresD[j]}");
                }
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message, "Dato inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ResultadoVogel r = VogelSolver.Resolver(costos, oferta, demanda, nombresO, nombresD);
            MostrarResultado(r);
        }

        /// <summary>Lee una celda numérica (acepta punto o coma decimal) y valida que no sea negativa.</summary>
        private double LeerNumero(int fila, int col, string descripcion)
        {
            string texto = Convert.ToString(dgvDatos.Rows[fila].Cells[col].Value)?.Trim().Replace(',', '.');
            if (string.IsNullOrEmpty(texto) ||
                !double.TryParse(texto, NumberStyles.Float, CultureInfo.InvariantCulture, out double valor))
            {
                dgvDatos.CurrentCell = dgvDatos.Rows[fila].Cells[col];
                throw new FormatException($"Falta o no es válido el valor de: {descripcion}.");
            }
            if (valor < 0)
            {
                dgvDatos.CurrentCell = dgvDatos.Rows[fila].Cells[col];
                throw new FormatException($"El valor de {descripcion} no puede ser negativo.");
            }
            return valor;
        }

        // =====================================================================
        //  Presentación de resultados
        // =====================================================================
        private void MostrarResultado(ResultadoVogel r)
        {
            int m = r.Costos.GetLength(0);
            int n = r.Costos.GetLength(1);

            dgvResultado.Columns.Clear();
            dgvResultado.Rows.Clear();

            dgvResultado.Columns.Add("Origen", "Origen / Destino");
            for (int j = 0; j < n; j++)
                dgvResultado.Columns.Add($"R{j}", r.NombresDestinos[j]);
            dgvResultado.Columns.Add("Oferta", "Oferta");

            for (int i = 0; i < m; i++)
            {
                var fila = new object[n + 2];
                fila[0] = r.NombresOrigenes[i];
                for (int j = 0; j < n; j++)
                    fila[j + 1] = r.EsBasica[i, j]
                        ? $"{r.Asignaciones[i, j]}  [{r.Costos[i, j]}]"
                        : $"[{r.Costos[i, j]}]";
                fila[n + 1] = r.Oferta[i];
                int idx = dgvResultado.Rows.Add(fila);

                // Resaltar celdas con asignación
                for (int j = 0; j < n; j++)
                    if (r.EsBasica[i, j])
                    {
                        dgvResultado.Rows[idx].Cells[j + 1].Style.BackColor = colorAsignada;
                        dgvResultado.Rows[idx].Cells[j + 1].Style.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                    }
                    else
                    {
                        dgvResultado.Rows[idx].Cells[j + 1].Style.ForeColor = Color.Gray;
                    }
            }

            var filaDem = new object[n + 2];
            filaDem[0] = "Demanda";
            for (int j = 0; j < n; j++) filaDem[j + 1] = r.Demanda[j];
            int d = dgvResultado.Rows.Add(filaDem);
            dgvResultado.Rows[d].DefaultCellStyle.BackColor = colorTotales;

            dgvResultado.Columns[0].DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvResultado.Columns[n + 1].DefaultCellStyle.BackColor = colorTotales;

            lblCosto.Text = $"Costo total Z = {r.CostoTotal:N2}";
            lblBalanceo.Text = r.TipoBalanceo;
            txtPasos.Text = r.Procedimiento.Replace("\n", Environment.NewLine).Replace("\r\r", "\r");

            ultimoResultado = r;
            btnExportar.Enabled = true;
        }

        // =====================================================================
        //  Exportar resultado a archivo de texto
        // =====================================================================
        private void Exportar()
        {
            if (ultimoResultado == null) return;

            using var dialogo = new SaveFileDialog
            {
                Title = "Exportar resultado",
                Filter = "Archivo de texto (*.txt)|*.txt",
                FileName = $"Vogel_{DatosAutor.Nombre.Replace(' ', '_')}_{DateTime.Now:yyyy-MM-dd_HHmm}.txt"
            };
            if (dialogo.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                File.WriteAllText(dialogo.FileName, GenerarReporte(ultimoResultado), System.Text.Encoding.UTF8);
                MessageBox.Show($"Resultado exportado en:\n{dialogo.FileName}", "Exportación completa",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo guardar el archivo:\n{ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>Arma el texto del reporte: encabezado del autor, tablas, procedimiento y costo total.</summary>
        private string GenerarReporte(ResultadoVogel r)
        {
            int m = r.Costos.GetLength(0);
            int n = r.Costos.GetLength(1);
            var sb = new System.Text.StringBuilder();
            string linea = new string('=', 70);

            sb.AppendLine(linea);
            sb.AppendLine("  MÉTODO DE TRANSPORTE – APROXIMACIÓN DE VOGEL (VAM)");
            sb.AppendLine(linea);
            sb.AppendLine($"  Alumno     : {DatosAutor.Nombre}");
            sb.AppendLine($"  No. control: {DatosAutor.NumeroControl}");
            sb.AppendLine($"  Carrera    : {DatosAutor.Carrera}");
            sb.AppendLine($"  Materia    : {DatosAutor.Materia} – Grupo {DatosAutor.Grupo}");
            sb.AppendLine($"  Docente    : {DatosAutor.Docente}");
            sb.AppendLine($"  Periodo    : {DatosAutor.Periodo}");
            sb.AppendLine($"  Escuela    : {DatosAutor.Escuela}");
            sb.AppendLine($"  Fecha      : {DateTime.Now:dd/MM/yyyy HH:mm}");
            sb.AppendLine(linea);
            sb.AppendLine();

            // Tabla de costos (balanceada)
            sb.AppendLine("TABLA DE COSTOS, OFERTA Y DEMANDA");
            sb.AppendLine(FilaTexto("", r.NombresDestinos, "Oferta"));
            for (int i = 0; i < m; i++)
            {
                var celdas = new string[n];
                for (int j = 0; j < n; j++) celdas[j] = r.Costos[i, j].ToString();
                sb.AppendLine(FilaTexto(r.NombresOrigenes[i], celdas, r.Oferta[i].ToString()));
            }
            sb.AppendLine(FilaTexto("Demanda", r.Demanda.Select(d => d.ToString()).ToArray(), ""));
            sb.AppendLine();
            sb.AppendLine(r.TipoBalanceo);
            sb.AppendLine();

            // Procedimiento
            sb.AppendLine("PROCEDIMIENTO");
            sb.AppendLine(new string('-', 70));
            sb.AppendLine(r.Procedimiento);
            sb.AppendLine();

            // Tabla de asignaciones
            sb.AppendLine("SOLUCIÓN INICIAL (cantidad asignada; '-' = sin asignación)");
            sb.AppendLine(FilaTexto("", r.NombresDestinos, "Oferta"));
            for (int i = 0; i < m; i++)
            {
                var celdas = new string[n];
                for (int j = 0; j < n; j++)
                    celdas[j] = r.EsBasica[i, j] ? r.Asignaciones[i, j].ToString() : "-";
                sb.AppendLine(FilaTexto(r.NombresOrigenes[i], celdas, r.Oferta[i].ToString()));
            }
            sb.AppendLine(FilaTexto("Demanda", r.Demanda.Select(d => d.ToString()).ToArray(), ""));
            sb.AppendLine();
            sb.AppendLine(linea);
            sb.AppendLine($"  COSTO TOTAL  Z = {r.CostoTotal:N2}");
            sb.AppendLine(linea);

            return sb.ToString();
        }

        /// <summary>Da formato de columnas de ancho fijo a una fila de la tabla.</summary>
        private static string FilaTexto(string encabezado, string[] celdas, string ultima)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(encabezado.PadRight(12));
            foreach (var c in celdas) sb.Append(c.PadLeft(10));
            sb.Append(ultima.PadLeft(10));
            return sb.ToString();
        }

        // =====================================================================
        //  Ventana "Acerca de"
        // =====================================================================
        private void MostrarAcercaDe()
        {
            using var acerca = new Form
            {
                Text = "Acerca de",
                Size = new Size(520, 430),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10F)
            };

            var lblEnc = new Label
            {
                Text = "Método de Aproximación de Vogel",
                Dock = DockStyle.Top,
                Height = 50,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = colorPrincipal,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblInfo = new Label
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(25, 15, 25, 10),
                Text =
                    $"Versión {DatosAutor.Version}\n\n" +
                    $"Elaborado por:  {DatosAutor.Nombre}\n" +
                    $"No. de control:  {DatosAutor.NumeroControl}\n" +
                    $"Carrera:  {DatosAutor.Carrera}\n" +
                    $"Materia:  {DatosAutor.Materia}\n" +
                    $"Grupo:  {DatosAutor.Grupo}\n" +
                    $"Docente:  {DatosAutor.Docente}\n" +
                    $"Periodo:  {DatosAutor.Periodo}\n" +
                    $"{DatosAutor.Escuela}\n\n" +
                    "Programa que obtiene una solución básica factible inicial del problema " +
                    "de transporte mediante el Método de Aproximación de Vogel, mostrando " +
                    "las penalizaciones de cada iteración y el costo total."
            };

            var btnCerrar = new Button
            {
                Text = "Cerrar",
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = colorPrincipal,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };

            acerca.Controls.Add(lblInfo);
            acerca.Controls.Add(btnCerrar);
            acerca.Controls.Add(lblEnc);
            acerca.AcceptButton = btnCerrar;
            acerca.ShowDialog(this);
        }
    }
}
