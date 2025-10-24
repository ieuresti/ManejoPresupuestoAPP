using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Models
{
    public class CuentaCreacionViewModel: Cuenta
    {
        // SelectListItem es una clase que representa un item en un dropdown list
        public IEnumerable<SelectListItem> TiposCuentas { get; set; }
    }
}
