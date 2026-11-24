namespace Parqueadero.Core.Interfaces;

public interface IRepositorio<T> where T : class
{
    IQueryable<T> Consultar();
    Task AgregarAsync(T entidad, CancellationToken cancellationToken = default);
    Task GuardarAsync(CancellationToken cancellationToken = default);
}
