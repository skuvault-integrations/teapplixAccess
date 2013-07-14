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
					var order = new TeapplixOrder
						{
							OrderSource = row[ 0 ].Value,
							AccountId = row[ 1 ].Value,
							TnxId = row[ 2 ].Value,
							TnxId2 = row[ 3 ].Value,
							Date = DateTime.Parse( row[ 4 ].Value,CultureInfo.InvariantCulture ),
							PaymentType = row[ 5 ].Value,
							PaymentAuthInfo = row[ 6 ].Value,
							FirstName = row[ 7 ].Value,
							LastName = row[ 8 ].Value,
							Email = row[ 9 ].Value,
							Phone = row[ 10 ].Value,
							Country = row[ 11 ].Value,
							State = row[ 12 ].Value,
							AddressZip = row[ 13 ].Value,
							City = row[ 14 ].Value,
							Address1 = row[ 15 ].Value,
							Address2 = row[ 16 ].Value,
							Total = decimal.Parse( row[ 17 ].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture ),
							Shipping = decimal.Parse( row[ 18 ].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture ),
							Tax = row[ 19 ].Value,
							Discount = row[ 20 ].Value,
							Fee = row[ 21 ].Value,
							ShipDate = row[ 22 ].Value,
							Carrier = row[ 23 ].Value,
							Class = row[ 24 ].Value,
							Tracking = row[ 25 ].Value,
							Postage = row[ 26 ].Value,
							ItemsCount = int.Parse( row[ 27 ].Value )
						};

					var items = new List< TeapplixItem >();
					for( var i = 0; i < order.ItemsCount; i++ )
					{
						var item = this.LoadItem( row, i );
						items.Add( item );
					}

					order.Items = items;
					orders.Add( order );
				}

				return orders;
			}
		}

		private TeapplixItem LoadItem( TeapplixRawDataRow row, int itemNumber )
		{
			var startColumn = 28 + 4 * itemNumber;
			int quantity;
			if( !int.TryParse( row[ startColumn + 1 ].Value, out quantity ) )
				quantity = 0;

			decimal subtotal;
			if( !decimal.TryParse( row[ startColumn + 3 ].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out subtotal ) )
				subtotal = 0m;

			var item = new TeapplixItem
				{
					Descrption = row[ startColumn ].Value ?? string.Empty,
					Quantity = quantity,
					Sku = row[ startColumn + 2 ].Value ?? string.Empty,
					Subtotal = subtotal
				};
			return item;
		}
	}
}