namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Chande Momentum Oscillator [CMO]
/// </summary>
public partial class ChandeMomentumOscillator : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, PlotStyle.Line);

	[Plot("Overbought")]
	public PlotLevel OverboughtLevel { get; set; } = new(70, Color.Red, LineStyle.Dash, 1);

	[Plot("Oversold")]
	public PlotLevel OversoldLevel { get; set; } = new(-70, Color.Green, LineStyle.Dash, 1);

	private readonly List<double> _down = [];
	private readonly List<double> _up = [];
	private int _priorIndex = -1;

	public ChandeMomentumOscillator()
	{
		Name = "Chande Momentum Oscillator";
		ShortName = "CMO";
	}

	protected override void Calculate(int index)
	{
		var input0 = Source[index];
		var input1 = index > 0 ? Source[index - 1] : input0;

		if (_priorIndex != index)
		{
			_down.Insert(0, 0);
			_up.Insert(0, 0);

			while (_down.Count > Period)
			{
				_down.RemoveAt(_down.Count - 1);
			}

			while (_up.Count > Period)
			{
				_up.RemoveAt(_up.Count - 1);
			}

			_priorIndex = index;
		}

		_down[0] = Math.Max(input1 - input0, 0);
		_up[0] = Math.Max(input0 - input1, 0);

		if (index < Period)
		{
			Result[index] = 0;
		}
		else
		{
			var downSum = _down.Sum();
			var upSum = _up.Sum();

			Result[index] = upSum + downSum == 0 ? 0 : 100 * ((upSum - downSum) / (upSum + downSum));
		}
	}
}
