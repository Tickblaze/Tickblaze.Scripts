# Configuring Watchlist Cells in an Indicator

In this tutorial, you will learn how to customize the display of watchlist cells in an indicator. By default, watchlist cells display the value of an indicator as plain text. However, you can override this behavior to display custom text and modify colors. This is particularly useful for creating visually intuitive watchlists that provide quick insights into market conditions.

We will use the `WatchlistIndicator` example provided to demonstrate how to configure watchlist cells.

---

## Step 1: Create the Indicator Class
Start by creating a new indicator class that inherits from the `Indicator` base class. This class will calculate a value based on the bar's close and open prices and display custom text and colors in the watchlist cell.

```csharp
namespace Tickblaze.Scripts.Tutorials;

public class WatchlistIndicator : Indicator
{
    [Plot("Result")] 
    public PlotSeries Result { get; set; }

    protected override void Calculate(int index)
    {
        var bar = Bars[index]!;

        // Calculate the indicator value
        Result[index] = bar.Close > bar.Open ? 1 : -1;
    }
}
```

### Explanation:
- `Result`: A `PlotSeries` property that stores the calculated values of the indicator.
- `Calculate(int index)`: This method is called for each bar to compute the indicator value. In this example, the value is `1` if the close price is greater than the open price (bullish) and `-1` otherwise (bearish).

---

## Step 2: Override `ConfigureWatchlistCell`
To customize the watchlist cell, override the `ConfigureWatchlistCell` method. This method provides a `WatchlistCellValue` object, which allows you to set the value, displayed text, background color, and foreground color.

```csharp
public override void ConfigureWatchlistCell(WatchlistCellValue cellValue)
{
    if (Result.Last() > 0)
    {
        cellValue.DisplayValue = "Bullish";
        cellValue.Background = Color.Green;
        cellValue.Foreground = Color.Black;
    }
    else
    {
        cellValue.DisplayValue = "Bearish";
        cellValue.Background = Color.Red;
        cellValue.Foreground = Color.White;
    }
}
```

### Explanation:
- `Result.Last()`: Retrieves the most recent value of the `Result` series.
- `cellValue.DisplayValue`: Sets the text displayed in the watchlist cell.
- `cellValue.Background`: Sets the background color of the cell.
- `cellValue.Foreground`: Sets the text color of the cell.

In this example:
- If the last value of `Result` is greater than `0`, the cell displays "Bullish" with a green background and black text.
- If the last value of `Result` is less than or equal to `0`, the cell displays "Bearish" with a red background and white text.

---

## Step 3: Complete Indicator Code
Here is the complete code for the `WatchlistIndicator`:

```csharp
namespace Tickblaze.Scripts.Tutorials;

public class WatchlistIndicator : Indicator
{
    [Plot("Result")] 
    public PlotSeries Result { get; set; }

    protected override void Calculate(int index)
    {
        var bar = Bars[index]!;

        // Calculate the indicator value
        Result[index] = bar.Close > bar.Open ? 1 : -1;
    }

    public override void ConfigureWatchlistCell(WatchlistCellValue cellValue)
    {
        if (Result.Last() > 0)
        {
            cellValue.DisplayValue = "Bullish";
            cellValue.Background = Color.Green;
            cellValue.Foreground = Color.Black;
        }
        else
        {
            cellValue.DisplayValue = "Bearish";
            cellValue.Background = Color.Red;
            cellValue.Foreground = Color.White;
        }
    }
}
```

---

## Step 4: Add the Indicator to a Watchlist
1. Compile the script and ensure there are no errors.
2. Open Tickblaze and create or open a watchlist.
3. Add the `WatchlistIndicator` to the watchlist as a column.
    1. Right-click and select `Script Columns`
    2. Click the `+` sign in the top right corner.
    3. Under the `Script`, select the `WatchlistIndicator`.
    4. Configure bar type, size, and source as desired.
    5. Hit `Ok`.
4. Observe the custom text and colors in the watchlist cell based on the indicator's calculation.

---

## Example Output
- If the last bar is bullish (close > open), the watchlist cell will display:
  - **Text**: "Bullish"
  - **Background**: Green
  - **Text Color**: Black

- If the last bar is bearish (close <= open), the watchlist cell will display:
  - **Text**: "Bearish"
  - **Background**: Red
  - **Text Color**: White

---

## Conclusion
By overriding the `ConfigureWatchlistCell` method, you can create highly customized and visually appealing watchlist cells that provide meaningful insights at a glance. This tutorial demonstrated how to display bullish/bearish signals with custom colors, but the possibilities are endless. Experiment with different configurations to suit your trading style and preferences.
