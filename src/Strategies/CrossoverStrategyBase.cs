namespace Tickblaze.Scripts.Strategies;

public abstract class CrossoverStrategyBase : BaseStopsAndTargetsStrategy
{
	[Parameter("Enable Long?")]
	public bool IsLongEnabled { get; set; } = true;

	[Parameter("Enable Short?")]
	public bool IsShortEnabled { get; set; } = true;

	protected abstract ISeries<double> FastSeries { get; }
	protected abstract ISeries<double> SlowSeries { get; }

	protected sealed override void OnBar(int index)
	{
		if (index == 0)
		{
			return;
		}

		if (FastSeries[index - 1] <= SlowSeries[index - 1] && FastSeries[index] > SlowSeries[index])
		{
			var comment = "Crossed Above";

			if (IsLongEnabled)
			{
				TryEnterMarket(OrderDirection.Long, comment);
			}
			else if (Position?.Direction is OrderDirection.Short)
			{
				ClosePosition(comment);
			}
		}
		else if (FastSeries[index - 1] >= SlowSeries[index - 1] && FastSeries[index] < SlowSeries[index])
		{
			var comment = "Crossed Below";

			if (IsShortEnabled)
			{
				TryEnterMarket(OrderDirection.Short, comment);
			}
			else if (Position?.Direction is OrderDirection.Short)
			{
				ClosePosition(comment);
			}
		}
	}
}
