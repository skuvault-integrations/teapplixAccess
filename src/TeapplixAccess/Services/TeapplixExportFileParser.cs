using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

			using( var reader = new StreamReader( source ) )
			{
				foreach( var row in cc.Read< TeapplixRawDataRow >( reader, new CsvFileDescription { FirstLineHasColumnNames = true } ) )
				{
					var order = new TeapplixOrder();

					order.OrderSource = row[ 0 ].Value;
					order.AccountId = row[ 1 ].Value;
					order.TnxId = row[ 2 ].Value;
					order.TnxId2 = row[ 3 ].Value;
					order.Date = DateTime.Parse( row[ 4 ].Value, CultureInfo.InvariantCulture );
					order.PaymentType = row[ 5 ].Value;
					order.PaymentAuthInfo = row[ 6 ].Value;
					order.FirstName = row[ 7 ].Value;
					order.LastName = row[ 8 ].Value;
					order.Email = row[ 9 ].Value;
					order.Phone = row[ 10 ].Value;
					order.Country = row[ 11 ].Value;
					order.State = row[ 12 ].Value;
					order.AddressZip = row[ 13 ].Value;
					order.City = row[ 14 ].Value;
					order.Address1 = row[ 15 ].Value;
					order.Address2 = row[ 16 ].Value;
					order.Tax = row[ 19 ].Value;
					order.Discount = row[ 20 ].Value;
					order.Fee = row[ 21 ].Value;
					order.Carrier = row[ 23 ].Value;
					order.Class = row[ 24 ].Value;
					order.Tracking = row[ 25 ].Value;
					order.Postage = row[ 26 ].Value;
					order.ItemsCount = int.Parse( row[ 27 ].Value );

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
					orders.Add( order );
				}

				return orders;
			}
		}

		private TeapplixItem LoadItem( TeapplixRawDataRow row, int itemNumber )
		{
			var startColumn = 28 + 4 * itemNumber;

			var item = new TeapplixItem
				{
					Descrption = GetItemDescription( row, startColumn ),
					Quantity = this.GetItemQuantity( row, startColumn ),
					Sku = this.GetItemSku( row, startColumn ),
					Subtotal = this.GetItemSubtotal( row, startColumn )
				};
			return item;
		}

		private static string GetItemDescription( TeapplixRawDataRow row, int startColumn )
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
	}
}