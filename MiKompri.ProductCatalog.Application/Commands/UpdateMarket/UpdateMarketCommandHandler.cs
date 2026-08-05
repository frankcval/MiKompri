using MediatR;
using MiKompri.ProductCatalog.Application.Interfaces;

namespace MiKompri.ProductCatalog.Application.Commands.UpdateMarket;

public class UpdateMarketCommandHandler : IRequestHandler<UpdateMarketCommand>
{
    private readonly IMarketRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMarketCommandHandler(IMarketRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateMarketCommand request, CancellationToken cancellationToken)
    {
        var market = await _repository.GetByIdAsync(request.MarketId, cancellationToken)
                     ?? throw new KeyNotFoundException("Mercado no encontrado.");

        var exists = await _repository.ExistsActiveByNameAsync(request.Name.Trim(), request.MarketId, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("Ya existe un mercado activo con el mismo nombre.");
        }

        market.UpdateDetails(request.Name, request.LocationHint);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
