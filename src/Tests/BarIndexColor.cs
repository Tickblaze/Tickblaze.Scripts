using System.ComponentModel;
using System.Diagnostics;

namespace Tickblaze.Scripts.Tests;

[Browsable(false)]
public partial class BarIndexColor : Indicator
{
	[Parameter("Bar width"), NumericRange(0, 1)]
	public double BarWidth { get; set; } = 0.5;

	[Plot("Bar Index")]
	public PlotSeries Result { get; set; } = new(Color.Transparent, PlotStyle.Histogram);

	private readonly Color[] _colors = [Color.Red, Color.Green, Color.Blue, Color.White];
	private int _index, _colorIndex;
	private int _lastIndex = -1;
	private bool _barWidthSet;

	protected override void Initialize()
	{

	}

	protected override void Calculate(int index)
	{
		var bar = Bars[index];

		Result[index] = bar.Close;
		Result.Colors[index] = _colors[_colorIndex];

		if (bar.Close > bar.Open)
		{
			Result.Colors[index] = Color.Green;
		}
		else if (bar.Close < bar.Open)
		{
			Result.Colors[index] = Color.Red;
		}
		else
		{
			Result.Colors[index] = Color.White;
		}

		return;

		if (_lastIndex > index)
		{
			Debugger.Break();
		}

		_lastIndex = index;

		if (_index != index)
		{
			_index = index;

			//_colorIndex = _colorIndex % 3;

			if (++_colorIndex >= _colors.Length)
			{
				_colorIndex = 0;
			}
		}
	}

	public override void OnRender(IDrawingContext context)
	{
		if (_barWidthSet is false)
		{
			Chart.BarWidth = BarWidth;
			_barWidthSet = true;
		}
	}
}
