# ============================================================
# KRT Bank - Script de Deploy da Arquitetura Enterprise
# ============================================================
# Execute este script no PowerShell como Administrador
# Uso: .\deploy-krt-bank.ps1
# ============================================================

param(
    [string]$SourcePath = "$env:USERPROFILE\Downloads\krt-bank-files",
    [string]$TargetPath = "$env:USERPROFILE\Desktop\Sistemas 2026\krt-bank"
)

$ErrorActionPreference = "Stop"

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host " KRT Bank - Deploy da Arquitetura Enterprise" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

# Verifica se o diretório de destino existe
if (-not (Test-Path $TargetPath)) {
    Write-Host "[ERRO] Diretorio de destino nao encontrado: $TargetPath" -ForegroundColor Red
    exit 1
}

Write-Host "[INFO] Diretorio de destino: $TargetPath" -ForegroundColor Green
Write-Host ""

# ============================================================
# ESTRUTURA DE PASTAS
# ============================================================

Write-Host "[1/5] Criando estrutura de pastas..." -ForegroundColor Yellow

$folders = @(
    # BuildingBlocks
    "src\BuildingBlocks\KRT.BuildingBlocks.Domain\ValueObjects",
    "src\BuildingBlocks\KRT.BuildingBlocks.Domain\Exceptions",
    "src\BuildingBlocks\KRT.BuildingBlocks.EventBus\Kafka",
    "src\BuildingBlocks\KRT.BuildingBlocks.Infrastructure\Data",
    "src\BuildingBlocks\KRT.BuildingBlocks.Infrastructure\Outbox",
    
    # Onboarding Service
    "src\Services\KRT.Onboarding\KRT.Onboarding.Domain\Entities",
    "src\Services\KRT.Onboarding\KRT.Onboarding.Domain\Enums",
    "src\Services\KRT.Onboarding\KRT.Onboarding.Domain\Events",
    "src\Services\KRT.Onboarding\KRT.Onboarding.Domain\Interfaces",
    "src\Services\KRT.Onboarding\KRT.Onboarding.Application\Accounts\DTOs\Requests",
    "src\Services\KRT.Onboarding\KRT.Onboarding.Application\Accounts\DTOs\Responses",
    "src\Services\KRT.Onboarding\KRT.Onboarding.Application\Accounts\Services",
    "src\Services\KRT.Onboarding\KRT.Onboarding.Application\Mappings",
    "src\Services\KRT.Onboarding\KRT.Onboarding.Application\Validations",
    "src\Services\KRT.Onboarding\KRT.Onboarding.Infra.Data\Context",
    "src\Services\KRT.Onboarding\KRT.Onboarding.Infra.Data\Repositories",
    "src\Services\KRT.Onboarding\KRT.Onboarding.Infra.Data\Migrations",
    "src\Services\KRT.Onboarding\KRT.Onboarding.Infra.Cache\Redis",
    "src\Services\KRT.Onboarding\KRT.Onboarding.Infra.MessageQueue\Events",
    "src\Services\KRT.Onboarding\KRT.Onboarding.Infra.MessageQueue\Handlers",
    "src\Services\KRT.Onboarding\KRT.Onboarding.Infra.IoC",
    "src\Services\KRT.Onboarding\KRT.Onboarding.Api\Controllers",
    "src\Services\KRT.Onboarding\KRT.Onboarding.Api\Middlewares",
    "src\Services\KRT.Onboarding\KRT.Onboarding.Api\Extensions"
)

foreach ($folder in $folders) {
    $fullPath = Join-Path $TargetPath $folder
    if (-not (Test-Path $fullPath)) {
        New-Item -ItemType Directory -Path $fullPath -Force | Out-Null
        Write-Host "  [+] Criado: $folder" -ForegroundColor DarkGray
    }
}

Write-Host "[OK] Estrutura de pastas criada!" -ForegroundColor Green
Write-Host ""

# ============================================================
# BUILDING BLOCKS - DOMAIN
# ============================================================

Write-Host "[2/5] Criando Building Blocks..." -ForegroundColor Yellow

# Entity.cs
$entityCs = @'
namespace KRT.BuildingBlocks.Domain;

public abstract class Entity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected Entity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    protected Entity(Guid id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
    public void SetUpdatedAt() => UpdatedAt = DateTime.UtcNow;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => (GetType().ToString() + Id).GetHashCode();
    public static bool operator ==(Entity? a, Entity? b) => a is null && b is null || a is not null && a.Equals(b);
    public static bool operator !=(Entity? a, Entity? b) => !(a == b);
}
'@
Set-Content -Path (Join-Path $TargetPath "src\BuildingBlocks\KRT.BuildingBlocks.Domain\Entity.cs") -Value $entityCs -Encoding UTF8

