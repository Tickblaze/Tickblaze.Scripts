using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

public class RsiOverboughtOversold : Strategy
{
	[NumericRange(1, int.MaxValue)]
	[Parameter("RSI Period")]
	public int RsiPeriod { get; set; } = 14;

	[NumericRange(0, 100)]
	[Parameter("RSI Overbought")]
	public int RsiOverboughtValue { get; set; } = 70;

	[NumericRange(0, 100)]
	[Parameter("RSI Oversold")]
	public int RsiOversoldValue { get; set; } = 30;

	[NumericRange(0, int.MaxValue)]
	[Parameter("Stop-loss Ticks")]
	public int StopLossTicks { get; set; } = 10;

	[NumericRange(0, int.MaxValue)]
	[Parameter("Take-profit Ticks")]
	public int TakeProfitTicks { get; set; } = 10;

	[Parameter("Short enabled?")]
	public bool IsShortEnabled { get; set; } = true;

	[Parameter("Long enabled?")]
	public bool IsLongEnabled { get; set; } = true;

	[Parameter("Reverse on opposite signal?")]
	public bool IsReverseOnOppositeSignalEnabled { get; set; } = true;

	private RelativeStrengthIndex _rsi;

	public RsiOverboughtOversold()
	{
		Name = "RSI Overbought/Oversold";
		Description = "A reversal strategy based on the Relative Strength Index (RSI) generating a long signal when the RSI falls below the oversold level and a short signal when it rises above the overbought level.";
	}

	protected override void Initialize()
	{
		_rsi = new RelativeStrengthIndex(Bars.Close, RsiPeriod, MovingAverageType.Simple, 1);
		_rsi.Average.IsVisible = false;
		_rsi.OverboughtLevel.Value = RsiOverboughtValue;
		_rsi.OversoldLevel.Value = RsiOversoldValue;
		_rsi.ShowOnChart = true;
	}

	protected override void OnBar(int index)
	{
		var rsi = _rsi.Result[index];
		if (rsi < RsiOversoldValue)
		{
			var comment = "Oversold";

			if (Position?.Direction is OrderDirection.Short)
			{
				ClosePosition(comment);
			}

			if (IsLongEnabled && Position is null && Orders.Count == 0)
			{
				var price = Bars.Close[index];
				var stopLoss = StopLossTicks > 0 ? price - StopLossTicks * Symbol.TickSize : (double?)null;
				var takeProfit = TakeProfitTicks > 0 ? price + TakeProfitTicks * Symbol.TickSize : (double?)null;

				EnterMarket(OrderDirection.Long, stopLoss, takeProfit);
			}
		}
		else if (rsi > RsiOverboughtValue)
		{
			var comment = "Overbought";

			if (Position?.Direction is OrderDirection.Long)
			{
				ClosePosition(comment);
			}

			if (IsShortEnabled && Position is null && Orders.Count == 0)
			{
				var price = Bars.Close[index];
				var stopLoss = StopLossTicks > 0 ? price + StopLossTicks * Symbol.TickSize : (double?)null;
				var takeProfit = TakeProfitTicks > 0 ? price - TakeProfitTicks * Symbol.TickSize : (double?)null;

				EnterMarket(OrderDirection.Short, stopLoss, takeProfit);
			}
		}
	}

	private void EnterMarket(OrderDirection direction, double? stopLoss, double? takeProfit)
	{
		var action = direction is OrderDirection.Long ? OrderAction.Buy : OrderAction.SellShort;
		var marketOrder = ExecuteMarketOrder(action, 1);

		if (stopLoss.HasValue)
		{
			SetStopLoss(marketOrder, stopLoss.Value);
		}

		if (takeProfit.HasValue)
		{
			SetTakeProfit(marketOrder, takeProfit.Value);
		}
	}
}
