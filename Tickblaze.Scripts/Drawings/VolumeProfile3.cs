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
	}

	private Area _area;
	private double _rowSize;
	private double[] _volumes;
	private int _pocIndex, _vahIndex, _valIndex;

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

		if (Points[0].Value.Equals(area.High) is false)
		{
			Points[0].Value = area.High;
		}

		if (Points[1].Value.Equals(area.Low) is false)
		{
			Points[1].Value = area.Low;
		}

		return area;
	}

	private void CalculateProfile(Area area)
	{
		_rowSize = RowsLayout is RowsLayoutType.NumberOfRows
			? Math.Max(Symbol.TickSize, area.Range / RowSize)
			: Symbol.TickSize * RowSize;
		_rowSize = Symbol.RoundToTick(_rowSize);

		if (_rowSize <= 0)
		{
			_volumes = [];
			return;
		}

		_volumes = new double[(int)Math.Round(area.Range / _rowSize)];

		for (var index = area.FromIndex; index <= area.ToIndex; index++)
		{
			var bar = Bars[index];
			if (bar is null)
			{
				continue;
			}

			var startLevel = Math.Max(0, (int)Math.Round((bar.Low - area.Low) / _rowSize));
			var endLevel = Math.Min(_volumes.Length - 1, (int)Math.Round((bar.High - area.Low) / _rowSize));
			var volumePerLevel = bar.Volume / (endLevel - startLevel + 1);

			for (var level = startLevel; level <= endLevel; level++)
			{
				_volumes[level] += volumePerLevel;
			}
		}

		_pocIndex = 0;

		for (var i = 1; i < _volumes.Length; i++)
		{
			if (_volumes[_pocIndex] < _volumes[i])
			{
				_pocIndex = i;
			}
		}
	}

	private void Render(IDrawingContext context)
	{
		var highY = ChartScale.GetYCoordinateByValue(_area.High);
		var lowY = ChartScale.GetYCoordinateByValue(_area.Low);
		var pixelsPerUnitY = Math.Abs(highY - lowY) / _area.Range;
		var pointA = new Point(Chart.GetXCoordinateByBarIndex(_area.FromIndex), highY);
		var pointB = new Point(Chart.GetXCoordinateByBarIndex(_area.ToIndex), lowY);

		context.DrawRectangle(pointA, pointB, null, Color.Red);

		for (var i = 0; i < _volumes.Length; i++)
		{
			var y = pointA.Y + i * _rowSize * pixelsPerUnitY;
			var volumeRatio = _volumes[i] / _volumes[_pocIndex];
			var barWidth = (pointB.X - pointA.X) * WidthPercent / 100 * volumeRatio;

			var startPoint = new Point(pointA.X, y);
			var endPoint = new Point(pointA.X + barWidth, y + _rowSize * pixelsPerUnitY);

			// Use color to differentiate POC, VAH, and VAL
			Color fillColor;

			if (i == _pocIndex)
			{
				fillColor = Color.Orange;
			}
			else if (i >= _valIndex && i <= _vahIndex)
			{
				fillColor = Color.Blue;
			}
			else
			{
				fillColor = Color.Gray;
			}

			context.DrawRectangle(startPoint, endPoint, fillColor, null);

			if (i > 0)
			{
				y = pointA.Y + _rowSize * pixelsPerUnitY * i;
				context.DrawLine(new Point(pointA.X, y), new Point(pointB.X, y), Color.White);
			}
		}
	}
}
