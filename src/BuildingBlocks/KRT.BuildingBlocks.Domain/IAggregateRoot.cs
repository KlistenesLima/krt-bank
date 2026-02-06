namespace KRT.BuildingBlocks.Domain
{
    /// <summary>
    /// Interface marcadora para Raízes de Agregação.
    /// Útil para restringir Repositórios Genéricos (ex: IRepository<T> where T : IAggregateRoot).
    /// </summary>
    public interface IAggregateRoot { }
}
