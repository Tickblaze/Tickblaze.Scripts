using System.Runtime.InteropServices;
using Tickblaze.Scripts.Api.Interfaces.Orders;

namespace Tickblaze.Scripts.PositionSizers;

public class FixedFractionalPercent : PositionSizer
{
	[NumericRange(1, 100)]
	[Parameter("Fractional", Description = "The percentage of the equity to risk. (1 to 100)")]
	public double Fractional { get; set; } = 2;

	[Parameter("Enable exit sizing", Description = "Use to indicate whether to enable the position sizing script to determine the size of exit orders.")]
	public bool EnableExitSizing { get; set; } = true;

	public FixedFractionalPercent()
	{
		Name = "Fixed Fractional %";
		ShortName = "FF%";
		Description = "This position sizing script is used to allocate a fixed percentage of the account equity to each trade.";
	}

	protected override double GetPositionSize(IOrder order)
	{
		if (EnableExitSizing && Position is not null)
		{
			if (Position.Quantity < order.Quantity)
			{
				return order.Quantity;
			}

			return Position.Quantity;
		}

		var price = Bars.Close[^1];
		if (price <= 0)
		{
			return 0;
		}

		var exchangeRate = GetExchangeRate(Symbol.CurrencyCode, Account.BaseCurrencyCode);
		var size = Math.Floor((Fractional / 100.0 * Account.Equity) / (exchangeRate * price));

		return size;
	}
}
