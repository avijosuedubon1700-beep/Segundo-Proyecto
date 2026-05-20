using System;
using System.Collections.Generic;

namespace Granja_adb
{
    class Program
    {
        static void Main(string[] args)
        {
            // Forzar codificación UTF8 para asegurar la visualización de iconos en consolas antiguas
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Clear();

            Console.WriteLine("╔══════════════════════════════════════════════════════╗");
            Console.WriteLine("║        SISTEMA DE GESTIÓN AGRÍCOLA - GRANJA ADB      ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════╝\n");

            double cashInicial = Solicitudes.CapturarDecimal("» Fondos de capital inicial en caja (Q): ");
            int obreros = Solicitudes.CapturarEntero("» Cantidad de operarios de campo (empleados): ");
            double salario = Solicitudes.CapturarDecimal("» Asignación salarial mensual por operario (Q): ");
            int maxMeses = Solicitudes.CapturarEntero("» Duración de la simulación en meses: ");
            int horizontal = Solicitudes.CapturarEntero("» Filas del terreno: ");
            int vertical = Solicitudes.CapturarEntero("» Columnas del terreno: ");

            // Inicialización del motor core ADB
            SimuladorGranja motor = new SimuladorGranja(cashInicial, obreros, salario, maxMeses, horizontal, vertical);
            motor.ArrancarBuclePrincipal();
        }
    }

    // CONTROLADOR CENTRAL DE OPERACIONES CONTABLES Y AGRONÓMICAS
    public class SimuladorGranja
    {
        private double cajaEfectivo;
        private readonly double capitalSemilla;
        private readonly int totalEmpleados;
        private readonly double sueldoFijo;
        private int mesesPorSimular;
        private int mesesTranscurridos = 0;

        // Historiales acumulativos para auditoría final
        private double egresoSemillasTotales = 0;
        private double ingresoCosechasTotales = 0;

        private readonly Parcela[,] cuadriculaTerreno;
        private readonly Dictionary<string, int> almacenSemillas;
        private readonly List<Cultivo> listaVariedades;

        public SimuladorGranja(double cap, int emp, double suel, int meses, int f, int c)
        {
            cajaEfectivo = cap;
            capitalSemilla = cap;
            totalEmpleados = emp;
            sueldoFijo = suel;
            mesesPorSimular = meses;

            cuadriculaTerreno = new Parcela[f, c];
            for (int i = 0; i < f; i++)
                for (int j = 0; j < c; j++)
                    cuadriculaTerreno[i, j] = new Parcela();

            almacenSemillas = new Dictionary<string, int> {
                { "Trigo", 0 }, { "Repollo", 0 }, { "Tomate", 0 }, { "Calabaza", 0 }, { "Espárrago", 0 }
            };

            listaVariedades = new List<Cultivo> {
                new Cultivo("Trigo", 100.00, 130.00, 1),
                new Cultivo("Repollo", 180.00, 280.00, 2),
                new Cultivo("Tomate", 250.00, 450.00, 3),
                new Cultivo("Calabaza", 360.00, 220.00, 4),
                new Cultivo("Espárrago", 500.00, 1000.00, 6)
            };
        }

        public void ArrancarBuclePrincipal()
        {
            while (mesesPorSimular > 0 && cajaEfectivo > 0)
            {
                Console.Clear();
                Console.WriteLine($"┌────────────────────────────────────────────────────────┐");
                Console.WriteLine($"│ MESES RESTANTES: {mesesPorSimular,-4} │ ARCAS DISPONIBLES: Q{cajaEfectivo,-11:N2} │");
                Console.WriteLine($"└────────────────────────────────────────────────────────┘");
                Console.WriteLine(" 1. Panel de Suministros (Comprar Semillas)");
                Console.WriteLine(" 2. Labores de Siembra (Sembrar)");
                Console.WriteLine(" 3. Inspección del Terreno (Mapa de Parcelas)");
                Console.WriteLine(" 4. Siguiente Ciclo de Tiempo (Avanzar Mes)");
                Console.WriteLine(" 5. Concluir Operaciones (Salir)");
                Console.Write("\n➔ Ingrese comando operativo ADB: ");

                string command = Console.ReadLine() ?? "";
                Console.Clear();

                switch (command)
                {
                    case "1": TransaccionCompra(); break;
                    case "2": AsignarSiembra(); break;
                    case "3": RenderizarMapaYDetalle(); break;
                    case "4": EjecutarTransicionMes(); break;
                    case "5": mesesPorSimular = 0; break;
                    default:
                        Console.WriteLine("❌ Código de comando desconocido.");
                        Solicitudes.PausarGesto();
                        break;
                }
            }
            GenerarCierreContable();
        }

