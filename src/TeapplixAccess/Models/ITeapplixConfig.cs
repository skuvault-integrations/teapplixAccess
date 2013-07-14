using System;

namespace TeapplixAccess.Models
{
	public interface ITeapplixConfig
	{
		Uri GetServiceUrl( TeapplixCredentials credentials );
	}
}