using System;
using System.Globalization;
using Xunit;
using BluetoothAudioReceiver;

namespace BluetoothAudioReceiver.Tests;

public class SliderFillConverterTests
{
    private readonly SliderFillConverter _converter = new SliderFillConverter();

    [Fact]
    public void Convert_ReturnsCorrectWidth_WhenValuesAreValid()
    {
        // Arrange
        double value = 50.0;
        double maximum = 100.0;
        double width = 200.0;
        object[] values = new object[] { value, maximum, width };
        double expected = 100.0; // (50/100) * 200

        // Act
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(expected, (double)result);
    }

    [Fact]
    public void Convert_ReturnsZero_WhenMaximumIsZero()
    {
        // Arrange
        double value = 50.0;
        double maximum = 0.0;
        double width = 200.0;
        object[] values = new object[] { value, maximum, width };

        // Act
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Convert_ReturnsZero_WhenMaximumIsNegative()
    {
        // Arrange
        double value = 50.0;
        double maximum = -10.0;
        double width = 200.0;
        object[] values = new object[] { value, maximum, width };

        // Act
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Convert_ReturnsZero_WhenValuesAreMissing()
    {
        // Arrange
        object[] values = new object[] { 50.0, 100.0 }; // Missing width

        // Act
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Convert_ReturnsZero_WhenInputTypesAreWrong()
    {
        // Arrange
        object[] values = new object[] { "invalid", 100.0, 200.0 };

        // Act
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(0.0, result);
    }
}