        private void TransaccionCompra()
        {
            Console.WriteLine("══════════════════════════════════════════════════════");
            Console.WriteLine("            COMPRA Y LOGÍSTICA DE SUMINISTROS         ");
            Console.WriteLine("══════════════════════════════════════════════════════\n");

            double costosOperativosInmediatos = totalEmpleados * sueldoFijo;
            double proyeccionFinanciera = cajaEfectivo - costosOperativosInmediatos;

            Console.WriteLine($"Fondo Actual: Q{cajaEfectivo:N2}");
            Console.WriteLine($"Reserva Obligatoria de Sueldos: Q{costosOperativosInmediatos:N2}");
            Console.WriteLine($"Margen de Seguridad: Q{proyeccionFinanciera:N2}\n");

            if (proyeccionFinanciera < 0)
            {
                Console.WriteLine("⚠️ OPERACIÓN DENEGADA: Liquidez comprometida. Retenga capital para cubrir salarios.");
                Solicitudes.PausarGesto();
                return;
            }

            for (int i = 0; i < listaVariedades.Count; i++)
            {
                var v = listaVariedades[i];
                Console.WriteLine($" [{i + 1}] {v.Especie,-11} | Costo Semilla: Q{v.CostoSemilla,-6:F2} | Maduración: {v.MesesMaduracion} mes(es)");
            }

            int index = Solicitudes.CapturarEntero("\n» Seleccione el índice de semilla (1-5) o '0' para cancelar: ") - 1;
            if (index < 0 || index >= listaVariedades.Count) return;

            Cultivo seleccion = listaVariedades[index];
            int lote = Solicitudes.CapturarEntero($"» Unidades de semilla de {seleccion.Especie} a adquirir: ");

            if (lote <= 0) return;
            double totalFactura = seleccion.CostoSemilla * lote;

            if (cajaEfectivo >= totalFactura)
            {
                cajaEfectivo -= totalFactura;
                egresoSemillasTotales += totalFactura;
                almacenSemillas[seleccion.Especie] += lote;
                Console.WriteLine($"\n📦 Factura autorizada por Caja ADB. {lote} unidades de {seleccion.Especie} al almacén.");
            }
            else
            {
                Console.WriteLine("\n❌ Operación rechazada: Fondos insuficientes en caja.");
            }
            Solicitudes.PausarGesto();
        }

        private void AsignarSiembra()
        {
            Console.WriteLine("══════════════════════════════════════════════════════");
            Console.WriteLine("                  MÓDULO DE SIEMBRAS                  ");
            Console.WriteLine("══════════════════════════════════════════════════════\n");

            Console.WriteLine("Existencias en Almacén:");
            foreach (var kp in almacenSemillas)
                Console.WriteLine($" • {kp.Key,-11}: {kp.Value} semillas disp.");

            int f = Solicitudes.CapturarEntero($"\n» Ingrese número de Fila (0 a {cuadriculaTerreno.GetLength(0) - 1}): ");
            int c = Solicitudes.CapturarEntero($"» Ingrese número de Columna (0 a {cuadriculaTerreno.GetLength(1) - 1}): ");

            if (f < 0 || f >= cuadriculaTerreno.GetLength(0) || c < 0 || c >= cuadriculaTerreno.GetLength(1))
            {
                Console.WriteLine("❌ Error de coordenadas: Posición fuera del rango asignado.");
                Solicitudes.PausarGesto();
                return;
            }

            if (cuadriculaTerreno[f, c].TieneSiembra)
            {
                Console.WriteLine("❌ Error de campo: Parcela ocupada con cultivos en desarrollo.");
                Solicitudes.PausarGesto();
                return;
            }

            Console.WriteLine("\nSeleccione la semilla de su inventario:");
            for (int i = 0; i < listaVariedades.Count; i++)
                Console.WriteLine($" [{i + 1}] {listaVariedades[i].Especie} (Almacén: {almacenSemillas[listaVariedades[i].Especie]})");

            int opt = Solicitudes.CapturarEntero("» Selección: ") - 1;
            if (opt < 0 || opt >= listaVariedades.Count) return;

            Cultivo escogido = listaVariedades[opt];

            if (almacenSemillas[escogido.Especie] > 0)
            {
                almacenSemillas[escogido.Especie]--;
                cuadriculaTerreno[f, c].AlojarCultivo(escogido);
                Console.WriteLine($"\n🌱 Operación exitosa: [{escogido.Especie}] sembrado en la parcela [{f},{c}].");
            }
            else
            {
                Console.WriteLine("\n❌ Almacén vacío: No dispone de existencias de este cultivo.");
            }
            Solicitudes.PausarGesto();
        }

