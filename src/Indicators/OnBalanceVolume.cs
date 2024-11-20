namespace Tickblaze.Scripts.Indicators;

public sealed partial class OnBalanceVolume : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Plot("Result")]
	public PlotSeries Result { get; set; }

	public OnBalanceVolume()
	{
		Name = "On Balance Volume";
		ShortName = "OBV";
	}

	protected override void Calculate(int index)
	{
		if (index == 0)
		{
			Result[index] = 0;
		}
		else
		{
			var current = Source[index];
			var previous = Source[index - 1];

			if (current > previous)
			{
				Result[index] = Result[index - 1] + Bars.Volume[index];
			}
			else if (current < previous)
			{
				Result[index] = Result[index - 1] - Bars.Volume[index];
			}
			else
			{
				Result[index] = Result[index - 1];
			}
		}
	}
}
