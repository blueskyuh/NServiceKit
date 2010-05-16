using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using RedisWebServices.ServiceModel.Operations.Set;

namespace RedisWebServices.Tests
{
	public class SetOperationsTests
		: TestBase
	{
		private const string SetId = "testset";
		private const string SetId2 = "testset2";
		private const string SetId3 = "testset3";

		private List<string> stringList;
		private List<string> stringList2;
		private List<string> stringList3;

		[SetUp]
		public override void OnBeforeEachTest()
		{
			base.OnBeforeEachTest();
			stringList = new List<string> { "one", "two", "three", "four" };
			stringList2 = new List<string> { "four", "five", "six", "seven" };
			stringList3 = new List<string> { "one", "five", "seven", "eleven" };
		}

		[Test]
		public void Test_AddItemToSet()
		{
			var response = base.Send<AddItemToSetResponse>(
				new AddItemToSet { Id = SetId, Item = TestValue }, x => x.ResponseStatus);

			var value = RedisExec(r => r.PopItemFromSet(SetId));

			Assert.That(value, Is.EqualTo(TestValue));
		}

		[Test]
		public void Test_GetAllItemsFromSet()
		{
			stringList.ForEach(x =>
				RedisExec(r => r.AddItemToSet(SetId, x)));

			var response = base.Send<GetAllItemsFromSetResponse>(
				new GetAllItemsFromSet { Id = SetId }, x => x.ResponseStatus);

			Assert.That(response.Items, Is.EquivalentTo(stringList));
		}

		[Test]
		public void Test_GetDifferencesFromSet()
		{
			stringList.ForEach(x =>
				RedisExec(r => r.AddItemToSet(SetId, x)));
			stringList2.ForEach(x =>
				RedisExec(r => r.AddItemToSet(SetId2, x)));
			stringList3.ForEach(x =>
				RedisExec(r => r.AddItemToSet(SetId3, x)));

			var response = base.Send<GetDifferencesFromSetResponse>(
				new GetDifferencesFromSet { Id = SetId, SetIds = { SetId2, SetId3 } }, x => x.ResponseStatus);

			Assert.That(response.Items, Is.EquivalentTo(new List<string> { "two", "three" }));
		}

		[Test]
		public void Test_GetIntersectFromSets()
		{
			stringList.ForEach(x =>
				RedisExec(r => r.AddItemToSet(SetId, x)));
			stringList2.ForEach(x =>
				RedisExec(r => r.AddItemToSet(SetId2, x)));

			var response = base.Send<GetIntersectFromSetsResponse>(
				new GetIntersectFromSets { SetIds = { SetId, SetId2 } }, x => x.ResponseStatus);

			Assert.That(response.Items, Is.EquivalentTo(new List<string> { "four" }));
		}

		[Test]
		public void Test_GetRandomItemFromSet()
		{
			stringList.ForEach(x =>
				RedisExec(r => r.AddItemToSet(SetId, x)));

			var response = base.Send<GetRandomItemFromSetResponse>(
				new GetRandomItemFromSet { Id = SetId }, x => x.ResponseStatus);

			Assert.That(stringList.Contains(response.Item), Is.True);
		}

		[Test]
		public void Test_GetSetCount()
		{
			stringList.ForEach(x =>
				RedisExec(r => r.AddItemToSet(SetId, x)));

			var response = base.Send<GetSetCountResponse>(
				new GetSetCount { Id = SetId }, x => x.ResponseStatus);

			Assert.That(response.Count, Is.EqualTo(stringList.Count));
		}

		[Test]
		public void Test_GetUnionFromSets()
		{
			stringList.ForEach(x =>
				RedisExec(r => r.AddItemToSet(SetId, x)));
			stringList2.ForEach(x =>
				RedisExec(r => r.AddItemToSet(SetId2, x)));

			var response = base.Send<GetUnionFromSetsResponse>(
				new GetUnionFromSets { SetIds = { SetId, SetId2 } }, x => x.ResponseStatus);

			var unionList = new List<string>(stringList);
			stringList2.ForEach(x => { if (!unionList.Contains(x)) unionList.Add(x); });

			Assert.That(response.Items, Is.EquivalentTo(unionList));
		}

		[Test]
		public void Test_MoveBetweenSets()
		{
			stringList.ForEach(x =>
				RedisExec(r => r.AddItemToSet(SetId, x)));

			var lastItem = stringList.Last();
			var response = base.Send<MoveBetweenSetsResponse>(
				new MoveBetweenSets { FromSetId = SetId, ToSetId = SetId2, Item = lastItem }, x => x.ResponseStatus);

			stringList.Remove(lastItem);

			var setItems = RedisExec(r => r.GetAllItemsFromSet(SetId));
			var setItems2 = RedisExec(r => r.GetAllItemsFromSet(SetId2));

			Assert.That(setItems, Is.EquivalentTo(stringList));
			Assert.That(setItems2, Is.EquivalentTo(new List<string> { lastItem }));
		}

		[Test]
		public void Test_PopItemFromSet()
		{
			stringList.ForEach(x =>
				RedisExec(r => r.AddItemToSet(SetId, x)));

			var response = base.Send<PopItemFromSetResponse>(
				new PopItemFromSet { Id = SetId }, x => x.ResponseStatus);

			Assert.That(stringList.Contains(response.Item), Is.True);
		}

		[Test]
		public void Test_RemoveItemFromSet()
		{
			stringList.ForEach(x =>
				RedisExec(r => r.AddItemToSet(SetId, x)));

			var lastItem = stringList.Last();
			var response = base.Send<RemoveItemFromSetResponse>(
				new RemoveItemFromSet { Id = SetId, Item = lastItem }, x => x.ResponseStatus);

			stringList.Remove(lastItem);
			var setItems = RedisExec(r => r.GetAllItemsFromSet(SetId));

			Assert.That(setItems, Is.EquivalentTo(stringList));
		}

		[Test]
		public void Test_SetContainsItem()
		{
			stringList.ForEach(x =>
				RedisExec(r => r.AddItemToSet(SetId, x)));

			var lastItem = stringList.Last();

			var response = base.Send<SetContainsItemResponse>(
				new SetContainsItem { Id = SetId, Item = lastItem }, x => x.ResponseStatus);
			Assert.That(response.Result, Is.True);

			response = base.Send<SetContainsItemResponse>(
				new SetContainsItem { Id = SetId, Item = "notexists" }, x => x.ResponseStatus);
			Assert.That(response.Result, Is.False);
		}

		[Test]
		public void Test_StoreDifferencesFromSet()
		{
			stringList.ForEach(x =>
				RedisExec(r => r.AddItemToSet(SetId, x)));
			stringList2.ForEach(x =>
				RedisExec(r => r.AddItemToSet(SetId2, x)));
			stringList3.ForEach(x =>
				RedisExec(r => r.AddItemToSet(SetId3, x)));

			var response = base.Send<StoreDifferencesFromSetResponse>(
				new StoreDifferencesFromSet { Id = "storeset", 
					FromSetId = SetId, SetIds = { SetId2, SetId3 } }, x => x.ResponseStatus);

			var setItems = RedisExec(r => r.GetAllItemsFromSet("storeset"));

			Assert.That(setItems, Is.EquivalentTo(new List<string> { "two", "three" }));
		}

		[Test]
		public void Test_StoreIntersectFromSets()
		{
			stringList.ForEach(x =>
				RedisExec(r => r.AddItemToSet(SetId, x)));
			stringList2.ForEach(x =>
				RedisExec(r => r.AddItemToSet(SetId2, x)));

			var response = base.Send<StoreIntersectFromSetsResponse>(
				new StoreIntersectFromSets { Id = SetId3, SetIds = { SetId, SetId2 } }, x => x.ResponseStatus);

			var setItems = RedisExec(r => r.GetAllItemsFromSet(SetId3));

			Assert.That(setItems.ToList(), Is.EquivalentTo(new List<string> { "four" }));
		}

		[Test]
		public void Test_StoreUnionFromSets()
		{
			stringList.ForEach(x =>
				RedisExec(r => r.AddItemToSet(SetId, x)));
			stringList2.ForEach(x =>
				RedisExec(r => r.AddItemToSet(SetId2, x)));

			var response = base.Send<StoreUnionFromSetsResponse>(
				new StoreUnionFromSets { Id = SetId3, SetIds = { SetId, SetId2 } }, x => x.ResponseStatus);

			var unionList = new List<string>(stringList);
			stringList2.ForEach(x => { if (!unionList.Contains(x)) unionList.Add(x); });

			var setItems = RedisExec(r => r.GetAllItemsFromSet(SetId3));

			Assert.That(setItems, Is.EquivalentTo(unionList));
		}
	}
}