namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Double Smoothed Stochastic [DSS]
/// </summary>
public partial class DoubleSmoothStochastics : Indicator
{
	[Parameter("Period 1"), NumericRange(1, int.MaxValue)]
	public int Period1 { get; set; } = 10;

	[Parameter("Period 2"), NumericRange(1, int.MaxValue)]
	public int Period2 { get; set; } = 3;

	[Parameter("Period 3"), NumericRange(1, int.MaxValue)]
	public int Period3 { get; set; } = 3;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, LineStyle.Solid);

	[Plot("Overbought")]
	public PlotLevel OverboughtLevel { get; set; } = new(90, Color.Red, LineStyle.Dash);

	[Plot("Oversold")]
	public PlotLevel OversoldLevel { get; set; } = new(10, Color.Green, LineStyle.Dash);

	private double _priorEMAP1;
	private double _priorEMAP3;
	private double _smoothFactor2;
	private double _smoothFactor3;
	private Minimum _minLow;
	private Maximum _maxHigh;
	private List<double> _p1 = [];
	private List<double> _p2 = [];
	private List<double> _p3 = [];
	private int _priorIndex = -1;

	public DoubleSmoothStochastics()
	{
		Name = "Double Smoothed Stochastic";
		ShortName = "DSS";
	}

	protected override void Initialize()
	{
		_smoothFactor2 = 2.0 / (Period2 + 1);
		_smoothFactor3 = 2.0 / (Period3 + 1);
		_maxHigh = new Maximum(Bars.High, Period1);
		_minLow = new Minimum(Bars.Low, Period1);
	}

	protected override void Calculate(int index)
	{
		if (index < Period1)
		{
			Result[index] = 50;
			_p1.Add(50);
			_p2.Add(0);
			_p3.Add(0);
			return;
		}

		if (_priorIndex != index)
		{
			_priorIndex = index;
			_priorEMAP1 = _p2[1];
			_priorEMAP3 = Result[index - 1];
			_p1.Insert(0, 0);
			_p2.Insert(0, 50);
			_p3.Insert(0, 50);
			while (_p1.Count > 2)
			{
				_p1.RemoveAt(_p1.Count - 1);
			}

			while (_p2.Count > Period1)
			{
				_p2.RemoveAt(_p2.Count - 1);
			}

			while (_p3.Count > 2)
			{
				_p3.RemoveAt(_p3.Count - 1);
			}
		}

		var maxHigh0 = _maxHigh[index];
		var minLow0 = _minLow[index];
		var range = maxHigh0 - minLow0;

		_p1[0] = range <= double.Epsilon ? _p1[1] : Math.Min(100, Math.Max(0, 100 * (Bars[index].Close - minLow0) / range));

		_p2[0] = Ema(_p1[0], _priorEMAP1, Period2, _smoothFactor2);
		var minP20 = _p2.Min();
		var s = _p2.Max() - minP20;

		_p3[0] = s <= double.Epsilon ? _p3[1] : Math.Min(100, Math.Max(0, 100 * (_p2[0] - minP20) / s));

		Result[index] = Ema(_p3[0], _priorEMAP3, Period3, _smoothFactor3);
	}

	private double Ema(double v, double priorE, int period, double smfactor)
	{
		return v * smfactor + priorE * (1 - smfactor);
	}
}