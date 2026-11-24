using Microsoft.EntityFrameworkCore;
using Parqueadero.Core.Interfaces;

namespace Parqueadero.Data.Repositories;

public sealed class Repositorio<T>(AppDbContext context) : IRepositorio<T> where T : class
{
    public IQueryable<T> Consultar() => context.Set<T>().AsQueryable();

    public async Task AgregarAsync(T entidad, CancellationToken cancellationToken = default) =>
        await context.Set<T>().AddAsync(entidad, cancellationToken);

    public Task GuardarAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
