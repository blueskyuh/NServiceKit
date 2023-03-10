using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NUnit.Framework;
using NServiceKit.Common.Tests.Models;
using NServiceKit.Common.Utils;
using NServiceKit.DataAnnotations;
using System.Collections.Generic;
using NServiceKit.Text;

namespace NServiceKit.Common.Tests
{
    /// <summary>A reflection utility tests.</summary>
	[TestFixture]
	public class ReflectionUtilTests
	{
        /// <summary>Values that represent TestClassType.</summary>
		public enum TestClassType
		{
            /// <summary>An enum constant representing the one option.</summary>
			One = 1,

            /// <summary>An enum constant representing the two option.</summary>
			Two = 2,

            /// <summary>An enum constant representing the three option.</summary>
			Three = 3
		}

        /// <summary>A test class 2.</summary>
		public class TestClass2
		{
            /// <summary>Gets or sets the type.</summary>
            ///
            /// <value>The type.</value>
			public TestClassType Type { get; set; }
		}

        /// <summary>A test class.</summary>
		public class TestClass
		{
            /// <summary>Gets or sets the member 1.</summary>
            ///
            /// <value>The member 1.</value>
			[Required]
			public string Member1 { get; set; }

            /// <summary>Gets or sets the member 2.</summary>
            ///
            /// <value>The member 2.</value>
			public string Member2 { get; set; }

            /// <summary>Gets or sets the member 3.</summary>
            ///
            /// <value>The member 3.</value>
			[Required]
			public string Member3 { get; set; }

            /// <summary>Gets or sets the member 4.</summary>
            ///
            /// <value>The member 4.</value>
			[StringLength(1)]
			public string Member4 { get; set; }
		}

        /// <summary>A dto with string array.</summary>
        public class DtoWithStringArray
        {
            /// <summary>Gets or sets the data.</summary>
            ///
            /// <value>The data.</value>
            public string[] Data { get; set; }
        }

        /// <summary>A dto with enum array.</summary>
        public class DtoWithEnumArray
        {
            /// <summary>Gets or sets the data.</summary>
            ///
            /// <value>The data.</value>
            public TestClassType[] Data { get; set; }
        }

        /// <summary>A recursive dto.</summary>
        public class RecursiveDto
        {
            /// <summary>Gets or sets the name.</summary>
            ///
            /// <value>The name.</value>
            public string Name { get; set; }

            /// <summary>Gets or sets the child.</summary>
            ///
            /// <value>The child.</value>
            public RecursiveDto Child { get; set; }
        }

        /// <summary>A dto with recursive array.</summary>
        public class DtoWithRecursiveArray
        {
            /// <summary>Gets or sets the paths.</summary>
            ///
            /// <value>The paths.</value>
            public RecursiveDto[] Paths { get; set; }
        }

        /// <summary>A recursive array dto.</summary>
        public class RecursiveArrayDto
        {
            /// <summary>Gets or sets the name.</summary>
            ///
            /// <value>The name.</value>
            public string Name { get; set; }

            /// <summary>Gets or sets the nodes.</summary>
            ///
            /// <value>The nodes.</value>
            public RecursiveArrayDto[] Nodes { get; set; }
        }

        /// <summary>A mind twister.</summary>
        public class MindTwister
        {
            /// <summary>Gets or sets the name.</summary>
            ///
            /// <value>The name.</value>
            public string Name { get; set; }

            /// <summary>Gets or sets the arrays.</summary>
            ///
            /// <value>The arrays.</value>
            public RecursiveArrayDto[] Arrays { get; set; }

            /// <summary>Gets or sets the vortex.</summary>
            ///
            /// <value>The vortex.</value>
            public Vortex Vortex { get; set; }
        }

        /// <summary>A vortex.</summary>
        public class Vortex
        {
            /// <summary>Gets or sets the identifier.</summary>
            ///
            /// <value>The identifier.</value>
            public int Id { get; set; }

            /// <summary>Gets or sets the arrays.</summary>
            ///
            /// <value>The arrays.</value>
            public RecursiveArrayDto Arrays { get; set; }

            /// <summary>Gets or sets the twisters.</summary>
            ///
            /// <value>The twisters.</value>
            public MindTwister[] Twisters { get; set; }
        }

        /// <summary>Can populate recursive dto.</summary>
        [Test]
        public void Can_PopulateRecursiveDto()
        {
            var obj = (RecursiveDto)ReflectionUtils.PopulateObject(new RecursiveDto());
            Assert.IsNotNullOrEmpty(obj.Name);
            Assert.IsNotNull(obj.Child);
            Assert.IsNotNullOrEmpty(obj.Child.Name);
        }

