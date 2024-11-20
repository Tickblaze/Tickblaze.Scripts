using Tickblaze.Scripts.Api.Interfaces.Orders;

namespace Tickblaze.Scripts.Strategies;

public abstract class BaseStopsAndTargetsStrategy : Strategy
{
    [Parameter("Stop Loss Type")]
    public StopTargetDistanceType StopLossType { get; set; } = StopTargetDistanceType.DollarsPerQuantity;

    [Parameter("Stop Loss"), NumericRange(0)]
    public double StopLoss { get; set; } = 0;

    [Parameter("Take Profit Type")]
    public StopTargetDistanceType TakeProfitType { get; set; } = StopTargetDistanceType.DollarsPerQuantity;

    [Parameter("Take Profit"), NumericRange(0)]
    public double TakeProfit { get; set; } = 0;

    protected double GetStopOrTargetDistanceMultiplier(double quantity, StopTargetDistanceType distanceType, int bar)
    {
        return distanceType switch
        {
            StopTargetDistanceType.Dollars => Symbol.PointSize / (Symbol.PointValue * quantity),
            StopTargetDistanceType.DollarsPerQuantity => Symbol.PointSize / Symbol.PointValue,
            StopTargetDistanceType.Ticks => Symbol.TickSize,
            StopTargetDistanceType.PercentOfPrice => Bars.Close[bar] / 100
        };
    }

    protected void PlaceStopLossAndTarget(IOrder order, double entryPrice, OrderDirection orderDirection)
    {
        var exitMultiplier = orderDirection == OrderDirection.Long ? 1 : -1;

        if (StopLoss > 0)
        {
            var stopLossPrice = entryPrice - StopLoss * GetStopOrTargetDistanceMultiplier(order.Quantity, StopLossType, Bars.Count - 1) * exitMultiplier;
            SetStopLoss(order, Math.Max(0, stopLossPrice), "SL");
        }

        if (TakeProfit > 0)
        {
            var takeProfitPrice = entryPrice + TakeProfit * GetStopOrTargetDistanceMultiplier(order.Quantity, TakeProfitType, Bars.Count - 1) * exitMultiplier;
            SetTakeProfit(order, Math.Max(0, takeProfitPrice), "TP");
        }
    }
}

public enum StopTargetDistanceType
{
    Dollars,
    DollarsPerQuantity,
    Ticks,
    PercentOfPrice
}