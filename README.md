# Método de Aproximación de Vogel (VAM) en C#

Programa de escritorio que resuelve **problemas de transporte** con el **Método de Aproximación de Vogel**, obteniendo una solución básica factible inicial y su costo total. Muestra paso a paso las penalizaciones de cada iteración, por lo que también sirve para estudiar el método.

Proyecto individual de la asignatura **Investigación de Operaciones**, tema 2.5 Programación de proyectos (PERT/CPM).

---

## Funciones

- Captura de cualquier problema de **1 a 15 orígenes y destinos** (costos, oferta y demanda).
- **Balanceo automático**: si la oferta y la demanda no coinciden, agrega un origen o destino ficticio con costo 0.
- **Procedimiento paso a paso**: penalizaciones de filas y columnas, celda elegida, cantidad asignada y fila o columna tachada en cada iteración.
- **Tabla de solución** con las celdas asignadas resaltadas y el **costo total Z**.
- **Exportar** el resultado completo a un archivo `.txt`.
- Acepta costos enteros o con decimales (punto o coma).

## Cómo usarlo

### Opción 1: ejecutable (sin Visual Studio)
1. Ve a la sección **[Releases](../../releases)** de este repositorio.
2. Descarga `VogelTransporte.exe` de la versión más reciente.
3. Ábrelo con doble clic. Si Windows muestra un aviso de SmartScreen, elige **Más información → Ejecutar de todas formas** (el programa no está firmado digitalmente).

### Opción 2: desde el código fuente
1. Requisitos: **Visual Studio 2022** con la carga de trabajo *Desarrollo de escritorio de .NET* (.NET 8).
2. Clic en **Code → Download ZIP** y descomprime, o **Code → Open with Visual Studio**.
3. Abre `VogelTransporte.slnx` y presiona **F5**.

### Uso del programa
1. Indica el número de **orígenes** y **destinos** y presiona **Generar tabla**.
2. Captura los costos, la oferta (última columna) y la demanda (última fila).
3. Presiona **Resolver**.
4. Opcional: **Exportar** para guardar el resultado, o **Cargar ejemplo** para probar con un problema de muestra.

## Estructura del código

| Archivo | Descripción |
|---|---|
| `VogelSolver.cs` | Algoritmo de Vogel: balanceo, penalizaciones, selección, asignación, tachado y costo total. No depende de la interfaz. |
| `FormPrincipal.cs` | Interfaz gráfica (Windows Forms): captura, validación, resultados, exportación y ventana «Acerca de». |
| `DatosAutor.cs` | Datos del autor usados en el título, pie de página, «Acerca de» y archivo exportado. |
| `Program.cs` | Punto de entrada; abre la ventana principal. |

## Ejemplo

Problema de 3 orígenes y 4 destinos (Taha):

| | D1 | D2 | D3 | D4 | Oferta |
|---|---|---|---|---|---|
| O1 | 10 | 2 | 20 | 11 | 15 |
| O2 | 12 | 7 | 9 | 20 | 25 |
| O3 | 4 | 14 | 16 | 18 | 10 |
| Demanda | 5 | 15 | 15 | 15 | |

Resultado: x₁₂ = 15, x₁₄ = 0, x₂₃ = 15, x₂₄ = 10, x₃₁ = 5, x₃₄ = 5 → **Z = 475**.

## Autor

**Salgado Tirado Luis Gerardo**
Ingeniería en Sistemas Computacionales · Instituto Tecnológico de Tijuana
Investigación de Operaciones · Grupo SC3D · Agosto–Diciembre 2026
Docente: Sergio Armando Bueno Martínez

## Referencias

- Taha, H. A. (2017). *Investigación de operaciones* (10.ª ed.). Pearson.
- Hillier, F. S., y Lieberman, G. J. (2015). *Introducción a la investigación de operaciones* (10.ª ed.). McGraw-Hill.
