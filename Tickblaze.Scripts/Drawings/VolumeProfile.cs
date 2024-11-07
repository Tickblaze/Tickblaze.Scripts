namespace Tickblaze.Scripts.Drawings;

public class VolumeProfile : Drawing
{
	[Parameter("Width %"), NumericRange(0, 100)]
	public double WidthPercent { get; set; } = 100;

	[Parameter("Value Area %"), NumericRange(0, 100)]
	public double ValueAreaPercent { get; set; } = 70;

	public override int PointsCount => 2;

	private int FromIndex { get; set; } = -1;
	private int ToIndex { get; set; } = -1;
	private double RowSize { get; set; }
	private PriceLevel[] Levels { get; set; } = [];
	private PriceLevel High { get; set; }
	private PriceLevel Low { get; set; }
	private PriceLevel PointOfControl { get; set; }
	private PriceLevel ValueAreaHigh { get; set; }
	private PriceLevel ValueAreaLow { get; set; }

	private record PriceLevel(int Index, double Price)
	{
		public double Volume { get; set; }
	}

	public override void SetPoint(IComparable xDataValue, IComparable yDataValue, int index)
	{
		if (Points.Count < PointsCount)
		{
			return;
		}

		var fromIndex = Chart.GetBarIndexByXCoordinate(Points[0].X);
		var toIndex = Chart.GetBarIndexByXCoordinate(Points[1].X);

		if (fromIndex > toIndex)
		{
			(fromIndex, toIndex) = (toIndex, fromIndex);
		}

		if (fromIndex != FromIndex || toIndex != ToIndex)
		{
			Calculate(fromIndex, toIndex);
		}

		var midPrice = (High.Price + Low.Price) / 2;
		if (midPrice.Equals(Points[0].Value) is false || midPrice.Equals(Points[1].Value) is false)
		{
			Points[0].Value = Points[1].Value = midPrice;
		}
	}

	public override void OnRender(IDrawingContext context)
	{
		if (Levels?.Length > 0)
		{
			Render(context);
		}
	}

	private void Calculate(int fromIndex, int toIndex)
	{
		var high = double.MinValue;
		var low = double.MaxValue;

		for (var index = fromIndex; index <= toIndex; index++)
		{
			var bar = Bars[index];
			if (bar is null)
			{
				return;
			}

			high = Math.Max(high, bar.High);
			low = Math.Min(low, bar.Low);
		}

		var range = high - low;
		var highY = ChartScale.GetYCoordinateByValue(high);
		var lowY = ChartScale.GetYCoordinateByValue(low);
		var pixelsPerUnitY = Math.Abs(highY - lowY) / range;
		var priceIncrement = Math.Max(Symbol.TickSize, Math.Pow(10, -Symbol.Decimals));
		var rowSize = priceIncrement * Math.Ceiling(1 / (priceIncrement * pixelsPerUnitY));

		var count = (int)(range / rowSize) + 1;
		var values = new PriceLevel[count];
		var volumeTotal = 0.0;

		for (var index = fromIndex; index <= toIndex; index++)
		{
			var bar = Bars[index];
			if (bar is null)
			{
				continue;
			}

			var priceLevelsInBar = (int)((bar.High - bar.Low) / rowSize) + 1;
			var volumePerPriceLevel = bar.Volume / priceLevelsInBar;

			for (var price = bar.Low; price <= bar.High + rowSize / 2; price += rowSize)
			{
				var priceIndex = Math.Clamp((int)((price - low) / rowSize), 0, count - 1);

				if (values[priceIndex] == null)
				{
					values[priceIndex] = new(priceIndex, price);
				}

				values[priceIndex].Volume += volumePerPriceLevel;
				volumeTotal += volumePerPriceLevel;
			}
		}

		PointOfControl = null;

		foreach (var value in values)
		{
			if (value is null)
			{
				continue;
			}

			if (PointOfControl is null || PointOfControl.Volume < value.Volume)
			{
				PointOfControl = value;
			}
		}

		// Step 4: Calculate Value Area based on target percentage (e.g., 70%)
		var targetVolume = volumeTotal * ValueAreaPercent / 100;
		var accumulatedVolume = 0.0;

		ValueAreaLow = PointOfControl;
		ValueAreaHigh = PointOfControl;

		var sortedLevels = values
			.Where(v => v != null)
			.OrderByDescending(v => v.Volume)
			.ThenBy(v => Math.Abs(v.Index - PointOfControl.Index));

		foreach (var level in sortedLevels)
		{
			accumulatedVolume += level.Volume;

			if (ValueAreaLow.Index > level.Index)
			{
				ValueAreaLow = level;
			}

			if (ValueAreaHigh.Index < level.Index)
			{
				ValueAreaHigh = level;
			}

			if (accumulatedVolume >= targetVolume)
			{
				break;
			}
		}

		FromIndex = fromIndex;
		ToIndex = toIndex;
		High = values[^1];
		Low = values[0];
		Levels = values;
		RowSize = rowSize;
	}

	private void Render(IDrawingContext context)
	{
		var highY = ChartScale.GetYCoordinateByValue(High.Price);
		var lowY = ChartScale.GetYCoordinateByValue(Low.Price);
		var pixelsPerUnitY = Math.Abs(highY - lowY) / (High.Price - Low.Price);
		var pointA = new Point(Chart.GetXCoordinateByBarIndex(FromIndex), highY);
		var pointB = new Point(Chart.GetXCoordinateByBarIndex(ToIndex), lowY);

		if (true)
		{
			pointA.X = Chart.GetXCoordinateByBarIndex(Chart.GetBarIndexByXCoordinate(Points[0].X));
			pointB.X = Chart.GetXCoordinateByBarIndex(Chart.GetBarIndexByXCoordinate(Points[1].X));
		}

		context.DrawRectangle(pointA, pointB, null, Color.Red);

		var drawLines = pixelsPerUnitY * RowSize <= 1;
		var lines = new Dictionary<int, double>();

		for (var i = 0; i < Levels.Length; i++)
		{
			var level = Levels[i];
			if (level is null)
			{
				continue;
			}

			var volumeRatio = PointOfControl.Volume > 0 ? level.Volume / PointOfControl.Volume : 0;
			var width = (pointB.X - pointA.X) * volumeRatio * WidthPercent / 100;

			if (drawLines)
			{
				var key = (int)Math.Round(ChartScale.GetYCoordinateByValue(level.Price));

				if (lines.TryGetValue(key, out var w) is false || w < width)
				{
					lines[key] = width;
				}
			}
			else
			{
				var y = ChartScale.GetYCoordinateByValue(level.Price + (RowSize / 2));
				var startPoint = new Point(pointA.X, y);
				var endPoint = new Point(pointA.X + width, y);

				var color = level == PointOfControl ? Color.Red : level == High ? Color.White : Color.Gray;
				color = new(128, color.R, color.G, color.B);

				endPoint.Y += RowSize * pixelsPerUnitY;
				context.DrawRectangle(startPoint, endPoint, color, null);
			}
		}

		if (drawLines)
		{
			foreach (var (yCoordinate, width) in lines)
			{
				var startPoint = new Point(pointA.X, yCoordinate);
				var endPoint = new Point(pointA.X + width, yCoordinate);

				context.DrawLine(startPoint, endPoint, "#80808080");
			}
		}

		context.DrawText(new Point(0, 0), $"{RowSize}: {drawLines}", Color.White);
	}
}
