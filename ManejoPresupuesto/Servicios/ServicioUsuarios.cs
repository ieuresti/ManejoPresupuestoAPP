using System.Security.Claims;

namespace ManejoPresupuesto.Servicios
{
    public interface IServicioUsuarios
    {
        int ObtenerUsuarioId();
    }
    public class ServicioUsuarios: IServicioUsuarios
    {
        private readonly HttpContext httpContext;

        public ServicioUsuarios(IHttpContextAccessor httpContextAccessor)
        {
            // Guardamos la referencia al HttpContext actual para poder leer los claims del usuario
            httpContext = httpContextAccessor.HttpContext;
        }

        public int ObtenerUsuarioId()
        {
            // Comprobar que el usuario está autenticado
            if (httpContext.User.Identity.IsAuthenticated)
            {
                // Buscar el claim que contiene el id (NameIdentifier)
                var idClaim = httpContext.User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
                // Intentar convertir el valor del claim a entero evitando excepciones
                var id = int.Parse(idClaim.Value);
                return id;
            }
            else
            {
                // El usuario no está autenticado => no hay id disponible
                throw new ApplicationException("El usuario no esta autenticado");
            }
        }
    }
}
