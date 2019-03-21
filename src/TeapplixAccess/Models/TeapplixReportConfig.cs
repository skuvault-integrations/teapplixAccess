using System;
using System.Globalization;
using System.Net;
using CuttingEdge.Conditions;

namespace TeapplixAccess.Models
{
	public sealed class TeapplixReportConfig : ITeapplixConfig
	{
		public TeapplixReportSubaction Subaction { get; private set; }
		public DateTime? PaidOrdersStartUtc { get; private set; }
		public DateTime? PaidOrdersEndUtc { get; private set; }
		public DateTime? ShippedOrdersStartUtc { get; private set; }
		public DateTime? ShippedOrdersEndUtc { get; private set; }

		public TeapplixReportConfig( TeapplixReportSubaction subaction, DateTime? paidOrdersStartUtc, DateTime? paidOrdersEndUtc,
			DateTime? shippedOrdersStartUtc, DateTime? shippedOrdersEndUtc )
		{
			Condition.Requires( subaction, "subaction" ).IsNotNull();
			if( paidOrdersStartUtc.HasValue )
			{
				Condition.Requires( paidOrdersEndUtc, "paidOrdersEndUtc" ).IsNotNull();
				Condition.Requires( paidOrdersStartUtc, "paidOrdersStartUtc" ).IsLessThan( paidOrdersEndUtc );
			}
			if( shippedOrdersStartUtc.HasValue )
			{
				Condition.Requires( shippedOrdersEndUtc, "shippedOrdersEndUtc" ).IsNotNull();
				Condition.Requires( shippedOrdersStartUtc, "shippedOrdersStartUtc" ).IsLessThan( shippedOrdersEndUtc );
			}
			
			this.Subaction = subaction;
			this.PaidOrdersStartUtc = paidOrdersStartUtc;
			this.PaidOrdersEndUtc = paidOrdersEndUtc;
			this.ShippedOrdersStartUtc = shippedOrdersStartUtc;
			this.ShippedOrdersEndUtc = shippedOrdersEndUtc;
		}

		public Uri GetServiceUrl( TeapplixCredentials credentials )
		{
			Condition.Requires( credentials, "credentials" ).IsNotNull();

			var uri = new Uri( string.Format( "https://app.teapplix.com/h/{0}/ea/admin.php?User={1}&Passwd={2}&Action=Report&Subaction={3}{4}",
				// the Teapplix engineer (Evgeniy Bogdanov) states and experiment confirms that account name in url has to be in lower case 
				credentials.AccountName.ToLowerInvariant(),
				credentials.UserName,
				credentials.Password,
				this.Subaction.Subaction,
				this.GetReportSubactionDates() ) );

			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			
			return uri;
		}

		private string GetReportSubactionDates()
		{
			var result = string.Empty;

			if( this.PaidOrdersStartUtc.HasValue && this.PaidOrdersEndUtc.HasValue )
				result += string.Concat( "&start_date=", this.PaidOrdersStartUtc.Value.ToString( "yyyy/MM/dd", CultureInfo.InvariantCulture ), "&end_date=", this.PaidOrdersEndUtc.Value.ToString( "yyyy/MM/dd", CultureInfo.InvariantCulture ) );
			if( this.ShippedOrdersStartUtc.HasValue && this.ShippedOrdersEndUtc.HasValue )
				result += string.Concat( "&ship_date_s=", this.ShippedOrdersStartUtc.Value.ToString( "yyyy/MM/dd", CultureInfo.InvariantCulture ), "&ship_date_e=", this.ShippedOrdersEndUtc.Value.ToString( "yyyy/MM/dd", CultureInfo.InvariantCulture ) );

			return result;
		}

	}
}