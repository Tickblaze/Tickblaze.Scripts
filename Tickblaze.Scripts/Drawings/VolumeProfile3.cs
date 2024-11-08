namespace Tickblaze.Scripts.Drawings;

public class VolumeProfile3 : Drawing
{
	[Parameter("Rows Layout")]
	public RowsLayoutType RowsLayout { get; set; }

	[Parameter("Row Size")]
	public int RowSize { get; set; } = 24;

	[Parameter("Width %"), NumericRange(0, 100)]
	public double WidthPercent { get; set; } = 30;

	[Parameter("Value Area %"), NumericRange(0, 100)]
	public double ValueAreaPercent { get; set; } = 70;

	[Parameter("Extend Right")]
	public bool ExtendRight { get; set; }

	public override int PointsCount => 2;

	public enum RowsLayoutType
	{
		NumberOfRows,
		TicksPerRow
	}

	private record Area()
	{
		public int FromIndex { get; set; }
		public int ToIndex { get; set; }
		public double High { get; set; } = double.MinValue;
		public double Low { get; set; } = double.MaxValue;
		public double Volume { get; set; }
		public double Range => High - Low;
		public double RowSize { get; set; }
		public double[] Volumes { get; set; }
	}

	private Area _area;
	private int _pocIndex, _vahIndex, _valIndex;
	private Color _color, _colorValueArea;

	protected override void Initialize()
	{
		var baseColor = Color.TealGreen;

		_color = new((byte)Math.Round(255 * 0.5), baseColor.R, baseColor.G, baseColor.B);
		_colorValueArea = new((byte)Math.Round(255 * 0.75), baseColor.R, baseColor.G, baseColor.B);
	}

	public override void SetPoint(IComparable xDataValue, IComparable yDataValue, int index)
	{
		if (Points.Count >= PointsCount)
		{
			CalculateArea();
		}
	}

	public override void OnRender(IDrawingContext context)
	{
		var area = CalculateArea();

		if (_area is null || _area != area)
		{
			_area = area;
			CalculateProfile(area);
		}

		Render(context);
	}

	private Area CalculateArea()
	{
		var area = new Area()
		{
			FromIndex = Chart.GetBarIndexByXCoordinate(Points[0].X),
			ToIndex = ExtendRight ? Bars.Count - 1 : Chart.GetBarIndexByXCoordinate(Points[1].X)
		};

		if (area.FromIndex > area.ToIndex)
		{
			(area.FromIndex, area.ToIndex) = (area.ToIndex, area.FromIndex);
		}

		for (var index = area.FromIndex; index <= area.ToIndex; index++)
		{
			var bar = Bars[index];
			if (bar is null)
			{
				continue;
			}

			if (area.High < bar.High)
			{
				area.High = bar.High;
			}

			if (area.Low > bar.Low)
			{
				area.Low = bar.Low;
			}

			area.Volume += bar.Volume;
		}

		area.RowSize = Symbol.RoundToTick(RowsLayout is RowsLayoutType.NumberOfRows
			? Math.Max(Symbol.TickSize, area.Range / RowSize)
			: Symbol.TickSize * RowSize);

		if (area.RowSize <= 0)
		{
			area.Volumes = [];
		}
		else
		{
			area.Low = Math.Floor(area.Low / area.RowSize) * area.RowSize;
			area.High = Math.Ceiling(area.High / area.RowSize) * area.RowSize;
			area.Volumes = new double[(int)Math.Round(area.Range / area.RowSize)];

			if (Points[0].Value.Equals(area.High) is false)
			{
				Points[0].Value = area.High;
			}

			if (Points[1].Value.Equals(area.Low) is false)
			{
				Points[1].Value = area.Low;
			}
		}

		return area;
	}

	private void CalculateProfile(Area area)
	{
		for (var index = area.FromIndex; index <= area.ToIndex; index++)
		{
			var bar = Bars[index];
			if (bar is null)
			{
				continue;
			}

			var startLevel = Math.Max(0, (int)Math.Round((bar.Low - area.Low) / area.RowSize));
			var endLevel = Math.Min(area.Volumes.Length - 1, (int)Math.Round((bar.High - area.Low) / area.RowSize));
			var volumePerLevel = bar.Volume / (endLevel - startLevel + 1);

			for (var level = startLevel; level <= endLevel; level++)
			{
				area.Volumes[level] += volumePerLevel;
			}
		}

		_pocIndex = 0;

		for (var i = 1; i < area.Volumes.Length; i++)
		{
			if (area.Volumes[_pocIndex] < area.Volumes[i])
			{
				_pocIndex = i;
			}
		}

		var targetVolume = area.Volume * (ValueAreaPercent / 100);
		var accumulatedVolume = area.Volumes[_pocIndex];

		_vahIndex = _pocIndex;
		_valIndex = _pocIndex;

		while (accumulatedVolume < targetVolume)
		{
			var expanded = false;

			if (_valIndex > 0 && (_vahIndex == area.Volumes.Length - 1 || area.Volumes[_valIndex - 1] >= area.Volumes[_vahIndex + 1]))
			{
				accumulatedVolume += area.Volumes[--_valIndex];
				expanded = true;
			}

			if (_vahIndex < area.Volumes.Length - 1 && (_valIndex == 0 || area.Volumes[_vahIndex + 1] >= area.Volumes[_valIndex - 1]))
			{
				accumulatedVolume += area.Volumes[++_vahIndex];
				expanded = true;
			}

			if (expanded is false)
			{
				break;
			}
		}
	}

	private void Render(IDrawingContext context)
	{
		var area = _area;
		var highY = ChartScale.GetYCoordinateByValue(_area.High);
		var lowY = ChartScale.GetYCoordinateByValue(_area.Low);
		var pixelsPerUnitY = Math.Abs(highY - lowY) / _area.Range;
		var adjustSpacing = area.RowSize * pixelsPerUnitY >= 2;
		var pointA = new Point(Chart.GetXCoordinateByBarIndex(_area.FromIndex), highY);
		var pointB = new Point(Chart.GetXCoordinateByBarIndex(_area.ToIndex), lowY);

		context.DrawRectangle(pointA, pointB, "#1326c6da", null);

		for (var i = 0; i < area.Volumes.Length; i++)
		{
			var y = lowY - i * area.RowSize * pixelsPerUnitY;
			var volumeRatio = area.Volumes[i] / area.Volumes[_pocIndex];
			var barWidth = (pointB.X - pointA.X) * WidthPercent / 100 * volumeRatio;

			var startPoint = new Point(pointA.X, y);
			var endPoint = new Point(pointA.X + barWidth, y - area.RowSize * pixelsPerUnitY + (adjustSpacing ? 1 : 0));
			var fillColor = i >= _valIndex && i <= _vahIndex ? _colorValueArea : _color;

			context.DrawRectangle(startPoint, endPoint, fillColor, null, adjustSpacing ? 2 : 1);
		}
	}
}