# AggregateRoot.cs
$aggregateRootCs = @'
namespace KRT.BuildingBlocks.Domain;

public abstract class AggregateRoot : Entity
{
    public int Version { get; protected set; }

    protected AggregateRoot() : base() { }
    protected AggregateRoot(Guid id) : base(id) { }

    protected void IncrementVersion()
    {
        Version++;
        SetUpdatedAt();
    }
}
'@
Set-Content -Path (Join-Path $TargetPath "src\BuildingBlocks\KRT.BuildingBlocks.Domain\AggregateRoot.cs") -Value $aggregateRootCs -Encoding UTF8

# ValueObject.cs
$valueObjectCs = @'
namespace KRT.BuildingBlocks.Domain;

public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType()) return false;
        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode() =>
        GetEqualityComponents().Select(x => x?.GetHashCode() ?? 0).Aggregate((x, y) => x ^ y);

    public static bool operator ==(ValueObject? a, ValueObject? b) =>
        a is null && b is null || a is not null && a.Equals(b);
    public static bool operator !=(ValueObject? a, ValueObject? b) => !(a == b);
}
'@
Set-Content -Path (Join-Path $TargetPath "src\BuildingBlocks\KRT.BuildingBlocks.Domain\ValueObject.cs") -Value $valueObjectCs -Encoding UTF8

# IDomainEvent.cs
$domainEventCs = @'
namespace KRT.BuildingBlocks.Domain;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
    string EventType { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public abstract string EventType { get; }
}
'@
Set-Content -Path (Join-Path $TargetPath "src\BuildingBlocks\KRT.BuildingBlocks.Domain\IDomainEvent.cs") -Value $domainEventCs -Encoding UTF8

# Result.cs
$resultCs = @'
namespace KRT.BuildingBlocks.Domain;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public string? ErrorCode { get; }

    protected Result(bool isSuccess, string? error, string? errorCode)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true, null, null);
    public static Result Failure(string error, string? errorCode = null) => new(false, error, errorCode);
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(string error, string? errorCode = null) => Result<T>.Failure(error, errorCode);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string? error, string? errorCode)
        : base(isSuccess, error, errorCode) => Value = value;

    public static new Result<T> Success(T value) => new(true, value, null, null);
    public static new Result<T> Failure(string error, string? errorCode = null) => new(false, default, error, errorCode);
}
'@
Set-Content -Path (Join-Path $TargetPath "src\BuildingBlocks\KRT.BuildingBlocks.Domain\Result.cs") -Value $resultCs -Encoding UTF8

# Money.cs
$moneyCs = @'
using System.Globalization;

namespace KRT.BuildingBlocks.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = Math.Round(amount, 2);
        Currency = currency.ToUpperInvariant();
    }

    public static Money Create(decimal amount, string currency = "BRL") => new(amount, currency);
    public static Money Zero(string currency = "BRL") => new(0, currency);

    public static Money operator +(Money a, Money b) { EnsureSameCurrency(a, b); return new(a.Amount + b.Amount, a.Currency); }
    public static Money operator -(Money a, Money b) { EnsureSameCurrency(a, b); return new(a.Amount - b.Amount, a.Currency); }
    public static bool operator <(Money a, Money b) { EnsureSameCurrency(a, b); return a.Amount < b.Amount; }
    public static bool operator >(Money a, Money b) { EnsureSameCurrency(a, b); return a.Amount > b.Amount; }
    public static bool operator <=(Money a, Money b) => !(a > b);
    public static bool operator >=(Money a, Money b) => !(a < b);

    private static void EnsureSameCurrency(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException($"Cannot operate on different currencies: {a.Currency} and {b.Currency}");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Currency} {Amount:N2}";
}
'@
Set-Content -Path (Join-Path $TargetPath "src\BuildingBlocks\KRT.BuildingBlocks.Domain\ValueObjects\Money.cs") -Value $moneyCs -Encoding UTF8

# DomainException.cs
$domainExceptionCs = @'
namespace KRT.BuildingBlocks.Domain.Exceptions;

public class DomainException : Exception
{
    public string Code { get; }
    public DomainException(string message, string? code = null) : base(message) => Code = code ?? "DOMAIN_ERROR";
}

public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object entityId)
        : base($"{entityName} com ID '{entityId}' nao encontrado", "ENTITY_NOT_FOUND") { }
}

public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message, string? code = null)
        : base(message, code ?? "BUSINESS_RULE_VIOLATION") { }
}

public class ConcurrencyException : DomainException
{
    public ConcurrencyException(string entityName, object entityId)
        : base($"Conflito de concorrencia ao atualizar {entityName} com ID '{entityId}'", "CONCURRENCY_CONFLICT") { }
}
'@
Set-Content -Path (Join-Path $TargetPath "src\BuildingBlocks\KRT.BuildingBlocks.Domain\Exceptions\DomainException.cs") -Value $domainExceptionCs -Encoding UTF8

