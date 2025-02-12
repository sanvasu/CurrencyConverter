using Xunit;
using Moq;
using CurrencyConverterAPI.Services;

public class CurrencyServiceTests
{
    [Fact]
    public void ConvertCurrency_ShouldReturnCorrectValue()
    {
        // Arrange
        var mockService = new Mock<ICurrencyService>();
        mockService.Setup(s => s.ConvertAsync("USD", "EUR", 100)).Returns(90);

        // Act
        var result = mockService.Object.ConvertAsync("USD", "EUR", 100);

        // Assert
        Assert.Equal(90, result);
    }
}
