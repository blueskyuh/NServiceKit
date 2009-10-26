namespace ServiceStack.LogicFacade
{
	/// <summary>
	/// Implement on services that need access to the RequestContext
	/// </summary>
	public interface IRequiresRequestContext
	{
		IRequestContext RequestContext { get; set; }
	}
}