# KRT.BuildingBlocks.Domain.csproj
$domainCsproj = @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
'@
Set-Content -Path (Join-Path $TargetPath "src\BuildingBlocks\KRT.BuildingBlocks.Domain\KRT.BuildingBlocks.Domain.csproj") -Value $domainCsproj -Encoding UTF8

Write-Host "[OK] Building Blocks Domain criado!" -ForegroundColor Green

# ============================================================
# BUILDING BLOCKS - EVENT BUS
# ============================================================

# IntegrationEvent.cs
$integrationEventCs = @'
namespace KRT.BuildingBlocks.EventBus;

public abstract record IntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = string.Empty;
    public string CausationId { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public string EventType => GetType().Name;
}
'@
Set-Content -Path (Join-Path $TargetPath "src\BuildingBlocks\KRT.BuildingBlocks.EventBus\IntegrationEvent.cs") -Value $integrationEventCs -Encoding UTF8

# IEventBus.cs
$iEventBusCs = @'
namespace KRT.BuildingBlocks.EventBus;

public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : IntegrationEvent;
    Task PublishAsync<T>(T @event, string topic, CancellationToken ct = default) where T : IntegrationEvent;
}

public interface IEventHandler<in TEvent> where TEvent : IntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken ct = default);
}

[AttributeUsage(AttributeTargets.Class)]
public class TopicAttribute : Attribute
{
    public string Name { get; }
    public TopicAttribute(string name) => Name = name;
}
'@
Set-Content -Path (Join-Path $TargetPath "src\BuildingBlocks\KRT.BuildingBlocks.EventBus\IEventBus.cs") -Value $iEventBusCs -Encoding UTF8

# KRT.BuildingBlocks.EventBus.csproj
$eventBusCsproj = @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Confluent.Kafka" Version="2.3.0" />
    <PackageReference Include="Microsoft.Extensions.Hosting.Abstractions" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Options" Version="8.0.0" />
  </ItemGroup>
</Project>
'@
Set-Content -Path (Join-Path $TargetPath "src\BuildingBlocks\KRT.BuildingBlocks.EventBus\KRT.BuildingBlocks.EventBus.csproj") -Value $eventBusCsproj -Encoding UTF8

Write-Host "[OK] Building Blocks EventBus criado!" -ForegroundColor Green

# ============================================================
# BUILDING BLOCKS - INFRASTRUCTURE
# ============================================================

# IUnitOfWork.cs
$unitOfWorkCs = @'
namespace KRT.BuildingBlocks.Infrastructure.Data;

public interface IUnitOfWork : IDisposable
{
    Task<int> CommitAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
'@
Set-Content -Path (Join-Path $TargetPath "src\BuildingBlocks\KRT.BuildingBlocks.Infrastructure\Data\IUnitOfWork.cs") -Value $unitOfWorkCs -Encoding UTF8

# Repository.cs
$repositoryCs = @'
using System.Linq.Expressions;
using KRT.BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;

namespace KRT.BuildingBlocks.Infrastructure.Data;

public interface IRepository<T> where T : Entity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}

public class Repository<T, TContext> : IRepository<T> where T : Entity where TContext : DbContext
{
    protected readonly TContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(TContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await DbSet.FindAsync(new object[] { id }, ct);

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default) =>
        await DbSet.ToListAsync(ct);

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        await DbSet.Where(predicate).ToListAsync(ct);

    public virtual async Task AddAsync(T entity, CancellationToken ct = default) =>
        await DbSet.AddAsync(entity, ct);

    public virtual void Update(T entity) => DbSet.Update(entity);
    public virtual void Remove(T entity) => DbSet.Remove(entity);
}
'@
Set-Content -Path (Join-Path $TargetPath "src\BuildingBlocks\KRT.BuildingBlocks.Infrastructure\Data\Repository.cs") -Value $repositoryCs -Encoding UTF8

# OutboxMessage.cs
$outboxMessageCs = @'
namespace KRT.BuildingBlocks.Infrastructure.Outbox;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; }
    public DateTime? ProcessedOn { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
    public string? CorrelationId { get; set; }
}
'@
Set-Content -Path (Join-Path $TargetPath "src\BuildingBlocks\KRT.BuildingBlocks.Infrastructure\Outbox\OutboxMessage.cs") -Value $outboxMessageCs -Encoding UTF8

# KRT.BuildingBlocks.Infrastructure.csproj
$infraCsproj = @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="MediatR" Version="12.2.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Hosting.Abstractions" Version="8.0.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\KRT.BuildingBlocks.Domain\KRT.BuildingBlocks.Domain.csproj" />
    <ProjectReference Include="..\KRT.BuildingBlocks.EventBus\KRT.BuildingBlocks.EventBus.csproj" />
  </ItemGroup>
