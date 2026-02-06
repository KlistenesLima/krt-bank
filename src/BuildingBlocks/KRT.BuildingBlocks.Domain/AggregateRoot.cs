using System.Collections.Generic;

namespace KRT.BuildingBlocks.Domain
{
    // Agora herda puramente de Entity sem ofuscar membros
    public abstract class AggregateRoot : Entity, IAggregateRoot
    {
        // Lógica de eventos delegada para a classe base Entity
    }
}
