using ServiceStack.DataAccess;
using ServiceStack.DesignPatterns.Command;
using ServiceStack.LogicFacade;

namespace ServiceStack.SakilaNHibernate.Logic
{
	public interface IAction<ReturnType> : ICommand<ReturnType>
	{
		IPersistenceProvider Provider { get; set; }
		IApplicationContext AppContext { get; set; }
	}
}