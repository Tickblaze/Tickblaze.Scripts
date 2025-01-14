using Tickblaze.Scripts.Api.Adapters;
using Tickblaze.Scripts.Api.Interfaces.Orders;
using Tickblaze.Scripts.Extensions;

namespace Tickblaze.Scripts.Strategies;

public abstract class BaseStopsAndTargetsStrategy : Strategy
{
	private const string GroupName = "Risk Management";

	[Parameter("Stop Loss Type", GroupName = GroupName)]
	public StopTargetDistanceType StopLossType { get; set; } = StopTargetDistanceType.Dollars;

	[Parameter("Stop Loss", GroupName = GroupName), NumericRange(0)]
	public double StopLoss { get; set; } = 0;

	[Parameter("Take Profit Type", GroupName = GroupName)]
	public StopTargetDistanceType TakeProfitType { get; set; } = StopTargetDistanceType.Dollars;

	[Parameter("Take Profit", GroupName = GroupName), NumericRange(0)]
	public double TakeProfit { get; set; } = 0;

	[Parameter("Position Sizing Strategy", GroupName = GroupName)]
	public SizingStrategy SizingStrategy { get; set; } = SizingStrategy.FixedQuantity;

	[Parameter("Position Sizing Input", GroupName = GroupName), NumericRange(0.00001)]
	public double SizingStrategyInput { get; set; } = 1;

	protected void PlaceStopLossAndTarget(IOrder order, double entryPrice, OrderDirection orderDirection)
	{
		if (StopLoss > 0)
		{
			switch (StopLossType)
			{
				case StopTargetDistanceType.Dollars:
					SetStopLossTicks(order, (int)Math.Max(StopLoss / (Symbol.TickValue * order.Quantity), 1), "SL");
					break;
				case StopTargetDistanceType.DollarsPerQuantity:
					SetStopLossTicks(order, (int)Math.Max(StopLoss / Symbol.TickValue, 1), "SL");
					break;
				case StopTargetDistanceType.Ticks:
					SetStopLossTicks(order, (int)Math.Max(StopLoss, 1), "SL");
					break;
				case StopTargetDistanceType.Points:
					SetStopLossTicks(order, (int)Math.Max(StopLoss * Symbol.TicksPerPoint, 1), "SL");
					break;
				case StopTargetDistanceType.PercentOfPrice:
					SetStopLossPercent(order, Math.Max(StopLoss, 0.0001), "SL");
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		if (TakeProfit > 0)
		{
			switch (TakeProfitType)
			{
				case StopTargetDistanceType.Dollars:
					SetTakeProfitTicks(order, (int)Math.Max(TakeProfit / (Symbol.TickValue * order.Quantity), 1), "TP");
					break;
				case StopTargetDistanceType.DollarsPerQuantity:
					SetTakeProfitTicks(order, (int)Math.Max(TakeProfit / Symbol.TickValue, 1), "TP");
					break;
				case StopTargetDistanceType.Ticks:
					SetTakeProfitTicks(order, (int)Math.Max(TakeProfit, 1), "TP");
					break;
				case StopTargetDistanceType.Points:
					SetTakeProfitTicks(order, (int)Math.Max(TakeProfit * Symbol.TicksPerPoint, 1), "TP");
					break;
				case StopTargetDistanceType.PercentOfPrice:
					SetTakeProfitPercent(order, Math.Max(TakeProfit, 0.0001), "TP");
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}

	protected override Parameters GetParameters(Parameters parameters)
	{
		if (StopLoss == 0 && SizingStrategy != SizingStrategy.FixedQuantity)
		{
			Alert?.ShowDialog(AlertType.Bad, "Stop loss required for equity sizing strategies. Reverting to fixed quantity sizing strategy.");
			parameters[nameof(SizingStrategy)].Value = SizingStrategy.FixedQuantity;
			parameters[nameof(SizingStrategyInput)].Value = 1;
		}

		if (StopLossType == StopTargetDistanceType.Dollars && SizingStrategy != SizingStrategy.FixedQuantity)
		{
			Alert?.ShowDialog(AlertType.Bad, "\"$\" stop loss type is incompatible with equity sizing strategies. Reverting to fixed quantity sizing strategy.");
			parameters[nameof(SizingStrategy)].Value = SizingStrategy.FixedQuantity;
			parameters[nameof(SizingStrategyInput)].Value = 1;
		}

		parameters[nameof(SizingStrategyInput)].Attributes.Name = SizingStrategy switch
		{
			SizingStrategy.FixedQuantity => "# Per Trade",
			SizingStrategy.FixedEquity => "$ Risk Per Trade",
			SizingStrategy.FixedFractional => "% Account Risk Per Trade",
			_ => throw new ArgumentOutOfRangeException()
		};

		return parameters;
	}

	protected double GetTradeQuantity(double entryPrice)
	{
		double entryQuantity;
		if (SizingStrategy == SizingStrategy.FixedQuantity)
		{
			entryQuantity = SizingStrategyInput;
		}
		else
		{
			var riskPerContract = GetStopLossDist(entryPrice) * Symbol.PointValue * GetExchangeRate(Symbol.CurrencyCode, Account.BaseCurrencyCode);
			var totalRisk = SizingStrategy == SizingStrategy.FixedEquity
				? SizingStrategyInput
				: SizingStrategyInput / 100 * Account.Equity;
			entryQuantity = totalRisk / riskPerContract;
		}

		entryQuantity = entryQuantity.FloorToNearestMultiple((double)Symbol.MinimumVolume);
		var exitQuantity = Position?.Quantity ?? 0;
		return exitQuantity + entryQuantity;
	}

	private double GetStopLossDist(double price)
	{
		return StopLossType switch
		{
			StopTargetDistanceType.Dollars or StopTargetDistanceType.DollarsPerQuantity => Math.Max(StopLoss / Symbol.PointValue, Symbol.TickSize),
			StopTargetDistanceType.Ticks => StopLoss * Symbol.TickSize,
			StopTargetDistanceType.Points => StopLoss * Symbol.PointSize,
			StopTargetDistanceType.PercentOfPrice => (price * StopLoss / 100).CielToNearestMultiple(Symbol.TickSize),
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	protected void TryEnterMarket(OrderDirection direction, string comment = "")
	{
		if (Position != null && Position.Direction == direction && Position.Quantity != 0)
		{
			return;
		}

		var action = direction is OrderDirection.Long ? OrderAction.Buy : OrderAction.SellShort;
		var quantity = GetTradeQuantity(Bars.Close[^1]);
		var marketOrder = ExecuteMarketOrder(action, quantity, TimeInForce.GoodTillCancel, comment);
		PlaceStopLossAndTarget(marketOrder, Bars.Close[^1], direction);
	}
}

public enum SizingStrategy
{
	[DisplayName("Fixed Quantity")]
	FixedQuantity,
	[DisplayName("Fixed Equity")]
	FixedEquity,
	[DisplayName("Fixed Fractional")]
	FixedFractional
}

public enum StopTargetDistanceType
{
	[DisplayName("$")]
	Dollars,
	[DisplayName("$/Unit")]
	DollarsPerQuantity,
	Ticks,
	Points,
	[DisplayName("% of Price")]
	PercentOfPrice
}