        /// <summary>Can populate array of recursive dto.</summary>
        [Test]
        public void Can_PopulateArrayOfRecursiveDto()
        {
            var obj = (DtoWithRecursiveArray)ReflectionUtils.PopulateObject(new DtoWithRecursiveArray());
            Assert.IsNotNull(obj.Paths);
            Assert.Greater(obj.Paths.Length, 0);
            Assert.IsNotNull(obj.Paths[0]);
            Assert.IsNotNullOrEmpty(obj.Paths[0].Name);
            Assert.IsNotNull(obj.Paths[0].Child);
            Assert.IsNotNullOrEmpty(obj.Paths[0].Child.Name);
        }

        /// <summary>Can populate recursive array dto.</summary>
        [Test]
        public void Can_PopulateRecursiveArrayDto()
        {
            var obj = (RecursiveArrayDto)ReflectionUtils.PopulateObject(new RecursiveArrayDto());
            Assert.IsNotNullOrEmpty(obj.Name);
            Assert.IsNotNull(obj.Nodes[0]);
            Assert.IsNotNullOrEmpty(obj.Nodes[0].Name);
            Assert.IsNotNull(obj.Nodes[0].Nodes);
            Assert.IsNotNullOrEmpty(obj.Nodes[0].Nodes[0].Name);
        }

        /// <summary>Can populate the vortex.</summary>
        [Test]
        public void Can_PopulateTheVortex()
        {
            var obj = (MindTwister)ReflectionUtils.PopulateObject(new MindTwister());
            Console.WriteLine("Mindtwister = " + NServiceKit.Text.XmlSerializer.SerializeToString(obj)); // TypeSerializer and JsonSerializer blow up on this structure with a Null Reference Exception!
            Assert.IsNotNull(obj);
            Assert.IsNotNull(obj.Name);
            Assert.IsNotNull(obj.Arrays);
            Assert.IsNotNull(obj.Vortex);
        }

        /// <summary>Can populate object with string array.</summary>
        [Test]
        public void Can_PopulateObjectWithStringArray()
        {
            var obj = (DtoWithStringArray)ReflectionUtils.PopulateObject(new DtoWithStringArray());
            Assert.IsNotNull(obj.Data);
            Assert.Greater(obj.Data.Length, 0);
            Assert.IsNotNull(obj.Data[0]);
        }

        /// <summary>Can populate object with non zero enum array.</summary>
        [Test]
        public void Can_PopulateObjectWithNonZeroEnumArray()
        {
            var obj = (DtoWithEnumArray)ReflectionUtils.PopulateObject(new DtoWithEnumArray());
            Assert.IsNotNull(obj.Data);
            Assert.Greater(obj.Data.Length, 0);
            Assert.That(Enum.IsDefined(typeof(TestClassType), obj.Data[0]), "Values in created array should be valid for the enum");
        }

        /// <summary>Populate object uses defined enum.</summary>
		[Test]
		public void PopulateObject_UsesDefinedEnum()
		{
			var requestObj = (TestClass2)ReflectionUtils.PopulateObject(Activator.CreateInstance(typeof(TestClass2)));
			Assert.True(Enum.IsDefined(typeof(TestClassType), requestObj.Type));
		}

        /// <summary>Populate object uses defined enum on nested types.</summary>
		[Test]
		public void PopulateObject_UsesDefinedEnum_OnNestedTypes()
		{
			var requestObj = (Dictionary<string, TestClass2>)ReflectionUtils.CreateDefaultValue(typeof(Dictionary<string,TestClass2>), new Dictionary<Type,int>());
			Assert.True(Enum.IsDefined(typeof(TestClassType), requestObj.First().Value.Type));
		}

        /// <summary>Tests get.</summary>
		[Test]
		public void GetTest()
		{
			var propertyAttributes = ReflectionUtils.GetPropertyAttributes<RequiredAttribute>(typeof(TestClass));
			var propertyNames = propertyAttributes.ToList().ConvertAll(x => x.Key.Name);
			Assert.That(propertyNames, Is.EquivalentTo(new[] { "Member1", "Member3" }));
		}

        /// <summary>Populate same objects.</summary>
		[Test]
		public void Populate_Same_Objects()
		{
			var toObj = ModelWithFieldsOfDifferentTypes.Create(1);
			var fromObj = ModelWithFieldsOfDifferentTypes.Create(2);

			var obj3 = ReflectionUtils.PopulateObject(toObj, fromObj);

			Assert.IsTrue(obj3 == toObj);
			Assert.That(obj3.Bool, Is.EqualTo(fromObj.Bool));
			Assert.That(obj3.DateTime, Is.EqualTo(fromObj.DateTime));
			Assert.That(obj3.Double, Is.EqualTo(fromObj.Double));
			Assert.That(obj3.Guid, Is.EqualTo(fromObj.Guid));
			Assert.That(obj3.Id, Is.EqualTo(fromObj.Id));
			Assert.That(obj3.LongId, Is.EqualTo(fromObj.LongId));
			Assert.That(obj3.Name, Is.EqualTo(fromObj.Name));
		}

