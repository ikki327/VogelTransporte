/*
 * =====================================================================
 *  Proyecto : Método de Transporte – Aproximación de Vogel (VAM)
 *  Archivo  : Program.cs
 *  Autor    : Salgado Tirado Luis Gerardo
 *  No. Ctrl : 25213085
 *  Carrera  : Ingeniería en Sistemas Computacionales
 *  Materia  : Investigación de Operaciones – Grupo SC3D (8:00 a 9:00)
 *  Docente  : Sergio Armando Bueno Martínez
 *  Periodo  : Semestre 3, Agosto–Diciembre 2026
 *  Escuela  : Instituto Tecnológico de Tijuana
 *
 *  Descripción: Punto de entrada de la aplicación; abre la ventana principal.
 * =====================================================================
 */

namespace VogelTransporte
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new FormPrincipal());
        }
    }
}
