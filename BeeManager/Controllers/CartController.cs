using BeeManager.Contracts;
using BeeManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeeManager.Controllers;

[Authorize]
[Route("api/cart")]
public class CartController : ApiControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CartItemDto>>> Get(CancellationToken cancellationToken)
    {
        return Ok(await _cartService.GetAsync(CurrentUserId, cancellationToken));
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse>> Replace(UpdateCartRequest request, CancellationToken cancellationToken)
    {
        await _cartService.SetAsync(CurrentUserId, request.Items, cancellationToken);
        return Ok(new ApiResponse { Message = "Koszyk został zapisany." });
    }

    [HttpPost("items")]
    public async Task<ActionResult<ApiResponse>> Add(CartItemDto item, CancellationToken cancellationToken)
    {
        await _cartService.AddAsync(CurrentUserId, item, cancellationToken);
        return Ok(new ApiResponse { Message = "Produkt został dodany do koszyka." });
    }

    [HttpDelete("items/{itemId:int}")]
    public async Task<ActionResult<ApiResponse>> Remove(int itemId, CancellationToken cancellationToken)
    {
        await _cartService.RemoveAsync(CurrentUserId, itemId, cancellationToken);
        return Ok(new ApiResponse { Message = "Produkt został usunięty z koszyka." });
    }

    [HttpDelete("clear")]
    public async Task<ActionResult<ApiResponse>> Clear(CancellationToken cancellationToken)
    {
        await _cartService.ClearAsync(CurrentUserId, cancellationToken);
        return Ok(new ApiResponse { Message = "Koszyk został wyczyszczony." });
    }
}
