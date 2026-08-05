using MediatR;
using MiKompri.ProductCatalog.Application.Interfaces;

namespace MiKompri.ProductCatalog.Application.Commands.DeactivateMarket;

public class DeactivateMarketCommandHandler : IRequestHandler<DeactivateMarketCommand>
{
    private readonly IMarketRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateMarketCommandHandler(IMarketRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeactivateMarketCommand request, CancellationToken cancellationToken)
    {
        var market = await _repository.GetByIdAsync(request.MarketId, cancellationToken)
                     ?? throw new KeyNotFoundException("Mercado no encontrado.");

        market.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
