using System;
using System.Linq;
using FluentAssertions;
using LINQtoCSV;
using NUnit.Framework;
using TeapplixAccess;
using TeapplixAccess.Models;

namespace TeapplixAccessTests.Report
{
	[ TestFixture ]
	public class TeapplixReportTests
	{
		private readonly ITeapplixFactory TeapplixFactory = new TeapplixFactory();
		private TestCredentials Credentials;

		[ SetUp ]
		public void Init()
		{
			const string credentialsFilePath = @"..\..\Files\teapplix_test_credentials.csv";

			var cc = new CsvContext();
			this.Credentials = cc.Read< TestCredentials >( credentialsFilePath, new CsvFileDescription { FirstLineHasColumnNames = true } ).FirstOrDefault();
		}

		[ Test ]
		public void Report_CreatedOrdersDownloaded()
		{
			var service = this.TeapplixFactory.CreateService( new TeapplixCredentials( this.Credentials.AccountName, this.Credentials.Login, this.Credentials.Password ) );
			var report = service.GetCustomerReport( new TeapplixReportConfig( TeapplixReportSubaction.CustomerRunReport, new DateTime( 2010, 1, 22 ), new DateTime( 2014,5,9 ),
				null, null ) );

			var listResult = report.ToList();
			listResult[ 0 ].TnxId.Should().Be( "51953672" );
		}

		[ Test ]
		public void Report_CreatedOrdersDownloadedAsync()
		{
			var service = this.TeapplixFactory.CreateService( new TeapplixCredentials( this.Credentials.AccountName, this.Credentials.Login, this.Credentials.Password ) );
			var report = service.GetCustomerReportAsync(new TeapplixReportConfig(TeapplixReportSubaction.CustomerRunReport, new DateTime(2014, 3, 26), new DateTime(2014, 5, 20),
				null, null ) );
			var listResult = report.Result.ToList();

			listResult[ 0 ].TnxId.Should().Be( "51953672" );
		}

		[ Test ]
		public void Report_ShippedOrdersDownloaded()
		{
			var service = this.TeapplixFactory.CreateService( new TeapplixCredentials( this.Credentials.AccountName, this.Credentials.Login, this.Credentials.Password ) );
			var report = service.GetCustomerReport( new TeapplixReportConfig( TeapplixReportSubaction.CustomerRunReport, null, null,
				new DateTime(2014, 3, 26), new DateTime(2014, 5, 20)));
			var listResult = report.ToList();

			listResult.Count.Should().Be( 3 );
			listResult[ 0 ].TnxId.Should().Be( "1" );
		}

		[ Test ]
		public void Report_ShippedOrdersDownloadedAsync()
		{
			var service = this.TeapplixFactory.CreateService( new TeapplixCredentials( this.Credentials.AccountName, this.Credentials.Login, this.Credentials.Password ) );
			var report = service.GetCustomerReportAsync( new TeapplixReportConfig( TeapplixReportSubaction.CustomerRunReport, null, null,
				new DateTime( 2013, 7, 13 ), new DateTime( 2013, 7, 14 ) ) );
			var listResult = report.Result.ToList();

			listResult.Count.Should().Be( 3 );
			listResult[ 0 ].TnxId.Should().Be( "1" );
		}

		[ Test ]
		public void Report_ShippedAndCreatedOrdersDownloaded()
		{
			var service = this.TeapplixFactory.CreateService( new TeapplixCredentials( this.Credentials.AccountName, this.Credentials.Login, this.Credentials.Password ) );
			var newOrdersReport = service.GetCustomerReportAsync( new TeapplixReportConfig( TeapplixReportSubaction.CustomerRunReport, new DateTime( 2010, 1, 22 ), new DateTime( 2010, 1, 25 ),
				null, null ) );
			var shippedOrdersReport = service.GetCustomerReportAsync( new TeapplixReportConfig( TeapplixReportSubaction.CustomerRunReport, null, null,
				new DateTime( 2013, 7, 13 ), new DateTime( 2013, 7, 14 ) ) );
			var shippedOrdersReportList = shippedOrdersReport.Result.ToList();
			var newOrdersReportList = newOrdersReport.Result.ToList();
			var listResult = newOrdersReportList.Union( shippedOrdersReportList ).ToList();

			shippedOrdersReportList.Count.Should().Be( 3 );
			newOrdersReportList.Count.Should().Be( 5 );
			listResult[ 0 ].TnxId.Should().Be( "51953672" );
			listResult[ 5 ].TnxId.Should().Be( "1" );
		}
	}
}