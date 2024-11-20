namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Dynamic Momentum Index [DMI]
/// </summary>
public partial class DynamicMomentumIndex : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, LineStyle.Solid);

	[Plot("Overbought")]
	public PlotLevel OverboughtLevel { get; set; } = new(70, Color.Red, LineStyle.Dash, 1);

	[Plot("Oversold")]
	public PlotLevel OversoldLevel { get; set; } = new(30, Color.Green, LineStyle.Dash, 1);

	private double _smoothFactor;

	public DynamicMomentumIndex()
	{
		Name = "Dynamic Momentum Index";
		ShortName = "DMI";
	}

	protected override void Initialize()
	{
		_smoothFactor = 1.0 / Period;
	}

	protected override void Calculate(int index)
	{
		if (index < Period + 1)
		{
			Result[index] = 50;
			return;
		}

		var gains = 0.0;
		var losses = 0.0;

		for (var i = Period - 1; i >= 0; i--)
		{
			double currentLoss, currentGain;

			var diff = Source[index - i] - Source[index - (i + 1)];
			if (diff >= 0)
			{
				currentGain = diff;
				currentLoss = 0;
			}
			else
			{
				currentLoss = -1 * diff;
				currentGain = 0;
			}

			gains = gains == 0 ? currentGain : (1 - _smoothFactor) * gains + _smoothFactor * currentGain;
			losses = losses == 0 ? currentLoss : (1 - _smoothFactor) * losses + _smoothFactor * currentLoss;
		}

		Result[index] = losses <= double.Epsilon ? Result[index - 1] : 100 - (100 / (1 + (gains / losses)));
	}
}
