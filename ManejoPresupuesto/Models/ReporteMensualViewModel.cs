namespace ManejoPresupuesto.Models
{
    public class ReporteMensualViewModel
    {
        public decimal Ingresos => TransaccionesPorMes.Sum(x => x.Ingresos);
        public decimal Gastos => TransaccionesPorMes.Sum(x => x.Gastos);
        public decimal Total => Ingresos - Gastos;
        public int Año { get; set; }
        public IEnumerable<ResultadoObtenerPorMes> TransaccionesPorMes { get; set; }
    }
}
