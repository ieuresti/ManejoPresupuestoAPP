using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioCuentas
    {
        Task Crear(Cuenta cuenta);
        Task<IEnumerable<Cuenta>> Buscar(int usuarioId);
        Task<Cuenta> ObtenerPorId(int id, int usuarioId);
        Task Actualizar(CuentaCreacionViewModel cuenta);
        Task Borrar(int id);
    }
    public class RepositorioCuentas: IRepositorioCuentas
    {
        private readonly string connectionString;

        public RepositorioCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Cuenta cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(
                @"INSERT INTO Cuentas (Nombre, TipoCuentaId, Balance, Descripcion)
                VALUES (@Nombre, @TipoCuentaId, @Balance, @Descripcion);
                SELECT SCOPE_IDENTITY();", cuenta
            );
            cuenta.Id = id;
        }

        public async Task<IEnumerable<Cuenta>> Buscar(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Cuenta>(
                @"SELECT Cuentas.Id, Cuentas.Nombre, TiposCuentas.Nombre AS TipoCuenta, Balance, Cuentas.Descripcion
                FROM Cuentas
                INNER JOIN TiposCuentas
                ON Cuentas.TipoCuentaId = TiposCuentas.Id
                WHERE TiposCuentas.UsuarioId = @UsuarioId
                ORDER BY TiposCuentas.Orden;", new { usuarioId }
            );
        }

        public async Task<Cuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Cuenta>(
                @"SELECT Cuentas.Id, Cuentas.Nombre, TiposCuentas.Id, Balance, Cuentas.Descripcion
                FROM Cuentas
                INNER JOIN TiposCuentas
                ON Cuentas.TipoCuentaId = TiposCuentas.Id
                WHERE TiposCuentas.UsuarioId = @UsuarioId AND Cuentas.Id = @Id", new { id, usuarioId }
            );
        }

        public async Task Actualizar(CuentaCreacionViewModel cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                @"UPDATE Cuentas
                SET Nombre = @Nombre, TipoCuentaId = @TipoCuentaId, Balance = @Balance, Descripcion = @Descripcion
                WHERE Id = @Id", cuenta
            );
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                @"DELETE Cuentas
                WHERE Id = @Id", new { id }
            );
        }
    }
}
