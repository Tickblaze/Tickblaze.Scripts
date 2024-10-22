
namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// ARC ATRForecaster [ATRf]
/// </summary>
public partial class ATRForecaster : Indicator
{
	[Parameter("ATR Period"), NumericRange(1, int.MaxValue)]
	public int ATRPeriod { get; set; } = 14;

	[Parameter("Lookback days"), NumericRange(1, int.MaxValue)]
	public int LookbackDays { get; set; } = 90;

	[Parameter("Type of levels")]
	public LevelType LevelsType { get; set; } = LevelType.Fixed;

	[Plot("ATR Max Upper")]
	public PlotSeries MaxUpper { get; set; } = new("#00ffff", LineStyle.Solid);

	[Plot("ATR Max Lower")]
	public PlotSeries MaxLower { get; set; } = new("#00ffff", LineStyle.Solid);

	[Plot("ATR Avg Upper")]
	public PlotSeries AvgUpper { get; set; } = new("#ffff00", LineStyle.Solid);

	[Plot("ATR Avg Lower")]
	public PlotSeries AvgLower { get; set; } = new("#ffff00", LineStyle.Solid);

	[Plot("Midline")]
	public PlotSeries Midline { get; set; } = new("#ffffff", LineStyle.Solid);

	public enum LevelType
	{
		Fixed,
		Dynamic
	}

	private List<double> _atr = [];
	private double _maxATR;
	private double _currentHigh;
	private double _currentLow = double.MaxValue;
	private double _upperPriceMax;
	private double _lowerPriceMax;
	private double _upperPriceAvg;
	private double _lowerPriceAvg;
	private int _dayId = 0;
	private double _averageATR = 0;
	private bool _isNewSession = false;

	public ATRForecaster()
	{
		Name = "ARC ATRForecaster";
		ShortName = "ATRf";
		IsOverlay = true;
	}

	protected override void Calculate(int index)
	{
		var localTime = Bars[index].Time.ToLocalTime();

		_isNewSession = localTime.Day != _dayId;

		if (_dayId == 0 || _isNewSession)
		{
			if (_currentLow != double.MaxValue)
			{
				_atr.Add(_currentHigh - _currentLow);
				while (_atr.Count > LookbackDays)
				{
					_atr.RemoveAt(0);
				}

				_maxATR = _atr.Max();
				_averageATR = _atr.Average();
			}

			_dayId = localTime.Day;
			_currentHigh = Bars[index].High;
			_currentLow = Bars[index].Low;
		}

		if (_isNewSession)
		{
			var atrAvgDistance = Bars.Symbol.RoundToTick(_averageATR / 2.0);
			_upperPriceAvg = Bars[index].Open + atrAvgDistance;
			_lowerPriceAvg = Bars[index].Open - atrAvgDistance;
			var atrMaxDistance = Bars.Symbol.RoundToTick(_maxATR / 2.0);
			_upperPriceMax = Bars[index].Open + atrMaxDistance;
			_lowerPriceMax = Bars[index].Open - atrMaxDistance;
		}

		_currentHigh = Math.Max(_currentHigh, Bars[index].High);
		_currentLow = Math.Min(_currentLow, Bars[index].Low);

		Midline[index] = (_currentHigh + _currentLow) / 2.0;
		if (LevelsType == LevelType.Dynamic)
		{
			MaxUpper[index] = Bars.Symbol.RoundToTick(Midline[index] + _maxATR / 2.0);
			MaxLower[index] = Bars.Symbol.RoundToTick(Midline[index] - _maxATR / 2.0);

			AvgUpper[index] = Bars.Symbol.RoundToTick(Midline[index] + _averageATR / 2.0);
			AvgLower[index] = Bars.Symbol.RoundToTick(Midline[index] - _averageATR / 2.0);
		}
		else
		{
			MaxUpper[index] = _upperPriceMax;
			MaxLower[index] = _lowerPriceMax;

			AvgUpper[index] = _upperPriceAvg;
			AvgLower[index] = _lowerPriceAvg;
		}
	}
}