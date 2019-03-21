using System;
using FluentAssertions;
using NUnit.Framework;
using TeapplixAccess.Models;

namespace TeapplixAccessTests.Core
{
	[ TestFixture ]
	public class ConfigTests
	{
		[ Test ]
		public void ReportUrlContainsCorrectDates()
		{
			var credentials = new TeapplixCredentials( "testaccount", "testuser", "testpassword" );
			var newOrdersDateConfig = new TeapplixReportConfig( TeapplixReportSubaction.CustomerRunReport, new DateTime( 2010, 1, 22 ), new DateTime( 2010, 1, 25 ), null, null );
			var snippedOrdersConfig = new TeapplixReportConfig( TeapplixReportSubaction.CustomerRunReport, null, null,
				new DateTime( 2013, 7, 13 ), new DateTime( 2013, 7, 14 ) );
			var fullOrdersDateConfig = new TeapplixReportConfig( TeapplixReportSubaction.CustomerRunReport, new DateTime( 2010, 1, 22 ), new DateTime( 2010, 1, 25 ),
				new DateTime( 2013, 7, 13 ), new DateTime( 2013, 7, 14 ) );

			newOrdersDateConfig.GetServiceUrl( credentials ).Should().Be( "https://app.teapplix.com/h/testaccount/ea/admin.php?User=testuser&Passwd=testpassword&Action=Report&Subaction=CustomerRun&start_date=2010/01/22&end_date=2010/01/25" );
			snippedOrdersConfig.GetServiceUrl( credentials ).Should().Be( "https://app.teapplix.com/h/testaccount/ea/admin.php?User=testuser&Passwd=testpassword&Action=Report&Subaction=CustomerRun&ship_date_s=2013/07/13&ship_date_e=2013/07/14" );
			fullOrdersDateConfig.GetServiceUrl( credentials ).Should().Be( "https://app.teapplix.com/h/testaccount/ea/admin.php?User=testuser&Passwd=testpassword&Action=Report&Subaction=CustomerRun&start_date=2010/01/22&end_date=2010/01/25&ship_date_s=2013/07/13&ship_date_e=2013/07/14" );
		}

		[ Test ]
		public void UploadUrlContainsCorrectSettings()
		{
			var credentoals = new TeapplixCredentials( "testaccount", "testuser", "testpassword" );
			var uploadWithoutAdditionalParamsConfig = new TeapplixUploadConfig( TeapplixUploadSubactionEnum.Inventory, false, false );
			var uploadWitAdditionalParamsConfig = new TeapplixUploadConfig( TeapplixUploadSubactionEnum.Inventory, true, true );

			uploadWithoutAdditionalParamsConfig.GetServiceUrl( credentoals ).Should().Be( "https://app.teapplix.com/h/testaccount/ea/api.php?User=testuser&Passwd=testpassword&Action=Upload&Subaction=Inventory" );
			uploadWitAdditionalParamsConfig.GetServiceUrl( credentoals ).Should().Be( "https://app.teapplix.com/h/testaccount/ea/api.php?User=testuser&Passwd=testpassword&Action=Upload&Subaction=Inventory&im_chk_clear=1&im_xref_action=create_product_auto" );

		}
	}
}