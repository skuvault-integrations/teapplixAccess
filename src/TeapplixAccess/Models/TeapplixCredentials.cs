using CuttingEdge.Conditions;

namespace TeapplixAccess.Models
{
	public sealed class TeapplixCredentials
	{
		public string AccountName { get; private set; }
		public string UserName { get; private set; }
		public string Password { get; private set; }

		public TeapplixCredentials( string accountName, string userName, string password )
		{
			Condition.Requires( accountName, "accountName" ).IsNotNullOrEmpty();
			Condition.Requires( userName, "userName" ).IsNotNullOrEmpty();
			Condition.Requires( password, "password" ).IsNotNullOrEmpty();

			this.AccountName = accountName;
			this.UserName = userName;
			this.Password = password;
		}
	}
}