using System.Collections.Concurrent;
using BeeManager.Contracts;

namespace BeeManager.Services;

public class CartService : ICartService
{
    private readonly ConcurrentDictionary<string, List<CartItemDto>> _carts = new();
    private readonly object _lock = new();

    public Task<IReadOnlyCollection<CartItemDto>> GetAsync(string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            var items = _carts.TryGetValue(userId, out var cart)
                ? cart.Select(item => new CartItemDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Price = item.Price
                }).ToList()
                : new List<CartItemDto>();

            return Task.FromResult<IReadOnlyCollection<CartItemDto>>(items);
        }
    }

    public Task SetAsync(string userId, IReadOnlyCollection<CartItemDto> items, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            _carts[userId] = items
                .Select(item => new CartItemDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Price = item.Price
                })
                .ToList();
        }

        return Task.CompletedTask;
    }

    public Task AddAsync(string userId, CartItemDto item, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            var cart = _carts.GetOrAdd(userId, _ => new List<CartItemDto>());
            cart.Add(new CartItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Price = item.Price
            });
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string userId, int itemId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            if (!_carts.TryGetValue(userId, out var cart))
            {
                return Task.CompletedTask;
            }

            var index = cart.FindIndex(item => item.Id == itemId);
            if (index >= 0)
            {
                cart.RemoveAt(index);
            }
        }

        return Task.CompletedTask;
    }

    public Task ClearAsync(string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            _carts.Remove(userId, out _);
        }

        return Task.CompletedTask;
    }
}
