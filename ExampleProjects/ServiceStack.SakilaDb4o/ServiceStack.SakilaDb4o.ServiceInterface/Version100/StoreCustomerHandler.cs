using Sakila.ServiceModel.Version100.Operations;
using ServiceStack.DataAccess;
using ServiceStack.LogicFacade;
using ServiceStack.ServiceInterface;
using ServiceStack.SakilaDb4o.ServiceInterface.Translators;

namespace ServiceStack.SakilaDb4o.ServiceInterface.Version100
{
	public class StoreCustomerHandler : IService
	{

		public object Execute(IOperationContext context)
		{
			var request = context.Request.Get<StoreCustomer>();
			var provider = context.Application.Get<IPersistenceProviderManager>().GetProvider();

			var customer = request.Customer.ToModel();

			var response = new StoreCustomerResponse {
				ResponseStatus = ResponseStatusTranslator.Instance.Parse(customer.Validate())
			};

			//Only store valid Customers. 			
			if (response.ResponseStatus.ErrorCode == null)
			{
				//Ideally this 'Store' request should be persisted before execution
				//and executed Asynchronously after the response is returned to the client
				using (var transaction = provider.BeginTransaction())
				{
					provider.Store(customer);
					transaction.Commit();
				}
			}

			return response;
		}

	}
}
