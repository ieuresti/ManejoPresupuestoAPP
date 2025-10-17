namespace ManejoPresupuesto.Servicios
{
    public interface IServicioUsuarios
    {
        int ObtenerUsuarioId();
    }
    public class ServicioUsuarios: IServicioUsuarios
    {
        public ServicioUsuarios()
        {
            
        }

        public int ObtenerUsuarioId()
        {
            return 1;
        }
    }
}
