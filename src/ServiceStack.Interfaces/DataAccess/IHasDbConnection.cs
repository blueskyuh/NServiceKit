﻿using System.Data;

namespace ServiceStack.DataAccess
{
	public interface IHasDbConnection
	{
		IDbConnection DbConnection { get; }
	}
}