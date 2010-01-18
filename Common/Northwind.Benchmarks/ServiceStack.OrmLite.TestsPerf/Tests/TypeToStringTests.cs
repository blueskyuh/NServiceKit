using System;
using System.Collections.Generic;
using Northwind.Common.ComplexModel;
using Northwind.Common.ServiceModel;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using ServiceStack.Common.Extensions;
using ServiceStack.Common.Utils;

namespace ServiceStack.OrmLite.TestsPerf.Tests
{
	[Ignore]
	[TestFixture]
	public class TypeToStringTests
		: TextTestBase
	{

		readonly CustomerDto customer = NorthwindDtoFactory.Customer(
		1.ToString("x"), "Alfreds Futterkiste", "Maria Anders", "Sales Representative", "Obere Str. 57",
		"Berlin", null, "12209", "Germany", "030-0074321", "030-0076545", null);

		readonly OrderDto order = NorthwindDtoFactory.Order(
			1, "VINET", 5, new DateTime(1996, 7, 4), new DateTime(1996, 1, 8), new DateTime(1996, 7, 16),
			3, 32.38m, "Vins et alcools Chevalier", "59 rue de l'Abbaye", "Reims", null, "51100", "France");

		readonly OrderDto order2 = NorthwindDtoFactory.Order(
			2, "VINET", 5, new DateTime(1996, 7, 4), new DateTime(1996, 1, 8), new DateTime(1996, 7, 16),
			3, 32.38m, "Vins et alcools Chevalier", "59 rue de l'Abbaye", "Reims", null, "51100", "France");

		readonly OrderDto order3 = NorthwindDtoFactory.Order(
			2, "VINET", 5, new DateTime(1996, 7, 4), new DateTime(1996, 1, 8), new DateTime(1996, 7, 16),
			3, 32.38m, "Vins et alcools Chevalier", "59 rue de l'Abbaye", "Reims", null, "51100", "France");

		readonly SupplierDto supplier = NorthwindDtoFactory.Supplier(
			1, "Exotic Liquids", "Charlotte Cooper", "Purchasing Manager", "49 Gilbert St.", "London", null,
			"EC1 4SD", "UK", "(171) 555-2222", null, null);

		[Test]
		public void Can_deserialize_CustomerDto()
		{
			var dtoString = StringConverterUtils.ToString(customer);

			Log(dtoString);

			var newDto = StringConverterUtils.Parse<CustomerDto>(dtoString);

			Assert.That(customer.Equals(newDto), Is.True);
		}

		[Test]
		public void Can_deserialize_OrderDto()
		{
			var dtoString = StringConverterUtils.ToString(order);

			Log(dtoString);

			var newDto = StringConverterUtils.Parse<OrderDto>(dtoString);

			Assert.That(order.Equals(newDto), Is.True);
		}

		[Test]
		public void Can_deserialize_SupplierDto()
		{
			var dtoString = StringConverterUtils.ToString(supplier);

			Log(dtoString);

			var newDto = StringConverterUtils.Parse<SupplierDto>(dtoString);

			Assert.That(supplier.Equals(newDto), Is.True);
		}

		[Test]
		public void Can_deserialize_MultiDto()
		{
			var multiDto = new MultiDto { Id = Guid.NewGuid(), Customer = customer, Supplier = supplier, };

			var dtoString = StringConverterUtils.ToString(multiDto);

			Log(dtoString);

			var newDto = StringConverterUtils.Parse<MultiDto>(dtoString);

			Assert.That(multiDto.Equals(newDto), Is.True);
		}

		[Test]
		public void Can_deserialize_MultiDtoWithOrders()
		{
			var multiDto = DtoFactory.MultiDtoWithOrders;

			var dtoString = StringConverterUtils.ToString(multiDto);

			Log(dtoString);

			var newDto = StringConverterUtils.Parse<MultiDtoWithOrders>(dtoString);

			Assert.That(multiDto.Equals(newDto), Is.True);
		}

		[Test]
		public void Can_deserialize_MultiOrderProperties()
		{
			var multiDto = DtoFactory.MultiOrderProperties;

			var dtoString = StringConverterUtils.ToString(multiDto);

			Log(dtoString);

			var newDto = StringConverterUtils.Parse<MultiOrderProperties>(dtoString);

			Assert.That(multiDto.Equals(newDto), Is.True);
		}

		[Test]
		public void Can_deserialize_MultiCustomerProperties()
		{
			var multiDto = DtoFactory.MultiCustomerProperties;

			var dtoString = StringConverterUtils.ToString(multiDto);

			Log(dtoString);

			var newDto = StringConverterUtils.Parse<MultiCustomerProperties>(dtoString);

			Assert.That(multiDto.Equals(newDto), Is.True);
		}

		[Test]
		public void Can_deserialize_ArrayDtoWithOrders()
		{
			var arrayDto = DtoFactory.ArrayDtoWithOrders;

			var dtoString = StringConverterUtils.ToString(arrayDto);

			Log(dtoString);

			var newDto = StringConverterUtils.Parse<ArrayDtoWithOrders>(dtoString);

			Assert.That(arrayDto.Equals(newDto), Is.True);
		}

		[Test]
		public void Can_deserialize_CustomerOrderListDto()
		{
			var dto = DtoFactory.CustomerOrderListDto;

			var dtoString = StringConverterUtils.ToString(dto);

			Log(dtoString);

			var newDto = StringConverterUtils.Parse<CustomerOrderListDto>(dtoString);

			Assert.That(dto.Equals(newDto), Is.True);
		}

		[Test]
		public void Can_deserialize_CustomerOrderArrayDto()
		{
			var dto = DtoFactory.CustomerOrderArrayDto;

			var dtoString = StringConverterUtils.ToString(dto);

			Log(dtoString);

			var newDto = StringConverterUtils.Parse<CustomerOrderArrayDto>(dtoString);

			Assert.That(dto.Equals(newDto), Is.True);
		}

		[Test]
		public void profile_Serialize_MultiDtoWithOrders()
		{
			var dto = DtoFactory.MultiDtoWithOrders;
			100.Times(i => StringConverterUtils.ToString(dto));
		}

		[Test]
		public void profile_Deserialize_MultiDtoWithOrders()
		{
			const string dtoString = "{Id=f8e1b4a8-a8d2-4d39-a92e-426809821474	Customer={Id=1	CompanyName=Alfreds Futterkiste	ContactName=Maria Anders	ContactTitle=Sales Representative	Address=Obere Str. 57	City=Berlin	Region=	PostalCode=12209	Country=Germany	Phone=030-0074321	Fax=030-0076545	Picture=}	Supplier={Id=1	CompanyName=Exotic Liquids	ContactName=Charlotte Cooper	ContactTitle=Purchasing Manager	Address=49 Gilbert St.	City=London	Region=	PostalCode=EC1 4SD	Country=UK	Phone=(171) 555-2222	Fax=	HomePage=}	Orders={Id=1	CustomerId=VINET	EmployeeId=5	OrderDate=04/07/1996 00:00:00	RequiredDate=08/01/1996 00:00:00	ShippedDate=16/07/1996 00:00:00	ShipVia=3	Freight=32.38	ShipName=Vins et alcools Chevalier	ShipAddress=59 rue de l'Abbaye	ShipCity=Reims	ShipRegion=	ShipPostalCode=51100	ShipCountry=France},{Id=2	CustomerId=VINET	EmployeeId=5	OrderDate=04/07/1996 00:00:00	RequiredDate=08/01/1996 00:00:00	ShippedDate=16/07/1996 00:00:00	ShipVia=3	Freight=32.38	ShipName=Vins et alcools Chevalier	ShipAddress=59 rue de l'Abbaye	ShipCity=Reims	ShipRegion=	ShipPostalCode=51100	ShipCountry=France},{Id=3	CustomerId=VINET	EmployeeId=5	OrderDate=04/07/1996 00:00:00	RequiredDate=08/01/1996 00:00:00	ShippedDate=16/07/1996 00:00:00	ShipVia=3	Freight=32.38	ShipName=Vins et alcools Chevalier	ShipAddress=59 rue de l'Abbaye	ShipCity=Reims	ShipRegion=	ShipPostalCode=51100	ShipCountry=France},{Id=4	CustomerId=VINET	EmployeeId=5	OrderDate=04/07/1996 00:00:00	RequiredDate=08/01/1996 00:00:00	ShippedDate=16/07/1996 00:00:00	ShipVia=3	Freight=32.38	ShipName=Vins et alcools Chevalier	ShipAddress=59 rue de l'Abbaye	ShipCity=Reims	ShipRegion=	ShipPostalCode=51100	ShipCountry=France}}";
			var newDto = StringConverterUtils.Parse<MultiDtoWithOrders>(dtoString);
		}

	}
}