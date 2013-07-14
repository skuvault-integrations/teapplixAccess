using System;
using Netco.ActionPolicyServices;
using Netco.Logging;
using Netco.Utils;

namespace TeapplixAccess.Misc
{
	public static class ActionPolicies
	{
		public static ActionPolicy TeapplixGetPolicy
		{
			get { return _teapplixGetPolicy; }
		}

		private static readonly ActionPolicy _teapplixGetPolicy = ActionPolicy.Handle< Exception >().Retry( 10, ( ex, i ) =>
			{
				typeof( ActionPolicies ).Log().Trace( ex, "Retrying Teapplix API get call for the {0} time", i );
				SystemUtil.Sleep( TimeSpan.FromSeconds( 0.5 + i ) );
			} );

		public static ActionPolicy TeapplixSubmitPolicy
		{
			get { return _teapplixSumbitPolicy; }
		}

		private static readonly ActionPolicy _teapplixSumbitPolicy = ActionPolicy.Handle< Exception >().Retry( 10, ( ex, i ) =>
			{
				typeof( ActionPolicies ).Log().Trace( ex, "Retrying Teapplix API submit call for the {0} time", i );
				SystemUtil.Sleep( TimeSpan.FromSeconds( 0.5 + i ) );
			} );
	}
}