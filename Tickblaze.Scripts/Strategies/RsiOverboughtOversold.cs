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
	[Parameter("Stop-loss %")]
	public double StopLossPercent { get; set; } = 10;

	[NumericRange(0, int.MaxValue)]
	[Parameter("Take-profit %")]
	public double TakeProfitPercent { get; set; } = 10;

	[Parameter("Short enabled?")]
	public bool IsShortEnabled { get; set; } = true;

	[Parameter("Long enabled?")]
	public bool IsLongEnabled { get; set; } = true;

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
		if (index == 0)
		{
			return;
		}

		var rsi = new[] { _rsi.Result[index], _rsi.Result[index - 1] };
		if (rsi[1] >= RsiOversoldValue && RsiOversoldValue > rsi[0])
		{
			var comment = "Overbought";

			if (IsLongEnabled)
			{
				EnterMarket(OrderDirection.Long, comment);
			}
			else if (Position?.Direction is OrderDirection.Short)
			{
				ClosePosition(comment);
			}
		}
		else if (rsi[1] <= RsiOverboughtValue && RsiOverboughtValue < rsi[0])
		{
			var comment = "Overbought";

			if (IsShortEnabled)
			{
				EnterMarket(OrderDirection.Short, comment);
			}
			else if (Position?.Direction is OrderDirection.Long)
			{
				ClosePosition(comment);
			}
		}
	}

	private void EnterMarket(OrderDirection direction, string comment = "")
	{
		if (Position?.Direction == direction)
		{
			return;
		}

		var price = Bars.Close[^1];
		var action = direction is OrderDirection.Long ? OrderAction.Buy : OrderAction.SellShort;
		var exitMultiplier = direction is OrderDirection.Long ? 1 : -1;
		var quantity = 1 + (Position?.Quantity ?? 0);
		var marketOrder = ExecuteMarketOrder(action, quantity, TimeInForce.GoodTillCancel, comment);

		if (StopLossPercent > 0)
		{
			var stopLossOffset = price * (StopLossPercent / 100);
			var stopLoss = price - stopLossOffset * exitMultiplier;

			SetStopLoss(marketOrder, stopLoss, "SL");
		}

		if (TakeProfitPercent > 0)
		{
			var takeProfitOffset = price * (TakeProfitPercent / 100);
			var takeProfit = price + takeProfitOffset * exitMultiplier;

			SetTakeProfit(marketOrder, takeProfit, "TP");
		}
	}
}
