using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LINQtoCSV;
using NUnit.Framework;

namespace TeapplixAccessTests.Services
{
	[TestFixture]
	public class ExportFileParserTest
	{
		[Test]
		public void GetDateValueThrowsException()
		{
			var row = new TeapplixAccess.Models.File.TeapplixRawDataRow();
			row.Add(new DataRowItem("123", 0));

			row.SetColumnToIndexMapping(new Dictionary<string, int>{ {"test", 0 } });
			
			var date = row.GetDate("test");

			var testDelegate = new TestDelegate(() => Console.WriteLine(date.Value));
			Assert.Throws<InvalidOperationException>(testDelegate);
		}

		[Test]
		public void GetDateGetValueOrDefaultDoesNotThrow()
		{
			var row = new TeapplixAccess.Models.File.TeapplixRawDataRow();
			row.Add(new DataRowItem("123", 0));
			row.SetColumnToIndexMapping(new Dictionary<string, int>{ {"test", 0 } });
			
			var date = row.GetDate("test");

			Assert.AreEqual(default(DateTime), date.GetValueOrDefault());
		}
	}
}
