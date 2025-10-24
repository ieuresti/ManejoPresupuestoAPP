using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using System.Transactions;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioTransacciones
    {
        Task Crear(Transaccion transaccion);
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnterior);
        Task<Transaccion> ObtenerPorId(int id, int usuarioId);
        Task Borrar(int id);
    }
    public class RepositorioTransacciones: IRepositorioTransacciones
    {
        private readonly string connectionString;
        public RepositorioTransacciones(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>("Transacciones_Insertar", 
                new { transaccion.UsuarioId, transaccion.FechaTransaccion, transaccion.Monto, transaccion.Nota, transaccion.CuentaId, transaccion.CategoriaId },
                commandType: System.Data.CommandType.StoredProcedure
            );
            transaccion.Id = id;
        }

        public async Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnteriorId)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("Transacciones_Actualizar",
                new { transaccion.Id, transaccion.FechaTransaccion, transaccion.Monto, transaccion.Nota, transaccion.CuentaId, transaccion.CategoriaId, montoAnterior, cuentaAnteriorId },
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Transaccion>(
                @"SELECT Transacciones.*, Categorias.TipoOperacionId
                FROM Transacciones
                INNER JOIN Categorias
                ON Categorias.Id = Transacciones.CategoriaId
                WHERE Transacciones.Id = @Id AND Transacciones.UsuarioId = @UsuarioId;", new { id, usuarioId }
            );
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("Transacciones_Borrar",
                new { id },
                commandType: System.Data.CommandType.StoredProcedure
            );
        }
    }
}
