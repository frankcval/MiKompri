using MediatR;
using MiKompri.ProductCatalog.Application.Abstractions;
using MiKompri.ProductCatalog.Application.Interfaces;
using MiKompri.ProductCatalog.Domain.Products;
using MiKompri.ProductCatalog.Domain.Products.ValueObjects;

namespace MiKompri.ProductCatalog.Application.Commands.RegisterProductPrice;

public class RegisterProductPriceCommandHandler : IRequestHandler<RegisterProductPriceCommand, Guid>
{
    private const string DefaultCurrency = "EUR";

    private readonly ICatalogProductRepository _catalogProductRepository;
    private readonly IMarketRepository _marketRepository;
    private readonly IProductPriceRecordRepository _productPriceRecordRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterProductPriceCommandHandler(
        ICatalogProductRepository catalogProductRepository,
        IMarketRepository marketRepository,
        IProductPriceRecordRepository productPriceRecordRepository,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _catalogProductRepository = catalogProductRepository;
        _marketRepository = marketRepository;
        _productPriceRecordRepository = productPriceRecordRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(RegisterProductPriceCommand request, CancellationToken cancellationToken)
    {
        if (request.EffectiveDate > _dateTimeProvider.UtcToday)
        {
            throw new InvalidOperationException("No se puede registrar precio con fecha futura.");
        }

        var product = await _catalogProductRepository.GetByIdAsync(request.CatalogProductId, cancellationToken)
                     ?? throw new KeyNotFoundException("Producto no encontrado.");

        var market = await _marketRepository.GetByIdAsync(request.MarketId, cancellationToken)
                    ?? throw new KeyNotFoundException("Mercado no encontrado.");

        if (!product.IsActive)
        {
            throw new InvalidOperationException("No se puede registrar precio para un producto inactivo.");
        }

        if (!market.IsActive)
        {
            throw new InvalidOperationException("No se puede registrar precio para un mercado inactivo.");
        }

        var exists = await _productPriceRecordRepository.ExistsAsync(
            request.CatalogProductId,
            request.MarketId,
            request.EffectiveDate,
            cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("Ya existe un registro de precio para producto, mercado y fecha efectiva.");
        }

        var price = new Money(request.PriceAmount, DefaultCurrency);
        var record = new ProductPriceRecord(request.CatalogProductId, request.MarketId, request.EffectiveDate, price);

        await _productPriceRecordRepository.AddAsync(record, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return record.Id;
    }
}
