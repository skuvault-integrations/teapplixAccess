using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using LINQtoCSV;
using TeapplixAccess.Models.File;

namespace TeapplixAccess.Services
{
	public class TeapplixExportFileParser
	{
		public IList< TeapplixOrder > Parse( Stream source )
		{
			var orders = new List< TeapplixOrder >();
			var cc = new CsvContext();

			var reader = new StreamReader( source );
			orders.AddRange( cc.Read< TeapplixRawDataRow >( reader, new CsvFileDescription { FirstLineHasColumnNames = true } ).Select( this.FillOrder ) );
			return orders;
		}

		private TeapplixOrder FillOrder( TeapplixRawDataRow row )
		{
			var order = new TeapplixOrder();

			order.OrderSource = row[ 0 ].Value ?? string.Empty;
			order.AccountId = row[ 1 ].Value ?? string.Empty;
			order.TnxId = row[ 2 ].Value ?? string.Empty;
			order.TnxId2 = row[ 3 ].Value ?? string.Empty;
			order.Date = string.IsNullOrEmpty( row[ 4 ].Value ) ? DateTime.MinValue : DateTime.Parse( row[ 4 ].Value, CultureInfo.InvariantCulture );
			order.PaymentType = row[ 5 ].Value ?? string.Empty;
			order.PaymentAuthInfo = row[ 6 ].Value ?? string.Empty;
			order.FirstName = row[ 7 ].Value ?? string.Empty;
			order.LastName = row[ 8 ].Value ?? string.Empty;
			order.Email = row[ 9 ].Value ?? string.Empty;
			order.Phone = row[ 10 ].Value ?? string.Empty;
			order.Country = row[ 11 ].Value ?? string.Empty;
			order.State = row[ 12 ].Value ?? string.Empty;
			order.AddressZip = row[ 13 ].Value ?? string.Empty;
			order.City = row[ 14 ].Value ?? string.Empty;
			order.Address1 = row[ 15 ].Value ?? string.Empty;
			order.Address2 = row[ 16 ].Value ?? string.Empty;
			order.Tax = row[ 19 ].Value ?? string.Empty;
			order.Discount = row[ 20 ].Value ?? string.Empty;
			order.Fee = row[ 21 ].Value ?? string.Empty;
			order.Carrier = row[ 23 ].Value ?? string.Empty;
			order.Class = row[ 24 ].Value ?? string.Empty;
			order.Tracking = row[ 25 ].Value ?? string.Empty;
			order.Postage = row[ 26 ].Value ?? string.Empty;
			order.PostageAccount = row[ 27 ].Value ?? string.Empty;
			order.ItemsCount = this.GetItemsCount( row[ 28 ].Value );

			decimal total;
			if( !decimal.TryParse( row[ 17 ].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out total ) )
				order.Total = 0m;

			decimal shipping;
			if( !decimal.TryParse( row[ 18 ].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out shipping ) )
				order.Shipping = 0m;

			DateTime shipDate;
			if( DateTime.TryParse( row[ 22 ].Value, out shipDate ) )
				order.ShipDate = shipDate;

			var items = new List< TeapplixItem >();
			for( var i = 0; i < order.ItemsCount; i++ )
			{
				var item = this.LoadItem( row, i );
				items.Add( item );
			}

			order.Total = total;
			order.Shipping = shipping;
			order.Items = items;

			return order;
		}

		private TeapplixItem LoadItem( TeapplixRawDataRow row, int itemNumber )
		{
			var startColumn = 29 + 4 * itemNumber;

			var item = new TeapplixItem
			{
				Descrption = this.GetItemDescription( row, startColumn ),
				Quantity = this.GetItemQuantity( row, startColumn ),
				Sku = this.GetItemSku( row, startColumn ),
				Subtotal = this.GetItemSubtotal( row, startColumn )
			};
			return item;
		}

		private string GetItemDescription( TeapplixRawDataRow row, int startColumn )
		{
			var description = string.Empty;
			if( row.Count > startColumn )
				description = row[ startColumn ].Value;
			return description;
		}

		private int GetItemQuantity( TeapplixRawDataRow row, int startColumn )
		{
			int quantity;
			if( ( row.Count <= startColumn + 1 ) || !int.TryParse( row[ startColumn + 1 ].Value, out quantity ) )
				quantity = 0;
			return quantity;
		}

		private string GetItemSku( TeapplixRawDataRow row, int startColumn )
		{
			var sku = string.Empty;
			if( row.Count > startColumn + 2 )
				sku = row[ startColumn + 2 ].Value;
			return sku;
		}

		private decimal GetItemSubtotal( TeapplixRawDataRow row, int startColumn )
		{
			decimal subtotal;
			if( ( row.Count <= startColumn + 3 ) || !decimal.TryParse( row[ startColumn + 3 ].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out subtotal ) )
				subtotal = 0m;
			return subtotal;
		}

		private int GetItemsCount( string value )
		{
			int count;
			if( int.TryParse( value, out count ) )
				return count;
			return 1;
		}
	}
}