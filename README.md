# Getting Started

Follow these steps to set up your environment and create your first custom indicator for Tickblaze.

### 1. Create a New Class Library

Create a new class class library targeting .NET 9.0:

```
dotnet new classlib -n CustomIndicator -f net9.0
```

And navigate to its directory:

```
cd CustomIndicator
```

### 2. Add the Tickblaze.Scripts NuGet Package

Add the Tickblaze.Scripts.Api NuGet package to your project:

```
dotnet add package Tickblaze.Scripts.Api --version *
```

The `--version *` ensures you always get the latest version.

### 3. Create Your Indicator

Open the _Class1.cs_ file in your project, and replace its contents with the following code:

```cs
namespace CustomIndicator;

/// <summary>
/// A custom indicator that calculates the typical price for a bar.
/// </summary>
public partial class TypicalPrice : Indicator
{
    [Plot("Result")]
    public PlotSeries Result { get; set; } = new(Color.Blue, LineStyle.Solid, 1);

    public TypicalPrice()
    {
        Name = "Typical Price";
        IsOverlay = true;
    }

    /// <summary>
    /// Calculates the typical price for the given bar.
    /// </summary>
    protected override void Calculate(int index)
    {
        var bar = Bars[index];

        Result[index] = (bar.High + bar.Low + bar.Close) / 3;
    }
}
```

This code defines a new indicator called **Typical Price** that calculates the average of a bar's high, low, and close prices.

### 4. Build Your Project

Build your project to compile the indicator:

```
dotnet build
```

This will generate the necessary output files and prepare your indicator for use in Tickblaze.

### 5. Attach the Indicator to Tickblaze

After building your project, the indicator should be automatically imported into Tickblaze. To use it:

1. **Right-click** on the chart in Tickblaze.
2. Select `Indicators` and click `Add / Edit Settings`.
3. From the dropdown menu, select `Typical Price`.
4. Click the down arrow to add it to the chart.
5. Click the `OK` button.

## How to Debug

Debugging your custom indicators and strategies is straightforward:

1. **Run Tickblaze** as usual.
2. **Attach your debugger** to the `Tickblaze.View.exe` process:
   - Open your IDE (e.g., Visual Studio or Rider).
   - Attach to the `Tickblaze.View.exe` process.
3. Set breakpoints in your code to inspect values or troubleshoot issues.

## Additional Resources

This repository includes several examples:

- [Bar Types](./src/BarTypes/)
- [Drawings](./src/Drawings/)
- [Indicators](./src/Indicators/)
- [Position Sizers](./src/PositionSizers/)
- [Strategies](./src/Strategies/)
- [Trade Management Strategies](./src/TradeManagementStrategies/)

## API Reference
For detailed documentation on the Tickblaze scripting API, visit the official [API Reference Page](https://tickblaze.github.io/Tickblaze.Scripts/).