        private void RenderizarMapaYDetalle()
        {
            int filas = cuadriculaTerreno.GetLength(0);
            int cols = cuadriculaTerreno.GetLength(1);

            Console.WriteLine("══════════════════════════════════════════════════════");
            Console.WriteLine("                MAPA TOPOGRÁFICO DE LOTES             ");
            Console.WriteLine("══════════════════════════════════════════════════════\n");

            Console.Write("       ");
            for (int j = 0; j < cols; j++) Console.Write($"C{j}  ");
            Console.WriteLine();

            for (int i = 0; i < filas; i++)
            {
                Console.Write($" [F{i}]  ");
                for (int j = 0; j < cols; j++)
                {
                    string icono = cuadriculaTerreno[i, j].ObtenerIconoProgreso();
                    Console.Write($"[{icono}] ");
                }
                Console.WriteLine();
            }
            Console.WriteLine("\n Leyenda: [ . ] Baldío  |  [🌱] Fase Inicial  |  [🌿] Fase Media  |  [💰] Listo para Cosecha");

            Console.WriteLine("\n──────────────────────────────────────────────────────");
            int rf = Solicitudes.CapturarEntero("» Ingrese la coordenada Fila a auditar: ");
            int rc = Solicitudes.CapturarEntero("» Ingrese la coordenada Columna a auditar: ");

            if (rf >= 0 && rf < filas && rc >= 0 && rc < cols)
            {
                Parcela slot = cuadriculaTerreno[rf, rc];
                Console.WriteLine($"\n📊 INFORME TÉCNICO DE PARCELA [{rf},{rc}]:");
                if (slot.TieneSiembra && slot.CultivoActivo != null)
                {
                    Console.WriteLine($" • Especie Cultivada: {slot.CultivoActivo.Especie}");
                    Console.WriteLine($" • Ciclo de Progreso: {slot.TiempoTranscurrido} de {slot.CultivoActivo.MesesMaduracion} meses.");
                    Console.WriteLine($" • Tiempo de Espera : {(slot.CultivoActivo.MesesMaduracion - slot.TiempoTranscurrido)} mes(es)");
                    Console.WriteLine($" • Retorno Estimado : Q{slot.CultivoActivo.ValorMercado:N2}");
                }
                else
                {
                    Console.WriteLine(" • Terreno disponible / Libre de siembra.");
                }
            }
            else
            {
                Console.WriteLine("❌ Error: Parámetros fuera de los límites catastrales.");
            }
            Solicitudes.PausarGesto();
        }

        private void EjecutarTransicionMes()
        {
            Console.WriteLine("══════════════════════════════════════════════════════");
            Console.WriteLine("             PROCESAMIENTO DE CIERRE MENSUAL          ");
            Console.WriteLine("══════════════════════════════════════════════════════\n");

            double nominaMensual = totalEmpleados * sueldoFijo;
            cajaEfectivo -= nominaMensual;
            mesesPorSimular--;
            mesesTranscurridos++;

            Console.WriteLine($"💸 Planilla de Campo: Se dedujeron Q{nominaMensual:N2} de las arcas por salarios.");
            Console.WriteLine("⚙️ Analizando madurez y biomasa del terreno...\n");

            for (int i = 0; i < cuadriculaTerreno.GetLength(0); i++)
            {
                for (int j = 0; j < cuadriculaTerreno.GetLength(1); j++)
                {
                    Parcela p = cuadriculaTerreno[i, j];
                    if (p.TieneSiembra && p.CultivoActivo != null)
                    {
                        p.IncrementarMes();

                        if (p.EvaluarMadurez())
                        {
                            double rendimiento = p.CultivoActivo.ValorMercado;
                            cajaEfectivo += rendimiento;
                            ingresoCosechasTotales += rendimiento;
                            Console.WriteLine($" ✨ [COSECHA AUTOMÁTICA] Parcela [{i},{j}]: {p.CultivoActivo.Especie} maduró. Caja: +Q{rendimiento:N2}");
                            p.Vaciar();
                        }
                        else
                        {
                            Console.WriteLine($" 🌾 [DESARROLLO] Parcela [{i},{j}] ({p.CultivoActivo.Especie}): {p.TiempoTranscurrido}/{p.CultivoActivo.MesesMaduracion} meses.");
                        }
                    }
                }
            }

            Console.WriteLine($"\n Cashflow Líquido al término del ciclo: Q{cajaEfectivo:N2}");
            Solicitudes.PausarGesto();
        }

