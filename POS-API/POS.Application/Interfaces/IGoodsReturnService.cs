using POS.Application.DTOs.GoodsReturn;

namespace POS.Application.Interfaces
{
    public interface IGoodsReturnService
    {
        Task<GoodsReturnDto> CreateReturnAsync(CreateGoodsReturnDto dto, Guid userId);
        Task<GoodsReturnDto> GetReturnByIdAsync(Guid id);
        Task<IEnumerable<GoodsReturnDto>> GetReturnsByLocationAsync(Guid locationId);
    }
}
