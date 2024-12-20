namespace Tickblaze.Scripts.Strategies;

public abstract class OverboughtOversoldStrategyBase : BaseStopsAndTargetsStrategy
{
	[Parameter("Overbought Level")]
	public double OverboughtLevel { get; set; } = 70;

	[Parameter("Oversold Level")]
	public double OversoldLevel { get; set; } = 30;

	[Parameter("Enable Long?")]
	public bool IsLongEnabled { get; set; } = true;

	[Parameter("Enable Short?")]
	public bool IsShortEnabled { get; set; } = true;

	protected abstract ISeries<double> Series { get; }

	protected sealed override void OnBar(int index)
	{
		if (index == 0)
		{
			return;
		}

		if (Series[index - 1] >= OverboughtLevel && Series[index] < OverboughtLevel)
		{
			var comment = "Overbought";

			if (IsShortEnabled)
			{
				TryEnterMarket(OrderDirection.Short, comment);
			}
			else if (Position?.Direction is OrderDirection.Short)
			{
				ClosePosition(comment);
			}
		}
		else if (Series[index - 1] <= OversoldLevel && Series[index] > OversoldLevel)
		{
			var comment = "Oversold";

			if (IsLongEnabled)
			{
				TryEnterMarket(OrderDirection.Long, comment);
			}
			else if (Position?.Direction is OrderDirection.Short)
			{
				ClosePosition(comment);
			}
		}
	}
}