        private void GenerarCierreContable()
        {
            double costoTotalSueldos = totalEmpleados * sueldoFijo * mesesTranscurridos;
            double inventarioEnProcesoValuado = 0;

            for (int i = 0; i < cuadriculaTerreno.GetLength(0); i++)
            {
                for (int j = 0; j < cuadriculaTerreno.GetLength(1); j++)
                {
                    if (cuadriculaTerreno[i, j].TieneSiembra && cuadriculaTerreno[i, j].CultivoActivo != null)
                    {
                        inventarioEnProcesoValuado += cuadriculaTerreno[i, j].CultivoActivo!.ValorMercado;
                    }
                }
            }

            // Aplicación matemática estricta de la fórmula requerida por la Facultad
            double utilidadesFormulaAcademica = capitalSemilla + ingresoCosechasTotales + inventarioEnProcesoValuado - costoTotalSueldos - egresoSemillasTotales;

            Console.WriteLine("====================================================================");
            Console.WriteLine("               AUDITORÍA FINANCIERA FINAL - ADB                     ");
            Console.WriteLine("====================================================================");
            Console.WriteLine($" (+) Capital de Trabajo Inicial  : Q{capitalSemilla,14:N2}");
            Console.WriteLine($" (+) Ingresos por Venta Cosecha  : Q{ingresoCosechasTotales,14:N2}");
            Console.WriteLine($" (+) Valuación de Activos Vivos  : Q{inventarioEnProcesoValuado,14:N2}");
            Console.WriteLine($" (-) Gastos de Operación (Nómina): Q{costoTotalSueldos,14:N2}");
            Console.WriteLine($" (-) Inversión Materia Prima     : Q{egresoSemillasTotales,14:N2}");
            Console.WriteLine("--------------------------------------------------------------------");
            Console.WriteLine($" (=) UTILIDADES NETAS SIMULACIÓN : Q{utilidadesFormulaAcademica,14:N2}");
            Console.WriteLine("====================================================================");
            Console.WriteLine($" Efectivo Real Remanente en Caja : Q{cajaEfectivo:N2}");
            Console.WriteLine($" Ciclos Cronológicos Completados : {mesesTranscurridos} meses.");
            Console.WriteLine("====================================================================");
            Console.WriteLine("\n[Proceso de simulación cerrado correctamente. Presione ENTER para salir]");
            Console.ReadLine();
        }
    }

    // CLASE AUXILIAR CON VALIDACIÓN INTEGRAL DE ENTRADAS (EVITA CRASHES)
    public static class Solicitudes
    {
        public static int CapturarEntero(string instruccion)
        {
            int salida;
            while (true)
            {
                Console.Write(instruccion);
                if (int.TryParse(Console.ReadLine(), out salida) && salida >= 0)
                {
                    return salida;
                }
                Console.WriteLine("❌ Entrada inválida. Ingrese un número entero no negativo.");
            }
        }

        public static double CapturarDecimal(string instruccion)
        {
            double salida;
            while (true)
            {
                Console.Write(instruccion);
                if (double.TryParse(Console.ReadLine(), out salida) && salida >= 0)
                {
                    return salida;
                }
                Console.WriteLine("❌ Entrada inválida. Ingrese un valor numérico/decimal no negativo.");
            }
        }

        public static void PausarGesto()
        {
            Console.WriteLine("\n[Presione ENTER para regresar al menú principal]");
            Console.ReadLine();
        }
    }
}