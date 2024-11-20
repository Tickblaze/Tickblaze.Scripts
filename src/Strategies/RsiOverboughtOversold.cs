using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

public class RsiOverboughtOversold : BaseStopsAndTargetsStrategy
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

	[Parameter("Short enabled?")]
	public bool IsShortEnabled { get; set; } = true;

	[Parameter("Long enabled?")]
	public bool IsLongEnabled { get; set; } = true;

	private RelativeStrengthIndex _rsi;

	private bool firstBar = true;

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
		if (firstBar)
		{
			firstBar = false;
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

		var action = direction is OrderDirection.Long ? OrderAction.Buy : OrderAction.SellShort;
		var quantity = 1 + (Position?.Quantity ?? 0);
		var marketOrder = ExecuteMarketOrder(action, quantity, TimeInForce.GoodTillCancel, comment);
        PlaceStopLossAndTarget(marketOrder, Bars.Close[^1], direction);

    }
}
