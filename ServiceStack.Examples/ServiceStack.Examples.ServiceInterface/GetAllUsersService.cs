using ServiceStack.Examples.ServiceInterface.Types;
using ServiceStack.OrmLite;
using ServiceStack.ServiceHost;

namespace ServiceStack.Examples.ServiceInterface
{
	public class GetAllUsersService
		: IService<GetAllUsers>
	{
		//Example of ServiceStack's IOC property injection
		public ExampleConfig Config { get; set; }

		public object Execute(GetAllUsers request)
		{
			using (var dbConn = Config.ConnectionString.OpenDbConnection())
			using (var dbCmd = dbConn.CreateCommand())
			{
				var users = dbCmd.Select<User>();
				return new GetAllUsersResponse { Users = new ArrayOfUser(users) };
			}
		}
	}

}