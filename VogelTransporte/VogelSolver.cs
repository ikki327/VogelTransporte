/*
 * =====================================================================
 *  Proyecto : Método de Transporte – Aproximación de Vogel (VAM)
 *  Archivo  : VogelSolver.cs
 *  Autor    : Salgado Tirado Luis Gerardo
 *  No. Ctrl : 25213085
 *  Carrera  : Ingeniería en Sistemas Computacionales
 *  Materia  : Investigación de Operaciones – Grupo SC3D (8:00 a 9:00)
 *  Docente  : Sergio Armando Bueno Martínez
 *  Periodo  : Semestre 3, Agosto–Diciembre 2026
 *  Escuela  : Instituto Tecnológico de Tijuana
 *
 *  Descripción: Implementación del algoritmo de Vogel (balanceo, penalizaciones, asignación y costo total).
 * =====================================================================
 */

using System.Text;

namespace VogelTransporte
{
    /// <summary>
    /// Resultado del método de Aproximación de Vogel.
    /// </summary>
    public class ResultadoVogel
    {
        public double[,] Costos;          // Matriz de costos (ya balanceada)
        public double[,] Asignaciones;    // Cantidad enviada de cada origen a cada destino
        public bool[,] EsBasica;          // true si la celda recibió asignación (aunque sea 0)
        public double[] Oferta;           // Oferta original (balanceada)
        public double[] Demanda;          // Demanda original (balanceada)
        public string[] NombresOrigenes; // Nombres de los orígenes (balanceados)
        public string[] NombresDestinos; // Nombres de los destinos (balanceados)
        public double CostoTotal; // Costo total de la solución obtenida
        public string TipoBalanceo;       // Mensaje sobre el balanceo aplicado
        public string Procedimiento;      // Bitácora paso a paso
    }

    /// <summary>
    /// Implementación del Método de Aproximación de Vogel (VAM)
    /// para obtener una solución básica factible inicial del problema de transporte.
    /// </summary>
    public static class VogelSolver // Clase estática para resolver el problema de transporte usando el método de Vogel
    {
        public static ResultadoVogel Resolver(double[,] costosOriginales, double[] ofertaOriginal,
                                              double[] demandaOriginal, string[] nombresO, string[] nombresD)
        {
            // Dimensiones originales
            int m = costosOriginales.GetLength(0);// Número de filas (orígenes)
            int n = costosOriginales.GetLength(1); // Número de columnas (destinos)

            // ---------- PASO 0: Balancear el problema ----------
            double totalOferta = ofertaOriginal.Sum();
            double totalDemanda = demandaOriginal.Sum();
            string tipoBalanceo;

            if (Math.Abs(totalOferta - totalDemanda) < 1e-9)
            {
                // Problema balanceado: no se hace nada
                tipoBalanceo = $"Problema balanceado (oferta = demanda = {totalOferta})."; 
            }
            else if (totalOferta > totalDemanda)
            {
                // Sobra oferta: se agrega un DESTINO ficticio con costo 0
                n++;// Se incrementa el número de columnas
                tipoBalanceo = $"Oferta ({totalOferta}) > Demanda ({totalDemanda}): se agregó un destino ficticio con demanda {totalOferta - totalDemanda} y costo 0.";
            }
            else
            {
                // Falta oferta: se agrega un ORIGEN ficticio con costo 0
                m++;// Se incrementa el número de filas
                tipoBalanceo = $"Demanda ({totalDemanda}) > Oferta ({totalOferta}): se agregó un origen ficticio con oferta {totalDemanda - totalOferta} y costo 0.";
            }

            // Matrices balanceadas
            double[,] c = new double[m, n];
            double[] oferta = new double[m]; 
            double[] demanda = new double[n];
            string[] nomO = new string[m];
            string[] nomD = new string[n];

            // Llenar las matrices balanceadas con los datos originales y los ficticios
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    // Si la fila/columna es ficticia, el costo es 0; si no, se copia el costo original
                    bool dentro = i < costosOriginales.GetLength(0) && j < costosOriginales.GetLength(1);
                    c[i, j] = dentro ? costosOriginales[i, j] : 0; // fila/columna ficticia = costo 0
                    //La línea con ? : se llama operador ternario. Es un if compacto:
                }
                oferta[i] = i < ofertaOriginal.Length ? ofertaOriginal[i] : totalDemanda - totalOferta;
                nomO[i] = i < nombresO.Length ? nombresO[i] : "Ficticio";
            }
            for (int j = 0; j < n; j++)
            {
                // Si la fila/columna es ficticia, la demanda es la diferencia; si no, se copia la demanda original
                demanda[j] = j < demandaOriginal.Length ? demandaOriginal[j] : totalOferta - totalDemanda;
                nomD[j] = j < nombresD.Length ? nombresD[j] : "Ficticio";
            }

