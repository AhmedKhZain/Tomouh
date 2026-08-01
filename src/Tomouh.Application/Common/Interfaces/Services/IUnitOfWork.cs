namespace Tomouh.Application.Common.Interfaces.Services;

public interface IUnitOfWork<TDb>
    where TDb : class, IUnitOfWork<TDb>
{
    void CollectEventsAsync();
}
