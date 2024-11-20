namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Accelerator Oscillator [AoC]
/// </summary>
public partial class AcceleratorOscillator : Indicator
{
	[Plot("Up")]
	public PlotSeries Up { get; set; } = new(Color.Green, PlotStyle.Histogram);

	[Plot("Down")]
	public PlotSeries Down { get; set; } = new(Color.Red, PlotStyle.Histogram);

	private AwesomeOscillator _awesomeOscillator;
	private SimpleMovingAverage _simpleMovingAverage;

	public AcceleratorOscillator()
	{
		Name = "Accelerator Oscillator";
		ShortName = "AoC";
	}

	protected override void Initialize()
	{
		_awesomeOscillator = new AwesomeOscillator();
		_simpleMovingAverage = new SimpleMovingAverage(_awesomeOscillator.Result, 5);
	}

	protected override void Calculate(int index)
	{
		var result = _awesomeOscillator[index] - _simpleMovingAverage[index];

		if (index > 0)
		{
			var result1 = _awesomeOscillator[index - 1] - _simpleMovingAverage[index - 1];
			if (result > result1)
			{
				Up[index] = result;
				Down[index] = 0;
			}
			else
			{
				Up[index] = 0;
				Down[index] = result;
			}
		}
		else
		{
			Up[index] = result;
			Down[index] = result;
		}
	}
}
