using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Funq;

namespace ServiceStack.ServiceHost.Tests.TypeFactory
{
	public class ExpressionFunqlet
		: FunqletBase
	{
		public ExpressionFunqlet(IEnumerable<Type> serviceTypes) : base(serviceTypes) {}

		public ExpressionFunqlet(params Type[] serviceTypes) : base(serviceTypes) {}

		public Func<Container, TService> AutoWire<TService>(MethodInfo resolveFn, ParameterExpression lambdaParam)
		{
			var serviceType = typeof(TService);

			var memberBindings = serviceType.GetProperties()
				.Where(x => x.CanWrite)
				.Select(x =>
					Expression.Bind
					(
						x,
						ResolveTypeExpression(resolveFn, x.PropertyType, lambdaParam)
					)
				).ToArray();

			return Expression.Lambda<Func<Container, TService>>
				(
					Expression.MemberInit
					(
						CtorExpression(resolveFn, serviceType, lambdaParam),
						memberBindings
					),
					lambdaParam
				).Compile();
		}

		private static NewExpression CtorExpression(
			MethodInfo resolveMethodInfo, Type type, Expression lambdaParam)
		{
			var ctorWithMostParameters = GetConstructorWithMostParams(type);

			var constructorParameterInfos = ctorWithMostParameters.GetParameters();
			var regParams = constructorParameterInfos
				.Select(pi => ResolveTypeExpression(resolveMethodInfo, pi.ParameterType, lambdaParam));

			return Expression.New(ctorWithMostParameters, regParams.ToArray());
		}

		private static MethodCallExpression ResolveTypeExpression(
			MethodInfo resolveFn, Type resolveType, Expression lambdaParam)
		{
			var method = resolveFn.MakeGenericMethod(resolveType);
			return Expression.Call(lambdaParam, method);
		}

		public void Register<T>()
		{
			var containerResolveFn = typeof(Container).GetMethod("Resolve", new Type[0]);
			var lambdaParam = Expression.Parameter(typeof(Container), "lambdaContainerParam");

			var serviceFactory = AutoWire<T>(containerResolveFn, lambdaParam);

			this.Container.Register(serviceFactory).ReusedWithin(ReuseScope.None);
		}

		protected override void Run()
		{
			foreach (var serviceType in serviceTypes)
			{
				var methodInfo = GetType().GetMethod("Register", new Type[0]);
				var registerMethodInfo = methodInfo.MakeGenericMethod(new[] { serviceType });
				registerMethodInfo.Invoke(this, new object[0]);
			}
		}
	}
}