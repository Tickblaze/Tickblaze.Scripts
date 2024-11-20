using System.ComponentModel;
namespace Tickblaze.Scripts.Indicators;

[Browsable(false)]
public sealed partial class Swing : Indicator
{
	[Parameter("Strength"), NumericRange(1, 999, 1)]
	public int Strength { get; set; } = 5;

	[Plot("Swing High")]
	public PlotSeries SwingHighPlot { get; set; } = new("#ffaa12", LineStyle.DashDot, 2);

	[Plot("Swing Low")]
	public PlotSeries SwingLowPlot { get; set; } = new("#ffaa12", LineStyle.DashDot, 2);

	private int _constant;
	private double _currentSwingHigh;
	private double _currentSwingLow;
	private List<double> _lastHighCache;
	private double _lastSwingHighValue;
	private List<double> _lastLowCache;
	private double _lastSwingLowValue;
	private int _saveCurrentBar;
	private DataSeries _swingHighSeries;
	private DataSeries _swingHighSwings;
	private DataSeries _swingLowSeries;
	private DataSeries _swingLowSwings;

	public Swing()
	{
		Name = "Swing";
		IsOverlay = true;
	}

	protected override void Calculate(int index)
	{
		var high0 = Bars.Symbol.RoundToTick(Bars[index].High);
		var low0 = Bars.Symbol.RoundToTick(Bars[index].Low);
		var close0 = Bars.Symbol.RoundToTick(Bars[index].Close);

		SwingHighPlot[^1] = close0;//only because this code needs all plot values to contain some non-null value
		SwingLowPlot[^1] = close0;//only because this code needs all plot values to contain some non-null value

		if (index < _saveCurrentBar)
		{
			_currentSwingHigh = SwingHighPlot[^1];
			_currentSwingLow = SwingLowPlot[^1];
			_lastSwingHighValue = _swingHighSeries[^1];
			_lastSwingLowValue = _swingLowSeries[^1];
			_swingHighSeries[index - Strength] = 0;
			_swingLowSeries[index - Strength] = 0;

			_lastHighCache.Clear();
			_lastLowCache.Clear();

			for (var barsBack = Math.Min(index, _constant) - 1; barsBack >= 0; barsBack--)
			{
				_lastHighCache.Add(Bars.Symbol.RoundToTick(Bars[index - barsBack].High));
				_lastLowCache.Add(Bars.Symbol.RoundToTick(Bars[index - barsBack].Low));
			}

			_saveCurrentBar = index;
			return;
		}

		if (_saveCurrentBar != index)
		{
			_swingHighSwings[^1] = 0;
			_swingLowSwings[^1] = 0;

			_swingHighSeries[^1] = 0;
			_swingLowSeries[^1] = 0;

			_lastHighCache.Add(high0);
			if (_lastHighCache.Count > _constant)
			{
				_lastHighCache.RemoveAt(0);
			}

			_lastLowCache.Add(low0);
			if (_lastLowCache.Count > _constant)
			{
				_lastLowCache.RemoveAt(0);
			}

			if (_lastHighCache.Count == _constant)
			{
				var isSwingHigh = true;
				var swingHighCandidateValue = _lastHighCache[Strength];

				for (var i = 0; i < Strength; i++)
				{
					if (_lastHighCache[i] != swingHighCandidateValue)
					{
						isSwingHigh = false;
					}
				}

				for (var i = Strength + 1; i < _lastHighCache.Count; i++)
				{
					if (_lastHighCache[i] == swingHighCandidateValue)
					{
						isSwingHigh = false;
					}
				}

				_swingHighSwings[Strength] = isSwingHigh ? swingHighCandidateValue : 0.0;

				if (isSwingHigh)
				{
					_lastSwingHighValue = swingHighCandidateValue;
				}

				if (isSwingHigh)
				{
					_currentSwingHigh = swingHighCandidateValue;

					for (var i = 0; i <= Strength; i++)
					{
						SwingHighPlot[index - i] = _currentSwingHigh;
					}
				}
				else if (high0 > _currentSwingHigh || _currentSwingHigh == 0)
				{
					_currentSwingHigh = 0.0;
					SwingHighPlot[^1] = close0;
					//SwingHighPlot.Reset();
				}
				else
				{
					SwingHighPlot[^1] = _currentSwingHigh;
				}

				if (isSwingHigh)
				{
					for (var i = 0; i <= Strength; i++)
					{
						_swingHighSeries[index - i] = _lastSwingHighValue;
					}
				}
				else
				{
					_swingHighSeries[^1] = _lastSwingHighValue;
				}
			}

			if (_lastLowCache.Count == _constant)
			{
				var isSwingLow = true;
				var swingLowCandidateValue = _lastLowCache[Strength];

				for (var i = 0; i < Strength; i++)
				{
					if (_lastLowCache[index - i] != swingLowCandidateValue)
					{
						isSwingLow = false;
					}
				}

				for (var i = Strength + 1; i < _lastLowCache.Count; i++)
				{
					if (_lastLowCache[index - i] == swingLowCandidateValue)
					{
						isSwingLow = false;
					}
				}

				_swingLowSwings[index - Strength] = isSwingLow ? swingLowCandidateValue : 0.0;
				if (isSwingLow)
				{
					_lastSwingLowValue = swingLowCandidateValue;
				}

				if (isSwingLow)
				{
					_currentSwingLow = swingLowCandidateValue;
					for (var i = 0; i <= Strength; i++)
					{
						SwingLowPlot[index - i] = _currentSwingLow;
					}
				}
				else if (low0 < _currentSwingLow || _currentSwingLow == 0)
				{
					_currentSwingLow = double.MaxValue;
					SwingLowPlot[^1] = close0;
					//SwingLowPlot.Reset();
				}
				else
				{
					SwingLowPlot[^1] = _currentSwingLow;
				}

				if (isSwingLow)
				{
					for (var i = 0; i <= Strength; i++)
					{
						_swingLowSeries[index - i] = _lastSwingLowValue;
					}
				}
				else
				{
					_swingLowSeries[^1] = _lastSwingLowValue;
				}
			}

			_saveCurrentBar = index;
		}
		else if (index >= _constant - 1)
		{
			if (_lastHighCache.Count == _constant && high0 > _lastHighCache[^1])
			{
				_lastHighCache[^1] = high0;
			}

			if (_lastLowCache.Count == _constant && low0 < _lastLowCache[^1])
			{
				_lastLowCache[^1] = low0;
			}

			if (high0 > _currentSwingHigh && _swingHighSwings[index - Strength] > 0.0)
			{
				_swingHighSwings[index - Strength] = 0.0;
				for (var i = 0; i <= Strength; i++)
				{
					SwingHighPlot[index - i] = close0;
					//                    SwingHighPlot.Reset(i);
					_currentSwingHigh = 0.0;
				}
			}
			else if (high0 > _currentSwingHigh && _currentSwingHigh != 0)
			{
				SwingHighPlot[^1] = close0;
				//                SwingHighPlot.Reset();
				_currentSwingHigh = 0.0;
			}
			else if (high0 <= _currentSwingHigh)
			{
				SwingHighPlot[^1] = _currentSwingHigh;
			}

			if (low0 < _currentSwingLow && _swingLowSwings[index - Strength] > 0.0)
			{
				_swingLowSwings[index - Strength] = 0.0;
				for (var i = 0; i <= Strength; i++)
				{
					SwingLowPlot[index - i] = close0;
					//                    SwingLowPlot.Reset(i);
					_currentSwingLow = double.MaxValue;
				}
			}
			else if (low0 < _currentSwingLow && _currentSwingLow != double.MaxValue)
			{
				//                SwingLowPlot.Reset();
				_currentSwingLow = double.MaxValue;
			}
			else if (low0 >= _currentSwingLow)
			{
				SwingLowPlot[^1] = _currentSwingLow;
			}
		}
	}
}