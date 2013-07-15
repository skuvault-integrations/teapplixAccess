using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using LINQtoCSV;
using NUnit.Framework;
using TeapplixAccess;
using TeapplixAccess.Models;
using TeapplixAccess.Models.File;

namespace TeapplixAccessTests.UploadInventory
{
	[ TestFixture ]
	public class TeapplixUploadInventoryTests
	{
		private List< TeapplixUploadItem > UploadData;
		private readonly ITeapplixFactory TeapplixFactory = new TeapplixFactory();
		private TestCredentials Credentials;
		private FileStream InventoryFileStream;

		[ SetUp ]
		public void Init()
		{
			const string uploadFilePath = @"..\..\Files\upload.csv";
			const string credentialsFilePath = @"..\..\Files\teapplix_test_credentials.csv";

			var cc = new CsvContext();
			this.Credentials = cc.Read< TestCredentials >( credentialsFilePath, new CsvFileDescription { FirstLineHasColumnNames = true } ).FirstOrDefault();
			this.UploadData = cc.Read< TeapplixUploadItem >( uploadFilePath, new CsvFileDescription { FirstLineHasColumnNames = true } ).ToList();

			this.InventoryFileStream = new FileStream( uploadFilePath, FileMode.Open, FileAccess.Read );
		}

		[ Test ]
		public void File_Uploaded()
		{
			var service = this.TeapplixFactory.CreateService( new TeapplixCredentials( this.Credentials.AccountName, this.Credentials.Login, this.Credentials.Password ) );
			var config = new TeapplixUploadConfig( TeapplixUploadSubactionEnum.Inventory, false, false );

			var result = service.InventoryUpload( config, this.InventoryFileStream ).ToList();
			result[ 0 ].Status.Should().Be( InventoryUploadStatusEnum.Success );
			result[ 0 ].Sku.Should().Be( this.UploadData[ 0 ].SKU );
		}

		[ Test ]
		public void Upload_Failed_Product_NotFound()
		{
			var service = this.TeapplixFactory.CreateService( new TeapplixCredentials( this.Credentials.AccountName, this.Credentials.Login, this.Credentials.Password ) );
			var config = new TeapplixUploadConfig( TeapplixUploadSubactionEnum.Inventory, false, false );

			var result = service.InventoryUpload( config, this.InventoryFileStream ).ToList();
			result[ 1 ].Status.Should().Be( InventoryUploadStatusEnum.NotFound );
			result[ 1 ].Sku.Should().Be( this.UploadData[ 1 ].SKU );
		}

		[ Test ]
		public void File_UploadedAsync()
		{
			var service = this.TeapplixFactory.CreateService( new TeapplixCredentials( this.Credentials.AccountName, this.Credentials.Login, this.Credentials.Password ) );
			var config = new TeapplixUploadConfig( TeapplixUploadSubactionEnum.Inventory, false, false );

			var result = service.InventoryUploadAsync( config, this.InventoryFileStream ).Result.ToList();
			result[ 0 ].Status.Should().Be( InventoryUploadStatusEnum.Success );
			result[ 0 ].Sku.Should().Be( this.UploadData[ 0 ].SKU );
		}

		[ Test ]
		public void Upload_Failed_Product_NotFoundAsync()
		{
			var service = this.TeapplixFactory.CreateService( new TeapplixCredentials( this.Credentials.AccountName, this.Credentials.Login, this.Credentials.Password ) );
			var config = new TeapplixUploadConfig( TeapplixUploadSubactionEnum.Inventory, false, false );

			var result = service.InventoryUploadAsync( config, this.InventoryFileStream ).Result.ToList();
			result[ 1 ].Status.Should().Be( InventoryUploadStatusEnum.NotFound );
			result[ 1 ].Sku.Should().Be( this.UploadData[ 1 ].SKU );
		}

		[ Test ]
		public void GetOrders_Update_Then_Upload_Info()
		{
			var service = this.TeapplixFactory.CreateService( new TeapplixCredentials( this.Credentials.AccountName, this.Credentials.Login, this.Credentials.Password ) );
			var report = service.GetCustomerReportAsync( new TeapplixReportConfig( TeapplixReportSubaction.CustomerRunReport, new DateTime( 2010, 1, 22 ), new DateTime( 2010, 1, 25 ),
				new DateTime( 2010, 1, 22 ), new DateTime( 2010, 1, 25 ) ) ).Result.ToList();

			var teapplixUploadItems = new List< TeapplixUploadItem >
				{
					new TeapplixUploadItem
						{
							Location = "location1",
							PostComment = report[ 0 ].Items.First().Descrption,
							PostDate = report[ 0 ].Date.ToString( "yyyy/MM/dd", CultureInfo.InvariantCulture ),
							PostType = TeapplixUploadQuantityPostType.Credit.PostType,
							Quantity = report[ 0 ].Items.First().Quantity.ToString( CultureInfo.InvariantCulture ),
							SKU = "testsku1",
							Total = report[ 0 ].Total.ToString( CultureInfo.InvariantCulture )
						}
				};

			var context = new CsvContext();
			var fileDescription = new CsvFileDescription
				{
					SeparatorChar = ',',
					FirstLineHasColumnNames = true,
				};

			
			using( var ms = new MemoryStream() )
			using( var writer = new StreamWriter( ms, Encoding.UTF8 ) )
			{
				context.Write( teapplixUploadItems, writer, fileDescription );
				writer.Flush();
				ms.Position = 0;
				var result = service.InventoryUploadAsync( new TeapplixUploadConfig( TeapplixUploadSubactionEnum.Inventory, false, false ), ms ).Result.ToList();

				result[ 0 ].Status.Should().Be( InventoryUploadStatusEnum.Success );
				result[ 0 ].Sku.Should().Be( teapplixUploadItems[ 0 ].SKU );
			}
		}
	}
}