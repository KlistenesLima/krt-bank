using KRT.Payments.Application.DTOs;
using KRT.Payments.Domain.Entities;
using KRT.Payments.Domain.Interfaces;
using KRT.BuildingBlocks.Domain; // Onde vive o IUnitOfWork

namespace KRT.Payments.Application.UseCases;

public class PixUseCase
{
    private readonly IPaymentRepository _repository;
    // Removido IUnitOfWork direto se o repositório já gerencia, 
    // mas mantendo para compatibilidade com sua estrutura atual
    private readonly IUnitOfWork _unitOfWork; 

    // Se o Repositório não expõe UnitOfWork, injetamos direto.
    // Mas o ideal é repo.UnitOfWork.
    public PixUseCase(IPaymentRepository repository)
    {
        _repository = repository;
        _unitOfWork = repository.UnitOfWork; 
    }

    public async Task<PixResponse> Handle(PixRequest request)
    {
        // 1. Criar Pagamento
        var payment = new Payment(request.AccountId, request.Key, request.Amount);
        
        // 2. Persistir
        await _repository.AddAsync(payment);
        await _unitOfWork.CommitAsync();

        // TODO: Publicar evento via EventBus

        return new PixResponse(payment.Id, payment.Status.ToString());
    }
}
