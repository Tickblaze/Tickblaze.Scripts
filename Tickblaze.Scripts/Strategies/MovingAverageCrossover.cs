using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

//[Browsable(false)]
public class MovingAverageCrossover : Strategy
{
	[Parameter("MA Type")]
	public MovingAverageType MovingAverageType { get; set; } = MovingAverageType.Simple;

	[Parameter("Fast Period"), NumericRange(0)]
	public int FastPeriod { get; set; } = 12;

	[Parameter("Slow Period"), NumericRange(0)]
	public int SlowPeriod { get; set; } = 26;

	[Parameter("Stop Loss %"), NumericRange(0)]
	public double StopLossPercent { get; set; } = 0;

	[Parameter("Take Profit %"), NumericRange(0)]
	public double TakeProfitPercent { get; set; } = 0;

	[Parameter("Enable Shorting")]
	public bool EnableShorting { get; set; } = true;

	[Parameter("Enable Longing")]
	public bool EnableLonging { get; set; } = true;

	private MovingAverage _fastMovingAverage, _slowMovingAverage;
	private Series<bool> _isBullishTrend;

	public MovingAverageCrossover()
	{
		Name = "Moving Average Crossover";
		Description = "The Moving Average Crossover Strategy detects trends by tracking crossovers between fast and slow moving averages. A bullish crossover triggers a buy order, while a bearish crossover triggers a sell order, aiming to capture early trend changes.";
	}

	protected override void Initialize()
	{
		_fastMovingAverage = new MovingAverage(Bars.Close, FastPeriod, MovingAverageType)
		{
			ShowOnChart = true
		};

		_slowMovingAverage = new MovingAverage(Bars.Close, SlowPeriod, MovingAverageType)
		{
			ShowOnChart = true
		};

		_isBullishTrend = new Series<bool>();
	}

	protected override void OnBar(int index)
	{
		var fastMovingAverage = _fastMovingAverage[index];
		var slowMovingAverage = _slowMovingAverage[index];

		if (fastMovingAverage > slowMovingAverage)
		{
			_isBullishTrend[index] = true;
		}
		else if (fastMovingAverage < slowMovingAverage)
		{
			_isBullishTrend[index] = false;
		}
		else if (index > 0)
		{
			_isBullishTrend[index] = _isBullishTrend[index - 1];
		}

		if (index == 0)
		{
			return;
		}

		if (_isBullishTrend[index] != !_isBullishTrend[index - 1])
		{
			return;
		}

		var orderDirection = _isBullishTrend[index] ? OrderDirection.Long : OrderDirection.Short;

		// If take profits are enabled, they handle the exits exclusively
		if (Position != null)
		{
			if (orderDirection == Position.Direction || TakeProfitPercent > 0)
			{
				return;
			}

			ClosePosition();
		}

		if (orderDirection == OrderDirection.Long ? !EnableLonging : !EnableShorting)
		{
			return;
		}

		var order = ExecuteMarketOrder(orderDirection == OrderDirection.Long ? OrderAction.Buy : OrderAction.Sell, 1);
		if (StopLossPercent > 0)
		{
			var stopLossPercentOfPrice = orderDirection == OrderDirection.Long ? 1 - StopLossPercent / 100 : 1 + StopLossPercent / 100;
			SetStopLoss(order, Math.Max(0.001, Bars[index].Close * stopLossPercentOfPrice));
		}

		if (TakeProfitPercent > 0)
		{
			var takeProfitPercentOfPrice = orderDirection == OrderDirection.Long ? 1 + TakeProfitPercent / 100 : 1 - TakeProfitPercent / 100;
			SetTakeProfit(order, Math.Max(0.001, Bars[index].Close * takeProfitPercentOfPrice));
		}
	}
}