            // Copias de trabajo (se van reduciendo conforme se asigna)
            double[] ofertaRest = (double[])oferta.Clone();
            double[] demandaRest = (double[])demanda.Clone();
            bool[] filaActiva = Enumerable.Repeat(true, m).ToArray();
            bool[] colActiva = Enumerable.Repeat(true, n).ToArray();

            double[,] x = new double[m, n];
            bool[,] basica = new bool[m, n];

            StringBuilder log = new StringBuilder();
            log.AppendLine(tipoBalanceo);
            log.AppendLine();

            int iteracion = 1;

            // ---------- Ciclo principal ----------
            while (filaActiva.Any(a => a) && colActiva.Any(a => a))
            {
                log.AppendLine($"===== Iteración {iteracion} =====");

                // PASO 1: Penalización de cada fila = diferencia entre sus dos costos más pequeños
                double[] penFila = new double[m];
                for (int i = 0; i < m; i++)
                {
                    if (!filaActiva[i]) continue;
                    var costosFila = new List<double>();
                    for (int j = 0; j < n; j++)
                        if (colActiva[j]) costosFila.Add(c[i, j]);
                    penFila[i] = Penalizacion(costosFila);
                    log.AppendLine($"  Penalización fila {nomO[i]}: {penFila[i]}");
                }

                // PASO 1 (cont.): Penalización de cada columna
                double[] penCol = new double[n];
                for (int j = 0; j < n; j++)
                {
                    if (!colActiva[j]) continue;
                    var costosCol = new List<double>();
                    for (int i = 0; i < m; i++)
                        if (filaActiva[i]) costosCol.Add(c[i, j]);
                    penCol[j] = Penalizacion(costosCol);
                    log.AppendLine($"  Penalización columna {nomD[j]}: {penCol[j]}");
                }

                // PASO 2: Elegir la fila o columna con la MAYOR penalización.
                // Empate: se elige la que contenga el costo más pequeño.
                double mejorPen = -1, mejorCosto = double.MaxValue;
                int selI = -1, selJ = -1;
                string elegido = "";

                for (int i = 0; i < m; i++)
                {
                    if (!filaActiva[i]) continue;
                    int jMin = MinEnFila(c, i, colActiva);
                    if (EsMejor(penFila[i], c[i, jMin], mejorPen, mejorCosto))
                    {
                        mejorPen = penFila[i]; mejorCosto = c[i, jMin];
                        selI = i; selJ = jMin;
                        elegido = $"fila {nomO[i]}";
                    }
                }
                for (int j = 0; j < n; j++)
                {
                    if (!colActiva[j]) continue;
                    int iMin = MinEnColumna(c, j, filaActiva);
                    if (EsMejor(penCol[j], c[iMin, j], mejorPen, mejorCosto))
                    {
                        mejorPen = penCol[j]; mejorCosto = c[iMin, j];
                        selI = iMin; selJ = j;
                        elegido = $"columna {nomD[j]}";
                    }
                }

                // PASO 3: Asignar lo máximo posible a la celda de menor costo
                double cantidad = Math.Min(ofertaRest[selI], demandaRest[selJ]);
                x[selI, selJ] += cantidad;
                basica[selI, selJ] = true;
                ofertaRest[selI] -= cantidad;
                demandaRest[selJ] -= cantidad;

                log.AppendLine($"  Mayor penalización: {mejorPen} en la {elegido}.");
                log.AppendLine($"  Celda de menor costo: ({nomO[selI]}, {nomD[selJ]}) con costo {c[selI, selJ]}.");
                log.AppendLine($"  Se asignan {cantidad} unidades.");

                // PASO 4: Tachar la fila o columna que quedó satisfecha.
                // Si ambas llegan a 0 se tacha solo una (evita perder una variable básica).
                bool ultimaFila = filaActiva.Count(a => a) == 1;
                bool ultimaCol = colActiva.Count(a => a) == 1;

                if (ofertaRest[selI] <= 1e-9 && demandaRest[selJ] <= 1e-9)
                {
                    if (ultimaFila && ultimaCol)
                    {
                        filaActiva[selI] = false; colActiva[selJ] = false;
                        log.AppendLine($"  Se tachan la fila {nomO[selI]} y la columna {nomD[selJ]}.");
                    }
                    else if (ultimaCol)
                    {
                        filaActiva[selI] = false;
                        log.AppendLine($"  Se tacha la fila {nomO[selI]} (la columna queda con demanda 0).");
                    }
                    else
                    {
                        colActiva[selJ] = false;
                        log.AppendLine($"  Se tacha la columna {nomD[selJ]} (la fila queda con oferta 0).");
                    }
                }
                else if (ofertaRest[selI] <= 1e-9)
                {
                    filaActiva[selI] = false;
                    log.AppendLine($"  Se tacha la fila {nomO[selI]} (oferta agotada).");
                }
                else
                {
                    colActiva[selJ] = false;
                    log.AppendLine($"  Se tacha la columna {nomD[selJ]} (demanda satisfecha).");
                }

                log.AppendLine();
                iteracion++;
            }

