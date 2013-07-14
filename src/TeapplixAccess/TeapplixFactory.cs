using TeapplixAccess.Models;

namespace TeapplixAccess
{
	public interface ITeapplixFactory
	{
		ITeapplixService CreateService( TeapplixCredentials credentials );
	}

	public sealed class TeapplixFactory : ITeapplixFactory
	{
		public ITeapplixService CreateService( TeapplixCredentials credentials )
		{
			return new TeapplixService( credentials );
		}
	}
}