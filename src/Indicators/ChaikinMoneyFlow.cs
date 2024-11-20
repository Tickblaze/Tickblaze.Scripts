namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Chaikin Money Flow [CMF]
/// </summary>
public partial class ChaikinMoneyFlow : Indicator
{
	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 21;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, PlotStyle.Histogram);

	public ChaikinMoneyFlow()
	{
		Name = "Chaikin Money Flow";
		ShortName = "CMF";
	}

	protected override void Calculate(int index)
	{
		if (index < Period)
		{
			return;
		}

		var volumeMultipliedSum = 0.0;
		var volumeSum = 0.0;

		for (var i = 0; i < Period; i++)
		{
			var bar = Bars[index - 1];
			var range = bar.High - bar.Low;

			if (range > 0)
			{
				volumeMultipliedSum += (bar.Close - bar.Low - (bar.High - bar.Close)) / range * bar.Volume;
			}

			volumeSum += bar.Volume;
		}

		Result[index] = volumeMultipliedSum / volumeSum;
	}
}
