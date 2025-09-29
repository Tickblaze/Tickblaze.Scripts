# Tutorial: Accessing Real-Time Level 2 (L2) Market Data in Tickblaze

This tutorial shows how to access and visualize real-time market depth (order book) data in Tickblaze.

## Step 1: Create Basic DOM Indicator Structure

```csharp
public partial class DomIndicator : Indicator
{
    private Tickblaze.Core.Data.IDom _depthOfMarket;

    protected override void Initialize()
    {
        // Get the DOM (Depth of Market) interface
        _depthOfMarket = GetDom();
        
        // Subscribe to L2 updates
        GetL2QuoteUpdates().Start(update => _depthOfMarket.ApplyUpdate(update));
    }
}
```

## Step 2: Visualizing the Order Book

```csharp
public override void OnRender(IDrawingContext context)
{
    // Get current price levels
    var bids = _depthOfMarket.GetPriceLevels(L2TickType.Bid);
    var asks = _depthOfMarket.GetPriceLevels(L2TickType.Ask);
    
    // Get visible price range
    var minPrice = ChartScale.MinPrice;
    var maxPrice = ChartScale.MaxPrice;

    // Freeze collections for thread-safe access
    using var bidReleaser = bids.Freeze();
    using var askReleaser = asks.Freeze();

    // Draw bid levels (blue)
    foreach (var bid in bids.Enumerate(minPrice, maxPrice))
    {
        DrawPriceLevel(context, bid, Color.Blue);
    }

    // Draw ask levels (red)
    foreach (var ask in asks.Enumerate(minPrice, maxPrice))
    {
        DrawPriceLevel(context, ask, Color.Red);
    }
}

private void DrawPriceLevel(IDrawingContext context, L2PriceLevel level, Color color)
{
	var y = ChartScale.GetYCoordinateByValue(level.Price);
	var width = level.Size;
	var pointA = new Point(Chart.Width - width, y);
	var pointB = new Point(Chart.Width, y);

	context.DrawLine(pointA, pointB, color, 2);
}
```

This implementation provides a real-time visualization of market depth that updates automatically as new L2 data arrives.

## Key Concepts Explained

1. **`IDom` Interface**:
   - Provides access to the order book
   - Updated in real-time via `GetL2QuoteUpdates()`

2. **Price Levels**:
   - `GetPriceLevels()` returns current bids/asks
   - `Enumerate()` filters levels within visible price range

3. **Thread Safety**:
   - `Freeze()` locks the collection during rendering
   - Required for thread-safe access to real-time data
