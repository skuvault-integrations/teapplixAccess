using System;
using System.Collections.Generic;
using System.Globalization;
using LINQtoCSV;

namespace TeapplixAccess.Models.File
{
	internal sealed class TeapplixRawDataRow : List< DataRowItem >, IDataRow
	{
		public void SetColumnToIndexMapping( IDictionary<string, int> columnIndexByHeader )
		{
			this.columnIndexByHeader = columnIndexByHeader;
		}
		
		public string GetString( string columnName )
		{
			if ( !this.columnIndexByHeader.TryGetValue( columnName, out var columnIndex ) )
			{
				throw new IndexOutOfRangeException( $"Non-existing CSV column '{columnName}' requested" );
			}
			return this[ columnIndex ].Value ?? string.Empty;
		}
		
		public DateTime? GetDate( string columnName )
		{
			var value = this.GetString( columnName );
			return DateTime.TryParse( value, out var date ) ? date : ( DateTime? ) null;
		}
		
		public decimal? GetDecimal( string columnName )
		{
			var value = this.GetString( columnName );
			return decimal.TryParse( value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var result )
				? result : ( decimal? ) null;
		}
		
		private IDictionary< string, int > columnIndexByHeader;
	}
}