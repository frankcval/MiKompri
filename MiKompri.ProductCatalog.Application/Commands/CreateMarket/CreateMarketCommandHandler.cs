using MediatR;
using MiKompri.ProductCatalog.Application.Interfaces;
using MiKompri.ProductCatalog.Domain.Markets;

namespace MiKompri.ProductCatalog.Application.Commands.CreateMarket;

public class CreateMarketCommandHandler : IRequestHandler<CreateMarketCommand, Guid>
{
    private readonly IMarketRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMarketCommandHandler(IMarketRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateMarketCommand request, CancellationToken cancellationToken)
    {
        var exists = await _repository.ExistsActiveByNameAsync(request.Name.Trim(), null, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("Ya existe un mercado activo con el mismo nombre.");
        }

        var market = new Market(request.Name, request.LocationHint);

        await _repository.AddAsync(market, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return market.Id;
    }
}
