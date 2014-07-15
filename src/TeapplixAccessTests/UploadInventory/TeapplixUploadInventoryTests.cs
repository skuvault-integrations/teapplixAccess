using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using LINQtoCSV;
using Netco.Logging;
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

		[ SetUp ]
		public void Init()
		{
			const string uploadFilePath = @"..\..\Files\upload.csv";
			const string credentialsFilePath = @"..\..\Files\teapplix_test_credentials.csv";

			var cc = new CsvContext();
			this.Credentials = cc.Read< TestCredentials >( credentialsFilePath, new CsvFileDescription { FirstLineHasColumnNames = true } ).FirstOrDefault();
			this.UploadData = cc.Read< TeapplixUploadItem >( uploadFilePath, new CsvFileDescription { FirstLineHasColumnNames = true } ).ToList();
			NetcoLogger.LoggerFactory = new ConsoleLoggerFactory();
		}

		[ Test ]
		public void File_Uploaded()
		{
			var service = this.TeapplixFactory.CreateService( new TeapplixCredentials( this.Credentials.AccountName, this.Credentials.Login, this.Credentials.Password ) );

			var result = service.InventoryUpload( this.CreateTeapplixUploadItems() ).ToList();
			result[ 0 ].Status.Should().Be( InventoryUploadStatusEnum.Success );
			result[ 0 ].Sku.Should().Be( this.UploadData[ 0 ].SKU );
		}

		[ Test ]
		public void Upload_Failed_Product_NotFound()
		{
			var service = this.TeapplixFactory.CreateService( new TeapplixCredentials( this.Credentials.AccountName, this.Credentials.Login, this.Credentials.Password ) );

			var result = service.InventoryUpload( this.CreateTeapplixUploadItems() ).ToList();
			result[ 1 ].Status.Should().Be( InventoryUploadStatusEnum.NotFound );
			result[ 1 ].Sku.Should().Be( this.UploadData[ 1 ].SKU );
		}

		[ Test ]
		public void File_UploadedAsync()
		{
			var service = this.TeapplixFactory.CreateService( new TeapplixCredentials( this.Credentials.AccountName, this.Credentials.Login, this.Credentials.Password ) );

			var result = service.InventoryUploadAsync( this.CreateTeapplixUploadItems() ).Result.ToList();
			result[ 0 ].Status.Should().Be( InventoryUploadStatusEnum.Success );
			result[ 0 ].Sku.Should().Be( this.UploadData[ 0 ].SKU );
		}

		[ Test ]
		public void Upload_Failed_Product_NotFoundAsync()
		{
			var service = this.TeapplixFactory.CreateService( new TeapplixCredentials( this.Credentials.AccountName, this.Credentials.Login, this.Credentials.Password ) );

			var result = service.InventoryUploadAsync( this.CreateTeapplixUploadItems() ).Result.ToList();
			result[ 1 ].Status.Should().Be( InventoryUploadStatusEnum.NotFound );
			result[ 1 ].Sku.Should().Be( this.UploadData[ 1 ].SKU );
		}

		private IEnumerable< TeapplixUploadItem > CreateTeapplixUploadItems()
		{
			return new List< TeapplixUploadItem >
			{
				new TeapplixUploadItem
				{
					Location = "DG01 (309), EC05 (19)",
					PostComment = "blablabla1",
					PostDate = DateTime.UtcNow.AddDays( 1 ).ToString( "yyyy/MM/dd", CultureInfo.InvariantCulture ),
					PostType = TeapplixUploadQuantityPostType.InStock.PostType,
					Quantity = "44",
					SKU = "testsku1",
					UnitPrice = "3.24"}
				//},
				//new TeapplixUploadItem
				//{
				//	Location = "location2888",
				//	PostComment = "sdsdsdsdsd",
				//	PostDate = DateTime.UtcNow.AddDays( 2 ).ToString( "yyyy/MM/dd", CultureInfo.InvariantCulture ),
				//	PostType = TeapplixUploadQuantityPostType.Debit.PostType,
				//	Quantity = "267",
				//	SKU = "testsku2",
				//	UnitPrice = "12.84"
				//}
			};

		}
	}
}