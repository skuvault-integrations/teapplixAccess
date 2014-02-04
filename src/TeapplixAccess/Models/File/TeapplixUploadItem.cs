using LINQtoCSV;

namespace TeapplixAccess.Models.File
{
	public sealed class TeapplixUploadItem
	{
		[ CsvColumn( Name = "Post Date", FieldIndex = 1 ) ]
		public string PostDate { get; set; }

		[ CsvColumn( Name = "Post Type", FieldIndex = 2 ) ]
		public string PostType { get; set; }

		[ CsvColumn( Name = "Post Comment", FieldIndex = 3 ) ]
		public string PostComment { get; set; }

		[ CsvColumn( Name = "SKU", FieldIndex = 4 ) ]
		public string SKU { get; set; }

		[ CsvColumn( Name = "Quantity", FieldIndex = 5 ) ]
		public string Quantity { get; set; }

		[ CsvColumn( Name = "Unit Price", FieldIndex = 6 ) ]
		public string Total { get; set; }

		[ CsvColumn( Name = "Location", FieldIndex = 7 ) ]
		public string Location { get; set; }
	}
}