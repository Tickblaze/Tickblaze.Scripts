namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Average Directional Movement Index [ADX]
/// </summary>
public partial class AverageDirectionalMovementIndex : Indicator
{
	[Parameter("Period"), NumericRange(1, 999, 1)]
	public int Period { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; }

	[Plot("Lower")]
	public PlotLevel Lower { get; set; } = new(25, "#80787b86", LineStyle.Dash);

	[Plot("Upper")]
	public PlotLevel Upper { get; set; } = new(75, "#80787b86", LineStyle.Dash);

	private List<double> _dmPlus = [];
	private List<double> _dmMinus = [];
	private List<double> _sumDmPlus = [];
	private List<double> _sumDmMinus = [];
	private List<double> _sumTr = [];
	private List<double> _tr = [];
	private int _priorABar = -1;

	public AverageDirectionalMovementIndex()
	{
		Name = "Average Directional Movement Index";
		ShortName = "ADX";
	}

	protected override void Calculate(int index)
	{
		if (_priorABar != index)
		{
			_priorABar = index;
			_tr.Insert(0, 0);
			_sumTr.Insert(0, 0);
			_dmPlus.Insert(0, 0);
			_dmMinus.Insert(0, 0);
			_sumDmPlus.Insert(0, 0);
			_sumDmMinus.Insert(0, 0);
			while (_tr.Count > 2)
			{
				_tr.RemoveAt(_tr.Count - 1);
				_sumTr.RemoveAt(_sumTr.Count - 1);
				_dmPlus.RemoveAt(_dmPlus.Count - 1);
				_dmMinus.RemoveAt(_dmMinus.Count - 1);
				_sumDmPlus.RemoveAt(_sumDmPlus.Count - 1);
				_sumDmMinus.RemoveAt(_sumDmMinus.Count - 1);
			}
		}

		if (index == 0)
		{
			_tr[0] = Bars[index].High - Bars[index].Low;
			_dmPlus[0] = 0;
			_dmMinus[0] = 0;
			_sumTr[0] = _tr[0];
			_sumDmPlus[0] = _dmPlus[0];
			_sumDmMinus[0] = _dmMinus[0];
			Result[0] = 50;
		}
		else
		{
			var high0 = Bars[index].High;
			var low0 = Bars[index].Low;
			var high1 = Bars[index - 1].High;
			var low1 = Bars[index - 1].Low;
			var close1 = Bars[index - 1].Close;

			_tr[0] = Math.Max(Math.Abs(low0 - close1), Math.Max(high0 - low0, Math.Abs(high0 - close1)));
			_dmPlus[0] = high0 - high1 > low1 - low0 ? Math.Max(high0 - high1, 0) : 0;
			_dmMinus[0] = low1 - low0 > high0 - high1 ? Math.Max(low1 - low0, 0) : 0;

			if (index < Period)
			{
				_sumTr[0] = _sumTr[1] + _tr[0];
				_sumDmPlus[0] = _sumDmPlus[1] + _dmPlus[0];
				_sumDmMinus[0] = _sumDmMinus[1] + _dmMinus[0];
			}
			else
			{
				_sumTr[0] = _sumTr[1] - _sumTr[1] / Period + _tr[0];
				_sumDmPlus[0] = _sumDmPlus[1] - _sumDmPlus[1] / Period + _dmPlus[0];
				_sumDmMinus[0] = _sumDmMinus[1] - _sumDmMinus[1] / Period + _dmMinus[0];
			}

			var diPlus = 100 * (_sumTr[0] == 0 ? 0 : _sumDmPlus[0] / _sumTr[0]);
			var diMinus = 100 * (_sumTr[0] == 0 ? 0 : _sumDmMinus[0] / _sumTr[0]);
			var diff = Math.Abs(diPlus - diMinus);
			var sum = diPlus + diMinus;

			Result[index] = sum == 0 ? 50 : ((Period - 1) * Result[index - 1] + 100 * diff / sum) / Period;
		}
	}
}
