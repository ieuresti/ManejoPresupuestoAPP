using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioTiposCuentas
    {
        Task Crear(TipoCuenta tipoCuenta);
        Task<bool> Existe(string nombre, int usuarioId);
        Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId);
        Task Actualizar(TipoCuenta tipoCuenta);
        Task<TipoCuenta> ObtenerPorId(int id, int usuarioId);
        Task Borrar(int id);
        Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenados);
    }
    public class RepositorioTiposCuentas: IRepositorioTiposCuentas
    {
        private readonly string connectionString;
        public RepositorioTiposCuentas(IConfiguration configuration)
        {
            // Obtener la cadena de conexion del appsettings.json
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(TipoCuenta tipoCuenta)
        {
            // Using asegura que se cierre y libere la conexion una vez que se termine de usar
            using var connection = new SqlConnection(connectionString);
            // Insertar y retornar el id del nuevo registro
            // QuerySingleAsync sirve para ejecutar una consulta que retorna 1 solo valor
            // CommandType.StoredProcedure indica que se esta ejecutando un SP
            var id = await connection.QuerySingleAsync<int>(
                "TiposCuentas_Insertar",
                new { usuarioId = tipoCuenta.UsuarioId, nombre = tipoCuenta.Nombre },
                commandType: System.Data.CommandType.StoredProcedure
            );
            tipoCuenta.Id = id;
        }

        public async Task<bool> Existe(string nombre, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            // QueryFirstOrDefaultAsync sirve para ejecutar una consulta que retorna 1 solo valor o el valor por defecto del tipo si no encuentra nada
            var existe = await connection.QueryFirstOrDefaultAsync<int>(
                // Este query retorna 1 si encuentra un registro que cumpla con la condicion, si no encuentra nada retorna 0
                // El new {nombre, usuarioId} crea un obj anonimo con las propiedades nombre y usuarioId que Dapper usa para relacionar con los parametros de la consulta
                @"SELECT 1
                FROM TiposCuentas
                WHERE Nombre = @Nombre AND UsuarioId = @UsuarioId;", new { nombre, usuarioId }
            );
            // Si existe es 1 entonces retorna true, si es 0 retorna false
            return existe == 1;
        }

        public async Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            // QueryAsync sirve para ejecutar una consulta que retorna una coleccion de objetos
            return await connection.QueryAsync<TipoCuenta>(
                @"SELECT Id, Nombre, UsuarioId, Orden
                FROM TiposCuentas
                WHERE UsuarioId = @UsuarioId
                ORDER BY Orden;", new { usuarioId }
            );
        }

        public async Task Actualizar(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            // ExecuteAsync sirve para ejecutar una consulta que no retorna nada (INSERT, UPDATE, DELETE)
            await connection.ExecuteAsync(
                @"UPDATE TiposCuentas
                SET Nombre = @Nombre
                WHERE Id = @Id;", tipoCuenta
            );
        }

        public async Task<TipoCuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<TipoCuenta>(
                @"SELECT Id, Nombre, Orden
                FROM TiposCuentas
                WHERE Id = @Id AND UsuarioId = @UsuarioId;", new { id, usuarioId }
            );
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                @"DELETE TiposCuentas
                WHERE Id = @Id;", new { id }
            );
        }

        public async Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenados)
        {
            using var connection = new SqlConnection(connectionString);
            var query = "UPDATE TiposCuentas SET Orden = @Orden WHERE Id = @Id;";
            await connection.ExecuteAsync(query, tipoCuentasOrdenados);
        }

    }
}
