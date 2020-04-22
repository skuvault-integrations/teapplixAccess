using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using LINQtoCSV;
using Netco.Extensions;
using TeapplixAccess.Models.File;

namespace TeapplixAccess.Services
{
	public class TeapplixExportFileParser
	{
		public IList< TeapplixOrder > Parse( Stream source )
		{
			var cc = new CsvContext();
			var reader = new StreamReader( source );
			IDictionary< string, int > columnIndexByHeader = null;
			return cc.Read< TeapplixRawDataRow >( reader, new CsvFileDescription { FirstLineHasColumnNames = false } )
				.Select( (row, ix) => {
					if ( ix == 0 )
					{
						// saving column headers to a dictionary to be able to address cells by name instead of index
						// while parsing data rows
						columnIndexByHeader = row.Where( i => i.Value != null ).Select( i => i.Value ).ToIndexDictionary();
						return null;
					}

					TeapplixOrder order = null;
					
					try
					{
						order = this.ExtractOrder( row, columnIndexByHeader );
					}
					catch( Exception ex )
					{
						LogServices.Logger.Error( ex, "Order line has errors" );
					}

					return order;
				})
				.Where( o => o != null )
				.ToList();
		}

		private TeapplixOrder ExtractOrder( TeapplixRawDataRow row, IDictionary<string, int> columnIndexByHeader )
		{
			row.SetColumnToIndexMapping( columnIndexByHeader );

			var date = row.GetDate( "date" );
			var total = row.GetDecimal( "total" );
			var order = new TeapplixOrder
			{
				OrderSource =		row.GetString( "order_source" ),
				AccountId =			row.GetString( "account" ),
				TnxId =			row.GetString( "txn_id" ),
				TnxId2 =			row.GetString( "txn_id2" ),
				Date = 			date.GetValueOrDefault(),
				PaymentStatus =		row.GetString( "payment_status" ),
				PaymentType =		row.GetString( "payment_type" ),
				PaymentAuthInfo =		row.GetString( "payment_auth_info" ),
				FirstName =			row.GetString( "first_name" ),
				LastName = 			row.GetString( "last_name" ),
				Email = 			row.GetString( "payer_email" ),
				Phone =			row.GetString( "contact_phone" ),
				Country =			row.GetString( "address_country" ),
				State =			row.GetString( "address_state" ),
				AddressZip =		row.GetString( "address_zip" ),
				City =			row.GetString( "address_city" ),
				Address1 =			row.GetString( "address_street" ),
				Address2 =			row.GetString( "address_street2" ),
				Currency =			row.GetString( "currency" ),
				Total = 			total.GetValueOrDefault(),
				Shipping =			row.GetDecimal( "shipping" ),
				Tax = 			row.GetString( "tax" ),
				Discount = 			row.GetString( "discount" ),
				Fee = 			row.GetString( "fee" ),
				ShipDate =			row.GetDate( "ship_date" ),
				Carrier =			row.GetString( "carrier" ),
				Class =			row.GetString( "method" ),
				Weight =			row.GetString( "weight" ),
				Tracking =			row.GetString( "tracking" ),
				Postage =			row.GetString( "postage" ),
				PostageAccount =		row.GetString( "postage_account" ),
				QueueId =			row.GetString( "queue_id" ),
				Insurance =			row.GetString( "insurance" ),
				InsuranceValue =		row.GetString( "insurance_value" ),
				InsuranceFee =		row.GetString( "insurance_fee" ),
				ItemsCount =		row.GetInt( "num_order_lines" )
			};

			if ( !date.HasValue )
			{
				throw new Exception( string.Format("Order {0} for account {1} has missing order date", order.TnxId, order.AccountId ) );
			}

			if ( !total.HasValue )
			{
				throw new Exception( string.Format( "Order {0} for account {1} has missing order total", order.TnxId, order.AccountId ) );
			}

			if( order.ItemsCount > 1000 )
			{
				throw new Exception( string.Format( "Order {0} for account {1}. Exceeded the maximum limit of items", order.TnxId, order.AccountId ) );
			}
	
			// number of items always precedes the cells with the items info
			int itemsStartColumnIndex = columnIndexByHeader[ "num_order_lines" ] + 1;
			var totalColumnsCount = itemsStartColumnIndex + order.ItemsCount * 4;
			
			if ( row.Count != totalColumnsCount )
			{
				throw new Exception( string.Format( "Order {0} for account {1}. Number of columns in a row does not match specified items count", order.TnxId, order.AccountId ) );
			}

			order.Items = new List< TeapplixItem >();
			for( var i = itemsStartColumnIndex; i < totalColumnsCount; i += 4 )
			{
				var item = new TeapplixItem
				{
					Description = row[ i ].Value ?? string.Empty,
					Quantity = int.TryParse( row[ i + 1 ].Value, out var quantity ) ? quantity : 0,
					Sku = row[ i + 2 ].Value,
					Subtotal = decimal.TryParse( row[ i + 3 ].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var subtotal )
						? subtotal
						: 0m
				};
				order.Items.Add( item );
			}

			return order;
		}
	}
}
