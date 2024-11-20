namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// AroonOscillator [ARNO]
/// </summary>
public partial class AroonOscillator : Indicator
{
	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, PlotStyle.Line);

	[Plot("Overbought")]
	public PlotLevel OvoerboughtLevel { get; set; } = new(70, Color.Red, LineStyle.Dash);

	[Plot("Zero")]
	public PlotLevel ZeroLevel { get; set; } = new(0, Color.Gray, LineStyle.Dash);

	[Plot("Oversold")]
	public PlotLevel OversoldLevel { get; set; } = new(-70, Color.Green, LineStyle.Dash);

	public AroonOscillator()
	{
		Name = "Aroon Oscillator";
		ShortName = "ARNO";
	}

	protected override void Calculate(int index)
	{
		if (index < Period)
		{
			Result[index] = 0;
		}
		else
		{
			var back = Math.Min(Period, index);
			var idxMax = -1;
			var idxMin = -1;
			var max = double.MinValue;
			var min = double.MaxValue;

			for (var idx = back; idx >= 0; idx--)
			{
				var i = index - back + idx;
				if (Bars[i].High >= max)
				{
					max = Bars[i].High;
					idxMax = i;
				}

				if (Bars[i].Low <= min)
				{
					min = Bars[i].Low;
					idxMin = i;
				}
			}

			Result[index] = 100 * ((double)(back - (index - idxMax)) / back) - 100 * ((double)(back - (index - idxMin)) / back);
		}
	}
}
