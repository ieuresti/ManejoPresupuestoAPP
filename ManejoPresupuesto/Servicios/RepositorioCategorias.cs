using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface  IRepositorioCategorias
    {
        Task Crear(Categoria categoria);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId, PaginacionViewModel paginacion);
        Task<int> Contar(int usuarioId);
        Task<Categoria> ObtenerPorId(int id, int usuarioId);
        Task<IEnumerable<Categoria>> ObtenerPorTipoOperacion(int usuarioId, TipoOperacion tipoOperacionId);
        Task Actualizar(Categoria categoria);
        Task Borrar(int id);
    }
    public class RepositorioCategorias: IRepositorioCategorias
    {
        private readonly string connectionString;
        public RepositorioCategorias(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear (Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(
                @"INSERT INTO Categorias (Nombre, TipoOperacionId, UsuarioId)
                VALUES (@Nombre, @TipoOperacionId, @UsuarioId);
                SELECT SCOPE_IDENTITY();", categoria
            );
            categoria.Id = id;
        }

        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId, PaginacionViewModel paginacion)
        {
            // Usamos parametros para evitar interpolación directa en la consulta.
            using var connection = new SqlConnection(connectionString);
            // OFFSET y FETCH requieren ORDER BY para que la paginación sea consistente.
            // OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY se parametriza para mayor seguridad.
            var sql = @"
                SELECT *
                FROM Categorias
                WHERE UsuarioId = @UsuarioId
                ORDER BY Nombre
                OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;";

            var parametros = new
            {
                UsuarioId = usuarioId,
                Skip = paginacion.RecordsASaltar,
                Take = paginacion.RecordsPorPagina
            };

            return await connection.QueryAsync<Categoria>(sql, parametros);
        }

        public async Task<int> Contar(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Categorias WHERE UsuarioId = @usuarioId;", new { usuarioId }
            );
        }

        public async Task<Categoria> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Categoria>(
            @"SELECT *
            FROM Categorias
            WHERE Id = @Id AND UsuarioId = @UsuarioId;", new { id, usuarioId }
            );
        }

        public async Task<IEnumerable<Categoria>> ObtenerPorTipoOperacion(int usuarioId, TipoOperacion tipoOperacionId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Categoria>(
                @"SELECT *
                FROM Categorias
                WHERE UsuarioId = @UsuarioId AND TipoOperacionId = @TipoOperacionId;", new { usuarioId, tipoOperacionId }
            );
        }

        public async Task Actualizar(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                @"UPDATE Categorias
                SET Nombre = @Nombre, TipoOperacionId = @TipoOperacionId
                WHERE Id = @Id;", categoria
            );
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                @"DELETE Categorias
                WHERE Id = @Id;", new { id }
            );
        }
    }
}