            // ---------- Costo total Z = Σ c(i,j) * x(i,j) ---------- 
            // Se suman solo las celdas con asignación positiva
            double z = 0;
            var terminos = new List<string>();
            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    if (x[i, j] > 0)
                    {
                        z += c[i, j] * x[i, j];
                        terminos.Add($"({c[i, j]})({x[i, j]})");
                    }

            log.AppendLine("===== Costo total =====");
            log.AppendLine($"  Z = {string.Join(" + ", terminos)}");
            z = Math.Round(z, 6);// Se redondea a 6 decimales para evitar problemas de precisión
            log.AppendLine($"  Z = {z}");

            // Fin del procedimiento
            return new ResultadoVogel
            {
                // Se devuelven los resultados
                Costos = c,
                Asignaciones = x,
                EsBasica = basica,
                Oferta = oferta,
                Demanda = demanda,
                NombresOrigenes = nomO,
                NombresDestinos = nomD,
                CostoTotal = z,
                TipoBalanceo = tipoBalanceo,
                Procedimiento = log.ToString()
            };
        }

        /// <summary>Diferencia entre los dos costos menores. Si solo queda uno, la penalización es ese costo.</summary>
        private static double Penalizacion(List<double> costos)
        {
            // Calcula la penalización como la diferencia entre los dos costos más pequeños de la lista
            if (costos.Count == 0) return 0;
            if (costos.Count == 1) return costos[0];
            var orden = costos.OrderBy(v => v).ToList();
            //return orden[1] - orden[0]; Lo que salía mal eran las penalizaciones mostradas con residuos como 0.05500000000000005, y en un caso de empate esos residuos
            //podrían hacer que el método eligiera mal.
            return Math.Round(orden[1] - orden[0], 9);// Se redondea a 9 decimales para evitar problemas de precisión
        }

        private static int MinEnFila(double[,] c, int i, bool[] colActiva)
        {
            // Encuentra el índice de la columna con el costo mínimo en la fila i, considerando solo las columnas activas
            int mejor = -1;
            for (int j = 0; j < c.GetLength(1); j++)
                if (colActiva[j] && (mejor == -1 || c[i, j] < c[i, mejor])) mejor = j;
            return mejor;
        }

        private static int MinEnColumna(double[,] c, int j, bool[] filaActiva)
        {
            // Encuentra el índice de la fila con el costo mínimo en la columna j, considerando solo las filas activas
            int mejor = -1;
            for (int i = 0; i < c.GetLength(0); i++)
                if (filaActiva[i] && (mejor == -1 || c[i, j] < c[mejor, j])) mejor = i;
            return mejor;
        }

        /// <summary>Mayor penalización gana; en empate gana la de menor costo.</summary>
        private static bool EsMejor(double pen, double costo, double mejorPen, double mejorCosto)
        {
            // Comparación con tolerancia para evitar problemas de precisión
            if (pen > mejorPen + 1e-9) return true;
            if (Math.Abs(pen - mejorPen) <= 1e-9 && costo < mejorCosto) return true;
            return false;
        }
    }
}
