using FluentAssertions;
using KRT.Payments.Api.Services;
using Xunit;

namespace KRT.UnitTests.Services;

public class BRCodeParserTests
{
    private readonly BRCodeParser _parser = new();

    // BRCode de exemplo: aurea@krtbank.com.br, R$15.00, AUREA Maison, Sao Paulo
    // Tag 26 content: "0014BR.GOV.BCB.PIX0120aurea@krtbank.com.br" = 42 chars
    private const string ValidBRCode =
        "00020126420014BR.GOV.BCB.PIX0120aurea@krtbank.com.br520400005303986540515.005802BR5912AUREA Maison6009Sao Paulo62070503***63040000";

    [Fact]
    public void Parse_ValidBRCode_ShouldExtractAmount()
    {
        var result = _parser.Parse(ValidBRCode);

        result.Amount.Should().Be(15.00m);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Parse_ValidBRCode_ShouldExtractMerchantName()
    {
        var result = _parser.Parse(ValidBRCode);

        result.MerchantName.Should().Be("AUREA Maison");
    }

    [Fact]
    public void Parse_ValidBRCode_ShouldExtractPixKey()
    {
        var result = _parser.Parse(ValidBRCode);

        result.PixKey.Should().Be("aurea@krtbank.com.br");
    }

    [Fact]
    public void Parse_ValidBRCode_ShouldExtractMerchantCity()
    {
        var result = _parser.Parse(ValidBRCode);

        result.MerchantCity.Should().Be("Sao Paulo");
    }

    [Fact]
    public void Parse_ValidBRCode_ShouldExtractTxId()
    {
        var result = _parser.Parse(ValidBRCode);

        result.TxId.Should().Be("***");
    }

    [Fact]
    public void Parse_EmptyString_ShouldReturnInvalidData()
    {
        var result = _parser.Parse("");

        result.IsValid.Should().BeFalse();
        result.PixKey.Should().BeEmpty();
        result.Amount.Should().Be(0);
    }

    [Fact]
    public void Parse_NullString_ShouldReturnInvalidData()
    {
        var result = _parser.Parse(null!);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Parse_InvalidFormat_ShouldNotThrow()
    {
        var result = _parser.Parse("xyzabc12345garbage");

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Parse_MissingBRGovTag_ShouldReturnInvalid()
    {
        // Tag 26 without BR.GOV.BCB.PIX GUI
        var brcode = "000201260C0008INVALID01041234520400005303986540510.005802BR5908TestName6008TestCity63040000";
        var result = _parser.Parse(brcode);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Parse_QrCodeServicePayload_ShouldParse()
    {
        // Simulate what QrCodeService.GeneratePixPayload produces
        var pixKey = "aurea@krtbank.com.br";
        var merchantName = "AUREA Maison";
        var city = "Sao Paulo";
        var amount = 1500.00m;
        var txId = "abc123def456ghi789012345x";

        var amountStr = amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        var tag26Content = $"0014BR.GOV.BCB.PIX01{pixKey.Length:D2}{pixKey}";
        var tag62Content = $"05{txId.Length:D2}{txId}";

        var payload = "000201" +
            $"26{tag26Content.Length:D2}{tag26Content}" +
            "52040000" +
            "5303986" +
            $"54{amountStr.Length:D2}{amountStr}" +
            "5802BR" +
            $"59{merchantName.Length:D2}{merchantName}" +
            $"60{city.Length:D2}{city}" +
            $"62{tag62Content.Length:D2}{tag62Content}" +
            "63040000";

        var result = _parser.Parse(payload);

        result.IsValid.Should().BeTrue();
        result.PixKey.Should().Be(pixKey);
        result.Amount.Should().Be(amount);
        result.MerchantName.Should().Be(merchantName);
        result.MerchantCity.Should().Be(city);
        result.TxId.Should().Be(txId);
    }
}