        /// <summary>Populate different objects with different property types.</summary>
		[Test]
		public void Populate_Different_Objects_with_different_property_types()
		{
			var toObj = ModelWithFieldsOfDifferentTypes.Create(1);
			var fromObj = ModelWithOnlyStringFields.Create("2");

			var obj3 = ReflectionUtils.PopulateObject(toObj, fromObj);

			Assert.IsTrue(obj3 == toObj);
			Assert.That(obj3.Id, Is.EqualTo(2));
			Assert.That(obj3.Name, Is.EqualTo(fromObj.Name));
		}

        /// <summary>Populate from properties with attribute.</summary>
		[Test]
		public void Populate_From_Properties_With_Attribute()
		{
			var originalToObj = ModelWithOnlyStringFields.Create("id-1");
			var toObj = ModelWithOnlyStringFields.Create("id-1");
			var fromObj = ModelWithOnlyStringFields.Create("id-2");

			ReflectionUtils.PopulateFromPropertiesWithAttribute(toObj, fromObj,
				typeof(IndexAttribute));

			Assert.That(toObj.Id, Is.EqualTo(originalToObj.Id));
			Assert.That(toObj.AlbumId, Is.EqualTo(originalToObj.AlbumId));

			//Properties with IndexAttribute
			Assert.That(toObj.Name, Is.EqualTo(fromObj.Name));
			Assert.That(toObj.AlbumName, Is.EqualTo(fromObj.AlbumName));
		}

        /// <summary>Populate from properties with non default values.</summary>
		[Test]
		public void Populate_From_Properties_With_Non_Default_Values()
		{
			var toObj = ModelWithFieldsOfDifferentTypes.Create(1);
			var fromObj = ModelWithFieldsOfDifferentTypes.Create(2);

			var originalToObj = ModelWithFieldsOfDifferentTypes.Create(1);
			var originalGuid = toObj.Guid;

			fromObj.Name = null;
			fromObj.Double = default(double);
			fromObj.Guid = default(Guid);

			ReflectionUtils.PopulateWithNonDefaultValues(toObj, fromObj);

			Assert.That(toObj.Name, Is.EqualTo(originalToObj.Name));
			Assert.That(toObj.Double, Is.EqualTo(originalToObj.Double));
			Assert.That(toObj.Guid, Is.EqualTo(originalGuid));

			Assert.That(toObj.Id, Is.EqualTo(fromObj.Id));
			Assert.That(toObj.LongId, Is.EqualTo(fromObj.LongId));
			Assert.That(toObj.Bool, Is.EqualTo(fromObj.Bool));
			Assert.That(toObj.DateTime, Is.EqualTo(fromObj.DateTime));
		}

        /// <summary>Populate from nullable properties with non default values.</summary>
        [Test]
        public void Populate_From_Nullable_Properties_With_Non_Default_Values()
        {
            var toObj = ModelWithFieldsOfDifferentTypes.Create(1);
            var fromObj = ModelWithFieldsOfDifferentTypesAsNullables.Create(2);

            var originalToObj = ModelWithFieldsOfDifferentTypes.Create(1);

            fromObj.Name = null;
            fromObj.Double = default(double);
            fromObj.Guid = default(Guid);
            fromObj.Bool = default(bool);

            ReflectionUtils.PopulateWithNonDefaultValues(toObj, fromObj);

            Assert.That(toObj.Name, Is.EqualTo(originalToObj.Name));

            Assert.That(toObj.Double, Is.EqualTo(fromObj.Double));
            Assert.That(toObj.Guid, Is.EqualTo(fromObj.Guid));
            Assert.That(toObj.Bool, Is.EqualTo(fromObj.Bool));
            Assert.That(toObj.Id, Is.EqualTo(fromObj.Id));
            Assert.That(toObj.LongId, Is.EqualTo(fromObj.LongId));
            Assert.That(toObj.DateTime, Is.EqualTo(fromObj.DateTime));
        }

        /// <summary>Translate between models of differrent types and nullables.</summary>
		[Test]
		public void Translate_Between_Models_of_differrent_types_and_nullables()
		{
			var fromObj = ModelWithFieldsOfDifferentTypes.CreateConstant(1);

			var toObj = fromObj.TranslateTo<ModelWithFieldsOfDifferentTypesAsNullables>();

			Console.WriteLine(toObj.Dump());

			ModelWithFieldsOfDifferentTypesAsNullables.AssertIsEqual(fromObj, toObj);
		}

        /// <summary>Translate between models of nullables and differrent types.</summary>
		[Test]
		public void Translate_Between_Models_of_nullables_and_differrent_types()
		{
			var fromObj = ModelWithFieldsOfDifferentTypesAsNullables.CreateConstant(1);

			var toObj = fromObj.TranslateTo<ModelWithFieldsOfDifferentTypes>();

			Console.WriteLine(toObj.Dump());

			ModelWithFieldsOfDifferentTypesAsNullables.AssertIsEqual(toObj, fromObj);
		}
	}
}
