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
		public void Report_Downloaded()
		{
			var service = this.TeapplixFactory.CreateService( new TeapplixCredentials( this.Credentials.AccountName, this.Credentials.Login, this.Credentials.Password ) );
			var report = service.GetCustomerReport( new TeapplixReportConfig( TeapplixReportSubaction.CustomerRunReport, new DateTime( 2010, 1, 22 ), new DateTime( 2010, 1, 25 ),
				new DateTime( 2010, 1, 22 ), new DateTime( 2010, 1, 25 ) ) );

			report.ToList()[ 0 ].TnxId.Should().Be( "51940626" );
		}

		[ Test ]
		public void Report_DownloadedAsync()
		{
			var service = this.TeapplixFactory.CreateService( new TeapplixCredentials( this.Credentials.AccountName, this.Credentials.Login, this.Credentials.Password ) );
			var report = service.GetCustomerReportAsync( new TeapplixReportConfig( TeapplixReportSubaction.CustomerRunReport, new DateTime( 2010, 1, 22 ), new DateTime( 2010, 1, 25 ),
				new DateTime( 2010, 1, 22 ), new DateTime( 2010, 1, 25 ) ) );

			report.Result.ToList()[ 0 ].TnxId.Should().Be( "51940626" );
		}
	}
}