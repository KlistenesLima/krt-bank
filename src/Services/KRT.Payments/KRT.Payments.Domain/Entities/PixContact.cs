namespace KRT.Payments.Domain.Entities;

public class PixContact
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public string Name { get; private set; } = "";
    public string PixKey { get; private set; } = "";
    public string PixKeyType { get; private set; } = "CPF";
    public string? BankName { get; private set; }
    public string? Nickname { get; private set; }
    public bool IsFavorite { get; private set; }
    public int TransferCount { get; private set; }
    public DateTime? LastTransferAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private PixContact() { }

    public static PixContact Create(Guid accountId, string name, string pixKey, string pixKeyType, string? bankName = null, string? nickname = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Nome e obrigatorio");
        if (string.IsNullOrWhiteSpace(pixKey)) throw new ArgumentException("Chave Pix e obrigatoria");

        return new PixContact
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Name = name.Trim(),
            PixKey = pixKey.Trim(),
            PixKeyType = pixKeyType,
            BankName = bankName,
            Nickname = nickname,
            IsFavorite = false,
            TransferCount = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? nickname, string? bankName)
    {
        if (!string.IsNullOrWhiteSpace(name)) Name = name.Trim();
        Nickname = nickname;
        BankName = bankName;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ToggleFavorite()
    {
        IsFavorite = !IsFavorite;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordTransfer()
    {
        TransferCount++;
        LastTransferAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public string GetDisplayName() => string.IsNullOrEmpty(Nickname) ? Name : $"{Nickname} ({Name})";
}