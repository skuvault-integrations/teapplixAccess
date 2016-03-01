using System.Collections.Generic;
using System.IO;
using LINQtoCSV;
using TeapplixAccess.Models;
using TeapplixAccess.Models.File;

namespace TeapplixAccess.Services
{
	public class TeapplixUploadResponseParser
	{
		public IEnumerable< TeapplixInventoryUploadResponse > Parse( Stream source )
		{
			var result = new List< TeapplixInventoryUploadResponse >();

			var reader = new StreamReader( source );
			var cc = new CsvContext();
			foreach( var row in cc.Read< TeapplixRawDataRow >( reader, new CsvFileDescription { FirstLineHasColumnNames = true } ) )
			{
				result.Add( new TeapplixInventoryUploadResponse( row[ 0 ].Value, row[ 1 ].Value, row[ 2 ].Value ) );
			}

			return result;
		}
	}
}