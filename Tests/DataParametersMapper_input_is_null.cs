//using System.Linq;
//using NUnit.Framework;

//namespace TinyORM.Tests
//{
//   [TestFixture]
//   public class DataParametersMapper_input_is_null
//   {
//      [Test]
//      public void should_return_empty_collection()
//      {
//         Assert.That(new DataParametersMapper().Map(null), Is.Empty);
//      }
//   }

//   [TestFixture]
//   public class DataParametersMapper_mapping_an_object_without_TypeMap
//   {
//      [Test]
//      public void should_return_matching_parameters()
//      {
//         var mapper = new DataParametersMapper();
//         var parameters = mapper.Map(new { Number = 15 });

//         Assert.That(parameters.Count(), Is.EqualTo(1));
//         var parameter = parameters.Single();
//         Assert.That(parameter.ParameterName, Is.EqualTo("@Number"));
//         Assert.That(parameter.sql, Is.EqualTo("@Number"));
//         Assert.That(parameter.ParameterName, Is.EqualTo("@Number"));
//      }
//   }
//}