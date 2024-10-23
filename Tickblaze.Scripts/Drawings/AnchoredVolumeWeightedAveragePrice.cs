namespace Tickblaze.Scripts.Drawings;

public sealed class AnchoredVolumeWeightedAveragePrice : VWAPBase
{
	public override int PointsCount => 1;

	private int _priorLeftIndex;

	public AnchoredVolumeWeightedAveragePrice()
	{
		Name = "Anchored VWAP";
	}

	public override void OnRender(IDrawingContext context)
	{
		var leftIndex = 0;
		var rightIndex = 0;
		var validBarIndexes = CalculateLeftAndRightIndexes(ref leftIndex, ref rightIndex, Points[0].X);

		if (validBarIndexes == null || validBarIndexes.Length == 0 || leftIndex >= rightIndex - 1 || Points[0].X > Chart.GetXCoordinateByBarIndex(Bars.Count - 1))
		{
			return;
		}

		CalculateAndPlotVWAP(context, validBarIndexes);

		if(_priorLeftIndex != leftIndex)
		{
			Points[0].Y = ChartScale.GetYCoordinateByValue(Bars[leftIndex].Close);
			_priorLeftIndex = leftIndex;
		}
	}
}