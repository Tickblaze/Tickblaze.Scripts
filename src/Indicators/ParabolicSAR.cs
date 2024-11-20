namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Parabolic SAR [PSAR]
/// </summary>
public partial class ParabolicSAR : Indicator
{
	[Parameter("Acceleration"), NumericRange(0.0001, double.MaxValue)]
	public double Acceleration { get; set; } = 0.02;

	[Parameter("Acceleration Step"), NumericRange(0.0001, double.MaxValue)]
	public double AccelerationStep { get; set; } = 0.02;

	[Parameter("Acceleration Max"), NumericRange(0.0001, double.MaxValue)]
	public double AccelerationMax { get; set; } = 0.2;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Yellow, PlotStyle.Dot, 2);

	private double _af;
	private bool _afIncreased;
	private bool _longPosition;
	private int _prevBar;
	private double _prevSAR;
	private int _reverseBar;
	private double _reverseValue;
	private double _todaySAR;
	private double _xp;
	private DataSeries _afSeries;
	private DataSeries _afIncreasedSeries;
	private DataSeries _longPositionSeries;
	private DataSeries _prevBarSeries;
	private DataSeries _prevSARSeries;
	private DataSeries _reverseBarSeries;
	private DataSeries _reverseValueSeries;
	private DataSeries _todaySARSeries;
	private DataSeries _xpSeries;

	public ParabolicSAR()
	{
		Name = "Parabolic SAR";
		ShortName = "PSAR";
		IsOverlay = true;
	}

	protected override void Initialize()
	{
		_afIncreased = false;
		_afSeries = new DataSeries();
		_afIncreasedSeries = new DataSeries();
		_longPositionSeries = new DataSeries();
		_prevBarSeries = new DataSeries();
		_prevSARSeries = new DataSeries();
		_reverseBarSeries = new DataSeries();
		_reverseValueSeries = new DataSeries();
		_todaySARSeries = new DataSeries();
		_xpSeries = new DataSeries();
	}

	protected override void Calculate(int index)
	{
		var high0 = Bars[index].High;
		var low0 = Bars[index].Low;
		var close0 = Bars[index].Close;

		if (index < 3)
		{
			Result[index] = close0;
			return;
		}

		if (index == 3)
		{
			_longPosition = high0 > Bars[index - 1].High;
			_xp = _longPosition ? Bars.High.Max() : Bars.Low.Min();
			_af = Acceleration;
			Result[index] = _xp + (_longPosition ? -1 : 1) * ((Bars.High.Max() - Bars.Low.Max()) * _af);
			return;
		}

		if (index < _prevBar)
		{
			_af = _afSeries[index];
			_afIncreased = _afIncreasedSeries[index] == 1;
			_longPosition = _longPositionSeries[index] == 1;
			_prevBar = (int)_prevBarSeries[index];
			_prevSAR = _prevSARSeries[index];
			_reverseBar = (int)_reverseBarSeries[index];
			_reverseValue = _reverseValueSeries[index];
			_todaySAR = _todaySARSeries[index];
			_xp = _xpSeries[index];
		}

		if (_afIncreased && _prevBar != index)
		{
			_afIncreased = false;
		}

		if (_reverseBar != index)
		{
			_todaySAR = TodaySAR(Result[index - 1] + _af * (_xp - Result[index - 1]), index, high0, low0);
			for (var x = 1; x <= 2; x++)
			{
				if (_longPosition)
				{
					if (_todaySAR > Bars[index - x].Low)
					{
						_todaySAR = Bars[index - x].Low;
					}
				}
				else
				{
					if (_todaySAR < Bars[index - x].High)
					{
						_todaySAR = Bars[index - x].High;
					}
				}
			}

			if (_longPosition)
			{
				if (_prevBar != index || low0 < _prevSAR)
				{
					Result[index] = _todaySAR;
					_prevSAR = _todaySAR;
				}
				else
				{
					Result[index] = _prevSAR;
				}

				if (high0 > _xp)
				{
					_xp = high0;
					AfIncrease();
				}
			}
			else if (!_longPosition)
			{
				if (_prevBar != index || high0 > _prevSAR)
				{
					Result[index] = _todaySAR;
					_prevSAR = _todaySAR;
				}
				else
				{
					Result[index] = _prevSAR;
				}

				if (low0 < _xp)
				{
					_xp = low0;
					AfIncrease();
				}
			}
		}
		else
		{
			if (_longPosition && high0 > _xp)
			{
				_xp = high0;
			}
			else if (!_longPosition && low0 < _xp)
			{
				_xp = low0;
			}

			Result[index] = _prevSAR;
			_todaySAR = TodaySAR(_longPosition ? Math.Min(_reverseValue, low0) : Math.Max(_reverseValue, high0), index, high0, low0);
		}

		_prevBar = index;

		// Reverse position
		if ((_longPosition && (low0 < _todaySAR || Bars[index - 1].Low < _todaySAR))
			|| (!_longPosition && (high0 > _todaySAR || Bars[index - 1].High > _todaySAR)))
		{
			Result[index] = Reverse(index, high0, low0);
		}

		{
			_afSeries[index] = _af;
			_afIncreasedSeries[index] = _afIncreased ? 1 : -1;
			_longPositionSeries[index] = _longPosition ? 1 : -1;
			_prevBarSeries[index] = _prevBar;
			_prevSARSeries[index] = _prevSAR;
			_reverseBarSeries[index] = _reverseBar;
			_reverseValueSeries[index] = _reverseValue;
			_todaySARSeries[index] = _todaySAR;
			_xpSeries[index] = _xp;
		}
	}

	private void AfIncrease()
	{
		if (!_afIncreased)
		{
			_af = Math.Min(AccelerationMax, _af + AccelerationStep);
			_afIncreased = true;
		}
	}

	private double TodaySAR(double tSAR, int index, double H0, double L0)
	{
		if (_longPosition)
		{
			var lowestSAR = Math.Min(Math.Min(tSAR, L0), Bars[index - 1].Low);
			if (L0 > lowestSAR)
			{
				tSAR = lowestSAR;
			}
		}
		else
		{
			var highestSAR = Math.Max(Math.Max(tSAR, H0), Bars[index - 1].High);
			if (H0 < highestSAR)
			{
				tSAR = highestSAR;
			}
		}

		return tSAR;
	}

	private double Reverse(int index, double H0, double L0)
	{
		var tSAR = _xp;

		if ((_longPosition && _prevSAR > L0) || (!_longPosition && _prevSAR < H0) || _prevBar != index)
		{
			_longPosition = !_longPosition;
			_reverseBar = index;
			_reverseValue = _xp;
			_af = Acceleration;
			_xp = _longPosition ? H0 : L0;
			_prevSAR = tSAR;
		}
		else
		{
			tSAR = _prevSAR;
		}

		return tSAR;
	}
}
