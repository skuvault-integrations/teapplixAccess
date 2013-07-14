using LINQtoCSV;

namespace TeapplixAccessTests
{
	internal class TestCredentials
	{
		[ CsvColumn( Name = "AccountName", FieldIndex = 1 ) ]
		public string AccountName { get; set; }

		[ CsvColumn( Name = "Login", FieldIndex = 2 ) ]
		public string Login { get; set; }

		[CsvColumn(Name = "Password", FieldIndex = 3)]
		public string Password { get; set; }
	}
}