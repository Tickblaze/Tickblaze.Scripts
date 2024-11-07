using Tickblaze.Scripts.Api.Interfaces;

namespace Tickblaze.Scripts.Drawings;

public class VolumeProfile2 : Drawing
{
	public override int PointsCount => 2;

	private int FromIndex { get; set; } = -1;
	private int ToIndex { get; set; } = -1;
	private double High { get; set; }
	private double Low { get; set; }
	private double[] Values { get; set; } = [];
	private double Maximum { get; set; }
	private double RowSize { get; set; }

	public override void SetPoint(IComparable xDataValue, IComparable yDataValue, int index)
	{
		if (Points.Count < PointsCount)
		{
			return;
		}

		Calculate();

		var midPrice = (High + Low) / 2;
		if (midPrice.Equals(Points[0].Value) is false || midPrice.Equals(Points[1].Value) is false)
		{
			Points[0].Value = Points[1].Value = midPrice;
		}
	}

	public override void OnRender(IDrawingContext context)
	{
		Calculate();

		if (Values?.Length > 0)
		{
			Render(context);
		}
	}

	private void Calculate()
	{
		var fromIndex = Chart.GetBarIndexByXCoordinate(Points[0].X);
		var toIndex = Chart.GetBarIndexByXCoordinate(Points[1].X);

		if (fromIndex > toIndex)
		{
			(fromIndex, toIndex) = (toIndex, fromIndex);
		}

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

		// Step 4: Define bin size and count based on Symbol.TickSize and minimum pixel height.
		var rowSize = Symbol.TickSize;
		var highY = ChartScale.GetYCoordinateByValue(high);
		var lowY = ChartScale.GetYCoordinateByValue(low);
		var pixelHeight = Math.Abs(highY - lowY); // Total pixel height of the profile area
		var range = high - low; // Price range

		// Calculate the maximum number of bins such that each bin has a minimum of 3 pixels in height
		var minPixelsPerBin = 1; // Minimum pixel height per bin
		var maxBinCount = Math.Max(1, (int)(pixelHeight / minPixelsPerBin)); // Ensures each bin is at least 3 pixels
		var count = Math.Min((int)(range / rowSize) + 1, maxBinCount);

		// Recalculate binSize if the actual binCount is smaller
		if (count == maxBinCount)
		{
			rowSize = range / count;
		}

		var values = new double[count]; // Array to store volume for each bin

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
				var binIndex = Math.Clamp((int)((price - low) / rowSize), 0, count - 1);

				values[binIndex] += volumePerPriceLevel;
			}
		}

		// Assign final results
		FromIndex = fromIndex;
		ToIndex = toIndex;
		High = high;
		Low = low;
		Values = values;
		Maximum = values.Max();
		RowSize = rowSize;
	}

	private void Render(IDrawingContext context)
	{
		var highY = ChartScale.GetYCoordinateByValue(High);
		var lowY = ChartScale.GetYCoordinateByValue(Low);
		var pixelsPerUnitY = Math.Abs(highY - lowY) / (High - Low);
		var pointA = new Point(Chart.GetXCoordinateByBarIndex(FromIndex), highY);
		var pointB = new Point(Chart.GetXCoordinateByBarIndex(ToIndex), lowY);

		context.DrawRectangle(pointA, pointB, null, Color.Red);

		for (var i = 0; i < Values.Length; i++)
		{
			var priceLevel = Low + (i * RowSize) + (RowSize / 2);
			var volumeRatio = Maximum > 0 ? Values[i] / Maximum : 0;

			var yCoordinate = ChartScale.GetYCoordinateByValue(priceLevel);
			var width = (pointB.X - pointA.X) * volumeRatio;

				var startPoint = new Point(pointA.X, yCoordinate);
				var endPoint = new Point(pointA.X + width, yCoordinate);

				endPoint.Y += RowSize * pixelsPerUnitY;
				context.DrawRectangle(startPoint, endPoint, "#800000ff", Color.Blue);
		}

		//context.DrawText(new Point(0, 0), $"{RowSize * pixelsPerUnitY}: {(drawLines ? "lines" : "columns")}", Color.White);
	}
}
