using BeeManager.Contracts;

namespace BeeManager.Services;

public interface ICartService
{
    Task<IReadOnlyCollection<CartItemDto>> GetAsync(string userId, CancellationToken cancellationToken = default);

    Task SetAsync(string userId, IReadOnlyCollection<CartItemDto> items, CancellationToken cancellationToken = default);

    Task AddAsync(string userId, CartItemDto item, CancellationToken cancellationToken = default);

    Task RemoveAsync(string userId, int itemId, CancellationToken cancellationToken = default);

    Task ClearAsync(string userId, CancellationToken cancellationToken = default);
}
