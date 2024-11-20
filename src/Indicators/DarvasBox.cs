using System.ComponentModel;

namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Darvas Box [DBO]
/// </summary>
[Browsable(false)]
public partial class DarvasBox : Indicator
{
	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 5;

	[Plot("Upper")]
	public PlotSeries Upper { get; set; } = new(Color.Blue);
	[Plot("Lower")]
	public PlotSeries Lower { get; set; } = new(Color.Blue);

	private double _boxBottom = double.MaxValue;
	private double _boxTop = double.MinValue;
	private double _currentBarHigh = double.MinValue;
	private double _currentBarLow = double.MaxValue;
	private int _savedCurrentBar = -1;
	private int _startBarActBox;
	private int _state;
	public DarvasBox()
	{
		Name = "Darvas Box";
		ShortName = "DBO";
		IsOverlay = true;
	}

	protected override void Calculate(int index)
	{
		Upper[index] = Bars[index].High;
		Lower[index] = Bars[index].Low;

		if (_savedCurrentBar == -1)
		{
			_currentBarHigh = Bars[index].High;
			_currentBarLow = Bars[index].Low;
			_state = GetNextState();
			_savedCurrentBar = index;
		}
		else if (_savedCurrentBar != index)
		{
			_currentBarHigh = Bars[index].High;
			_currentBarLow = Bars[index].Low;

			if ((_state == 5 && _currentBarHigh > _boxTop) || (_state == 5 && _currentBarLow < _boxBottom))
			{
				_state = 0;
				_startBarActBox = index;
			}

			_state = GetNextState();
			if (_boxBottom == double.MaxValue)
			{
				for (var i = index - _startBarActBox; i <= index; i++)
				{
					Upper[i] = _boxTop;
				}
			}
			else
			{
				for (var i = index - _startBarActBox; i <= index; i++)
				{
					Upper[i] = _boxTop;
					Lower[i] = _boxBottom;
				}
			}
		}
		else
		{
			if ((_state == 5 && _currentBarHigh > _boxTop) || (_state == 5 && _currentBarLow < _boxBottom))
			{
				_startBarActBox = index + 1;
				_state = 0;
			}

			if (_boxBottom == double.MaxValue)
			{
				Upper[index] = _boxTop;
			}
			else
			{
				Upper[index] = _boxTop;
				Lower[index] = _boxBottom;
			}
		}
	}
	private int GetNextState()
	{
		switch (_state)
		{
			case 0:
				_boxTop = _currentBarHigh;
				_boxBottom = double.MaxValue;
				return 1;

			case 1:
				if (_boxTop > _currentBarHigh)
				{
					return 2;
				}
				else
				{
					_boxTop = _currentBarHigh;
					return 1;
				}

			case 2:
				if (_boxTop > _currentBarHigh)
				{
					_boxBottom = _currentBarLow;
					return 3;
				}
				else
				{
					_boxTop = _currentBarHigh;
					return 1;
				}

			case 3:
				if (_boxTop > _currentBarHigh)
				{
					if (_boxBottom < _currentBarLow)
					{
						return 4;
					}
					else
					{
						_boxBottom = _currentBarLow;
						return 3;
					}
				}
				else
				{
					_boxTop = _currentBarHigh;
					_boxBottom = double.MaxValue;
					return 1;
				}

			case 4:
				if (_boxTop > _currentBarHigh)
				{
					if (_boxBottom < _currentBarLow)
					{
						return 5;
					}
					else
					{
						_boxBottom = _currentBarLow;
						return 3;
					}
				}
				else
				{
					_boxTop = _currentBarHigh;
					_boxBottom = double.MaxValue;
					return 1;
				}

			case 5:
				return 5;

			default:
				return _state;
		}

	}
}
