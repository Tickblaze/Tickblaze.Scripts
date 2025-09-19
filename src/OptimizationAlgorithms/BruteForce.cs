namespace Tickblaze.Scripts.OptimizationAlgorithms;

public class BruteForce : OptimizationAlgorithm
{
	public override int MaxOptimizations => Vectors.Count;

	public BruteForce()
	{
		Name = "Brute Force";
	}

	public override IEnumerable<OptimizationVector> GetNextVectorsToRun(int maxCount)
	{
		return Vectors
			.Where(v => !v.IsProcessed)
			.Take(maxCount);
	}
}