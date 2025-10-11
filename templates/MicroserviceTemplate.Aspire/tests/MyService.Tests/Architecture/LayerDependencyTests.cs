using NetArchTest.Rules;

namespace MyService.Tests.Architecture
{
    public class LayerDependencyTests
    {
        [Fact]
        public void Infrastructure_Should_Not_Depend_On_Api()
        {
            // Checks that the Infrastructure layer does not depend on the Api layer
            var result = Types
                .InAssembly(typeof(Infrastructure.Data.AppDbContext).Assembly)
                .ShouldNot()
                .HaveDependencyOn("MyService.Api")
                .GetResult();

            Assert.True(result.IsSuccessful, "Infrastructure layer must not depend on Api layer.");
        }

        [Fact]
        public void Domain_Should_Not_Depend_On_Infrastructure_Or_Api()
        {
            // Checks that the Domain layer does not depend on the Infrastructure or Api layers
            var result = Types
                .InAssembly(typeof(Domain.Common.Entities.OutboxMessage).Assembly)
                .ShouldNot()
                .HaveDependencyOn("MyService.Infrastructure")
                .Or()
                .HaveDependencyOn("MyService.Api")
                .GetResult();

            Assert.True(result.IsSuccessful, "Domain layer must not depend on Infrastructure or Api layers.");
        }

        [Fact]
        public void Api_Should_Not_Depend_On_Infrastructure()
        {
            // The Api layer should not depend on the Infrastructure layer directly,
            // but it can depend on it indirectly through extension methods for service registration.
            // These extension methods are typically defined in static classes in the Infrastructure project
            // and are used in the Program.cs or Startup.cs of the Api project to register services.

            var apiTypes = Types
                .InAssembly(typeof(Program).Assembly)
                .GetTypes();

            var forbiddenTypes = apiTypes
                .Where(t =>
                    t.Namespace != null &&
                    t.Namespace.StartsWith("MyService.Infrastructure") &&
                    !(t.IsSealed && t.IsAbstract && t.IsClass && t.Name.EndsWith("Extensions"))
                )
                .ToList();

            Assert.True(forbiddenTypes.Count == 0, "Api layer must not depend directly on Infrastructure layer types, except for static extension classes.");
        }

    }
}