</Project>
'@
Set-Content -Path (Join-Path $TargetPath "src\BuildingBlocks\KRT.BuildingBlocks.Infrastructure\KRT.BuildingBlocks.Infrastructure.csproj") -Value $infraCsproj -Encoding UTF8

Write-Host "[OK] Building Blocks Infrastructure criado!" -ForegroundColor Green
Write-Host ""

# ============================================================
# FINALIZAÇÃO
# ============================================================

Write-Host "[3/5] Os arquivos do servico Onboarding precisam ser baixados do chat..." -ForegroundColor Yellow
Write-Host ""
Write-Host "[4/5] Atualizando solution..." -ForegroundColor Yellow

# Cria arquivo de solução se não existir
$slnPath = Join-Path $TargetPath "KRT.Bank.sln"
if (-not (Test-Path $slnPath)) {
    $slnContent = @'
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
EndGlobal
'@
    Set-Content -Path $slnPath -Value $slnContent -Encoding UTF8
    Write-Host "  [+] Solution criada: KRT.Bank.sln" -ForegroundColor DarkGray
}

Write-Host "[OK] Solution atualizada!" -ForegroundColor Green
Write-Host ""

Write-Host "[5/5] Criando docker-compose.yml..." -ForegroundColor Yellow

$dockerCompose = @'
version: '3.8'

services:
  postgres:
    image: postgres:16-alpine
    container_name: krt-postgres
    environment:
      POSTGRES_USER: krt_admin
      POSTGRES_PASSWORD: KrtBank@2024
      POSTGRES_DB: krt_onboarding
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - krt-network

  redis:
    image: redis:7-alpine
    container_name: krt-redis
    command: redis-server --requirepass KrtBank@2024
    ports:
      - "6379:6379"
    networks:
      - krt-network

  zookeeper:
    image: confluentinc/cp-zookeeper:7.5.0
    container_name: krt-zookeeper
    environment:
      ZOOKEEPER_CLIENT_PORT: 2181
    ports:
      - "2181:2181"
    networks:
      - krt-network

  kafka:
    image: confluentinc/cp-kafka:7.5.0
    container_name: krt-kafka
    depends_on:
      - zookeeper
    ports:
      - "9092:9092"
    environment:
      KAFKA_BROKER_ID: 1
      KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://localhost:9092
      KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 1
    networks:
      - krt-network

  kafka-ui:
    image: provectuslabs/kafka-ui:latest
    container_name: krt-kafka-ui
    depends_on:
      - kafka
    ports:
      - "8080:8080"
    environment:
      KAFKA_CLUSTERS_0_NAME: krt-local
      KAFKA_CLUSTERS_0_BOOTSTRAPSERVERS: kafka:9092
    networks:
      - krt-network

  rabbitmq:
    image: rabbitmq:3.12-management-alpine
    container_name: krt-rabbitmq
    environment:
      RABBITMQ_DEFAULT_USER: krt_admin
      RABBITMQ_DEFAULT_PASS: KrtBank@2024
    ports:
      - "5672:5672"
      - "15672:15672"
    networks:
      - krt-network

volumes:
  postgres_data:

networks:
  krt-network:
    driver: bridge
'@
Set-Content -Path (Join-Path $TargetPath "docker-compose.yml") -Value $dockerCompose -Encoding UTF8

Write-Host "[OK] docker-compose.yml criado!" -ForegroundColor Green
Write-Host ""

Write-Host "============================================================" -ForegroundColor Green
Write-Host " DEPLOY CONCLUIDO COM SUCESSO!" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
Write-Host ""
Write-Host "Proximos passos:" -ForegroundColor Cyan
Write-Host "  1. Baixe os arquivos restantes do chat (Onboarding Service)" -ForegroundColor White
Write-Host "  2. Execute: docker-compose up -d" -ForegroundColor White
Write-Host "  3. Execute: dotnet restore" -ForegroundColor White
Write-Host "  4. Execute: dotnet ef migrations add InitialCreate -p src\Services\KRT.Onboarding\KRT.Onboarding.Infra.Data -s src\Services\KRT.Onboarding\KRT.Onboarding.Api" -ForegroundColor White
Write-Host "  5. Execute: dotnet run --project src\Services\KRT.Onboarding\KRT.Onboarding.Api" -ForegroundColor White
Write-Host ""
Write-Host "URLs:" -ForegroundColor Cyan
Write-Host "  - API: http://localhost:5000" -ForegroundColor White
Write-Host "  - Swagger: http://localhost:5000/swagger" -ForegroundColor White
Write-Host "  - Kafka UI: http://localhost:8080" -ForegroundColor White
Write-Host "  - RabbitMQ: http://localhost:15672 (krt_admin/KrtBank@2024)" -ForegroundColor White
Write-Host ""
