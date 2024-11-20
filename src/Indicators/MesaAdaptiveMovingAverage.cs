using System.ComponentModel;
namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// MESA Adaptive Adaptive Moving Average [MAMA]
/// </summary>
[Browsable(false)]
public sealed partial class MesaAdaptiveMovingAverage : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Fast Limit"), NumericRange(0.0001, 999, 0.1)]
	public double FastLimit { get; set; } = 0.5;

	[Parameter("Slow Limit"), NumericRange(0.0001, 999, 0.01)]
	public double SlowLimit { get; set; } = 0.05;

	[Plot("MAMA")]
	public PlotSeries Mama { get; set; } = new("#2962ff", LineStyle.Solid, 1);

	[Plot("FAMA")]
	public PlotSeries Fama { get; set; } = new("#ff6240", LineStyle.Solid, 1);

	private List<double> _i1 = [];
	private List<double> _i2 = [];
	private List<double> _im = [];
	private List<double> _q1 = [];
	private List<double> _q2 = [];
	private List<double> _re = [];
	private List<double> _detrender = [];
	private List<double> _period = [];
	private List<double> _phase = [];
	private List<double> _smooth = [];

	private int _priorIndex = -1;
	private double _median0;
	private double _median1;
	private double _median2;
	private double _median3;

	public MesaAdaptiveMovingAverage()
	{
		IsOverlay = true;
		Name = "MESA Adaptive Moving Average";
		ShortName = "MAMA";
	}

	protected override void Calculate(int index)
	{
		var price = Source[index];
		if (_priorIndex != index)
		{
			_priorIndex = index;
			_median3 = _median2 == 0 ? price : _median2;
			_median2 = _median1 == 0 ? price : _median1;
			_median1 = _median0 == 0 ? price : _median0;
			if (index <= 7)
			{
				_detrender.Add(0);
				_period.Add(0);
				_phase.Add(0);
				_smooth.Add(0);
				_i1.Add(0);
				_i2.Add(0);
				_im.Add(0);
				_q1.Add(0);
				_q2.Add(0);
				_re.Add(0);
			}

			while (_detrender.Count > 10)
			{
				_detrender.RemoveAt(0);
				_period.RemoveAt(0);
				_phase.RemoveAt(0);
				_smooth.RemoveAt(0);
				_i1.RemoveAt(0);
				_i2.RemoveAt(0);
				_im.RemoveAt(0);
				_q1.RemoveAt(0);
				_q2.RemoveAt(0);
				_re.RemoveAt(0);
			}
		}

		_median0 = (Bars[index].High + Bars[index].Low) / 2.0;

		if (index < 6)
		{
			Mama[index] = price;
			Fama[index] = price;
			return;
		}

		if (index == 6)
		{
			Mama[index] = _median0;
			Fama[index] = price;
			return;
		}

		var period1 = _period[^2];
		_smooth[^1] = (4 * _median0 + 3 * _median1 + 2 * _median2 + _median3) / 10;
		_detrender[^1] = (0.0962 * _smooth[^1] + 0.5769 * _smooth[^3] - 0.5769 * _smooth[^5] - 0.0962 * _smooth[^7]) * (0.075 * period1 + 0.54);

		_q1[^1] = (0.0962 * _detrender[^1] + 0.5769 * _detrender[^3] - 0.5769 * _detrender[^5] - 0.0962 * _detrender[^7]) * (0.075 * period1 + 0.54);
		_i1[^1] = _detrender[^4];

		var i10 = _i1[^1];
		var jI = (0.0962 * i10 + 0.5769 * _i1[^3] - 0.5769 * _i1[^5] - 0.0962 * _i1[^7]) * (0.075 * period1 + 0.54);
		var jQ = (0.0962 * _q1[^1] + 0.5769 * _q1[^3] - 0.5769 * _q1[^5] - 0.0962 * _q1[^7]) * (0.075 * period1 + 0.54);

		_i2[^1] = i10 - jQ;
		_q2[^1] = _q1[^1] + jI;

		_i2[^1] = 0.2 * _i2[^1] + 0.8 * _i2[^2];
		_q2[^1] = 0.2 * _q2[^1] + 0.8 * _q2[^2];

		var i20 = _i2[^1];
		var q21 = _q2[^2];
		var i21 = _i2[^2];
		var q20 = _q2[^1];
		var period0 = _period[^1];

		_re[^1] = i20 * i21 + q20 * q21;
		_im[^1] = i20 * q21 - q20 * i21;
		_re[^1] = 0.2 * _re[^1] + 0.8 * _re[^2];
		_im[^1] = 0.2 * _im[^1] + 0.8 * _im[^2];

		if (_im[^1] != 0.0 && _re[^1] != 0.0)
		{
			period0 = 360 / (180 / Math.PI * Math.Atan(_im[^1] / _re[^1]));
		}

		if (period0 > 1.5 * period1)
		{
			period0 = 1.5 * period1;
		}

		if (period0 < 0.67 * period1)
		{
			period0 = 0.67 * period1;
		}

		if (period0 < 6)
		{
			period0 = 6;
		}

		if (period0 > 50)
		{
			period0 = 50;
		}

		_period[^1] = 0.2 * period0 + 0.8 * period1;

		if (_i1[^1] != 0.0)
		{
			_phase[^1] = 180 / Math.PI * Math.Atan(_q1[^1] / _i1[^1]);
		}

		var deltaPhase = _phase[^2] - _phase[^1];
		if (deltaPhase < 1)
		{
			deltaPhase = 1;
		}

		var alpha = FastLimit / deltaPhase;
		if (alpha < SlowLimit)
		{
			alpha = SlowLimit;
		}

		Mama[index] = alpha * _median0 + (1 - alpha) * Mama[^2];
		Fama[index] = 0.5 * alpha * Mama[index] + (1 - 0.5 * alpha) * Fama[^2];
	}
}