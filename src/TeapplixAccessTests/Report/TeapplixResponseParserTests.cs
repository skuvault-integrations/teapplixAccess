using System.IO;
using FluentAssertions;
using NUnit.Framework;
using TeapplixAccess.Services;
using TeapplixAccessTests.Properties;

namespace TeapplixAccessTests.Report
{
	[ TestFixture ]
	public class TeapplixResponseParserTests
	{
		private Stream FileWithData;
		private Stream FileWithoutData;

		[ SetUp ]
		public void Init()
		{
			this.FileWithData = new MemoryStream( Resource.teapplix_orders_export );
			this.FileWithoutData = new MemoryStream( Resource.teapplix_empty_orders_export );
		}

		[ TearDown ]
		public void Cleanup()
		{
			this.FileWithData.Dispose();
			this.FileWithoutData.Dispose();
		}

		[ Test ]
		public void ContentExists_Parsed()
		{
			var parser = new TeapplixExportFileParser();
			var orders = parser.Parse( this.FileWithData );

			orders[ 0 ].Items[ 0 ].Sku.Should().Be( "Con CD160" );
			orders[ 1 ].TnxId.Should().Be( "108-2232901-5730637" );
			orders[ 2 ].Items[ 0 ].Sku.Should().Be( "Con CD122" );

			orders.Count.Should().Be( 3 );
			orders[ 0 ].Items.Count.Should().Be( 1 );
			orders[ 1 ].Items.Count.Should().Be( 1 );
			orders[ 1 ].Items[ 0 ].Sku.Should().Be( "Con Jewler" );
			orders[ 2 ].Items[ 0 ].Subtotal.Should().Be( 28.99m );

			orders[ 0 ].Items[ 0 ].Descrption.Should().Be( "Conair Hot Air Curling Combo, 1.5 Inch" );
			orders[ 1 ].Items[ 0 ].Descrption.Should().Be( "Conair HJ3BC Quick Gems Hair Jeweler" );

			orders[ 0 ].IsShipped.Should().BeTrue();
			orders[ 1 ].IsShipped.Should().BeFalse();
		}

		[ Test ]
		public void ContentDoesNotExist_EmptyResult()
		{
			var parser = new TeapplixExportFileParser();
			var orders = parser.Parse( this.FileWithoutData );

			orders.Should().BeEmpty();
		}
	}
}