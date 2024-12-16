using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

public class MovingAverageCrossover : BaseStopsAndTargetsStrategy
{
	[Parameter("MA Type")]
	public MovingAverageType MovingAverageType { get; set; } = MovingAverageType.Simple;

	[Parameter("Fast Period"), NumericRange(0)]
	public int FastPeriod { get; set; } = 12;

	[Parameter("Slow Period"), NumericRange(0)]
	public int SlowPeriod { get; set; } = 26;

	[Parameter("Enable Shorting")]
	public bool EnableShorting { get; set; } = true;

	[Parameter("Enable Longing")]
	public bool EnableLonging { get; set; } = true;

	private MovingAverage _fastMovingAverage, _slowMovingAverage;
	private Series<bool?> _isBullishTrend;
	private bool _firstBar = true;

	public MovingAverageCrossover()
	{
		Name = "MA Crossover";
		Description = "The Moving Average Crossover Strategy detects trends by tracking crossovers between fast and slow moving averages. A bullish crossover triggers a buy order, while a bearish crossover triggers a sell order, aiming to capture early trend changes.";
	}

	protected override void Initialize()
	{
		_fastMovingAverage = new MovingAverage(Bars.Close, FastPeriod, MovingAverageType) { ShowOnChart = true };
		_fastMovingAverage.Result.Color = Color.Blue;

		_slowMovingAverage = new MovingAverage(Bars.Close, SlowPeriod, MovingAverageType) { ShowOnChart = true };
		_slowMovingAverage.Result.Color = Color.Green;

		_isBullishTrend = new Series<bool?>();
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
		else
		{
			_isBullishTrend[index] = index == 0 ? null : _isBullishTrend[index - 1];
		}

		if (index == 0 || _isBullishTrend[index] == _isBullishTrend[index - 1] || _isBullishTrend[index - 1] == null)
		{
			return;
		}

		var orderDirection = _isBullishTrend[index]!.Value ? OrderDirection.Long : OrderDirection.Short;
		var quantity = 1d;

		// If take profits are enabled, they handle the exits exclusively
		if (Position != null)
		{
			if (orderDirection == Position.Direction || TakeProfit > 0)
			{
				return;
			}

			quantity = Position.Quantity * 2;
		}

		if (orderDirection == OrderDirection.Long ? !EnableLonging : !EnableShorting)
		{
			return;
		}

		var order = ExecuteMarketOrder(orderDirection == OrderDirection.Long ? OrderAction.Buy : OrderAction.Sell, quantity);
		PlaceStopLossAndTarget(order, Bars.Close[^1], orderDirection);
	}
}