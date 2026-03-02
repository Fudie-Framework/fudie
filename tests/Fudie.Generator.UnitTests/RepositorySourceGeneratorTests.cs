namespace Fudie.Generator.UnitTests;

public class RepositorySourceGeneratorTests
{
    #region Helper Methods

    private static GeneratorDriverRunResult RunGenerator(string sourceCode)
    {
        // Crear compilación con el código fuente
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
        };

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Crear y ejecutar el generador
        var generator = new RepositorySourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var diagnostics);

        return driver.GetRunResult();
    }

    private static string CreateTestCode(
        string interfaceCode,
        string? entityCode = null,
        bool includeFudieInfrastructure = true)
    {
        var code = @"
using System;
using System.Collections.Generic;
using Fudie.Attributes;
using Fudie.Infrastructure;
using Fudie.Domain;

namespace TestNamespace
{
    // Entity
    public class Entity
    {
        public Guid Id { get; protected set; }
    }
";

        if (entityCode != null)
        {
            code += entityCode + "\n";
        }

        code += interfaceCode;
        code += "\n}";

        if (includeFudieInfrastructure)
        {
            code += @"
namespace Fudie.Infrastructure
{
    public interface IGet<T, ID> { }
    public interface IAdd<T> { }
    public interface IUpdate<T, ID> : IGet<T, ID> { }
    public interface IRemove<T, ID> : IGet<T, ID> { }
    public interface IEntityLookup { }
    public interface IChangeTracker { }

    [System.AttributeUsage(System.AttributeTargets.Interface, AllowMultiple = true)]
    public class IncludeAttribute : System.Attribute
    {
        public IncludeAttribute(params string[] paths) { }
    }

    [System.AttributeUsage(System.AttributeTargets.Interface | System.AttributeTargets.Method)]
    public class AsNoTrackingAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Interface | System.AttributeTargets.Method)]
    public class TrackingAttribute : System.Attribute
    {
        public TrackingAttribute(bool enabled) { }
    }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class AsSplitQueryAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class IgnoreQueryFiltersAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class GenerateRepositoryAttribute<TEntity> : System.Attribute where TEntity : class { }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class GenerateRepositoryAttribute<TEntity, TId> : System.Attribute where TEntity : class { }
}

namespace Fudie.DependencyInjection
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class InjectableAttribute : System.Attribute
    {
        public InjectableAttribute(Microsoft.Extensions.DependencyInjection.ServiceLifetime lifetime) { }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public enum ServiceLifetime { Scoped }
}
";
        }

        return code;
    }

    #endregion

    #region Basic Generation Tests

    [Fact]
    public void Generator_WithSimpleIGetInterface_ShouldGenerateRepository()
    {
        // Arrange - SIN atributos, solo herencia
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity
    {
        public string Name { get; set; }
    }

    public interface ICustomerRepository : IGet<Customer, Guid>
    {
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();

        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("public class CustomerRepository : ICustomerRepository");
        generatedCode.Should().Contain("[Injectable(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(ICustomerRepository))]");
        // Base interfaces are marker interfaces and should NOT have [Injectable]
        generatedCode.Should().NotContain("[Injectable(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(IGet<Customer, System.Guid>))]");
        generatedCode.Should().Contain("public async Task<Customer> Get(System.Guid id)");
        generatedCode.Should().NotContain("AsNoTracking()"); // Sin atributo, no hay AsNoTracking
    }

    [Fact]
    public void Generator_WithIUpdateInterface_ShouldGenerateRepositoryWithTracking()
    {
        // Arrange - SIN atributos
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity
    {
        public string Name { get; set; }
    }

    public interface ICustomerRepository : IUpdate<Customer, Guid>
    {
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();

        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("public class CustomerRepository : ICustomerRepository");
        // Base interfaces are marker interfaces and should NOT have [Injectable]
        generatedCode.Should().NotContain("[Injectable(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(IUpdate<Customer, System.Guid>))]");
        generatedCode.Should().NotContain("[Injectable(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(IGet<Customer, System.Guid>))]");
        generatedCode.Should().Contain("_entityLookup.Set<Customer>()");
        generatedCode.Should().NotContain("AsNoTracking()");
    }

    [Fact]
    public void Generator_WithIAddInterface_ShouldGenerateAddMethod()
    {
        // Arrange - SIN atributos
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity
    {
        public string Name { get; set; }
    }

    public interface ICustomerRepository : IAdd<Customer>
    {
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();

        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("public class CustomerRepository : ICustomerRepository");
        // Base interfaces are marker interfaces and should NOT have [Injectable]
        generatedCode.Should().NotContain("[Injectable(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(IAdd<Customer>))]");
        generatedCode.Should().Contain("public void Add(Customer entity)");
        generatedCode.Should().Contain("_changeTracker.Entry(entity).State = EntityState.Added");
    }

    [Fact]
    public void Generator_WithIRemoveInterface_ShouldGenerateRemoveMethod()
    {
        // Arrange - SIN atributos
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity
    {
        public string Name { get; set; }
    }

    public interface ICustomerRepository : IRemove<Customer, Guid>
    {
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();

        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("public class CustomerRepository : ICustomerRepository");
        // Base interfaces are marker interfaces and should NOT have [Injectable]
        generatedCode.Should().NotContain("[Injectable(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(IRemove<Customer, System.Guid>))]");
        generatedCode.Should().NotContain("[Injectable(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(IGet<Customer, System.Guid>))]");
        // IRemove necesita tracking para eliminar, debe usar Set<T>() en lugar de Query<T>()
        generatedCode.Should().Contain("_entityLookup.Set<Customer>()");
        generatedCode.Should().Contain("public void Remove(Customer entity)");
        generatedCode.Should().Contain("_changeTracker.Entry(entity).State = EntityState.Deleted");
    }

    [Fact]
    public void Generator_WithMultipleInterfaces_ShouldImplementAll()
    {
        // Arrange - SIN atributos
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity
    {
        public string Name { get; set; }
    }

    public interface ICustomerRepository :
        IGet<Customer, Guid>,
        IAdd<Customer>,
        IRemove<Customer, Guid>
    {
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();

        var generatedCode = result.GeneratedTrees[0].ToString();
        // Should implement container interface
        generatedCode.Should().Contain("public class CustomerRepository : ICustomerRepository");
        // Should have [Injectable] only for container interface
        generatedCode.Should().Contain("[Injectable(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(ICustomerRepository))]");
        // Base interfaces are marker interfaces and should NOT have [Injectable]
        generatedCode.Should().NotContain("[Injectable(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(IGet<Customer, System.Guid>))]");
        generatedCode.Should().NotContain("[Injectable(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(IAdd<Customer>))]");
        generatedCode.Should().NotContain("[Injectable(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(IRemove<Customer, System.Guid>))]");
        // Should have all methods
        generatedCode.Should().Contain("public async Task<Customer> Get(System.Guid id)");
        generatedCode.Should().Contain("public void Add(Customer entity)");
        generatedCode.Should().Contain("public void Remove(Customer entity)");
    }

    #endregion

    #region Attribute Tests

    [Fact]
    public void Generator_WithAsNoTrackingAttribute_ShouldGenerateAsNoTracking()
    {
        // Arrange - CON atributo [AsNoTracking]
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity
    {
        public string Name { get; set; }
    }

    [AsNoTracking]
    public interface ICustomerRepository : IGet<Customer, Guid>
    {
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("query = query.AsNoTracking()");
    }

    [Fact]
    public void Generator_WithIncludeAttribute_ShouldGenerateIncludes()
    {
        // Arrange - CON atributo [Include]
        var source = CreateTestCode(
            interfaceCode: @"
    public class Order : Entity
    {
        public Guid CustomerId { get; set; }
    }

    public class Customer : Entity
    {
        public string Name { get; set; }
        public List<Order> Orders { get; set; }
    }

    [Include(""Orders"")]
    public interface ICustomerRepository : IGet<Customer, Guid>
    {
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("query = query.Include(c => c.Orders)");
    }

    [Fact]
    public void Generator_WithAsSplitQueryAttribute_ShouldGenerateAsSplitQuery()
    {
        // Arrange - CON atributo [AsSplitQuery]
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity
    {
        public string Name { get; set; }
    }

    [AsSplitQuery]
    public interface ICustomerRepository : IGet<Customer, Guid>
    {
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("query = query.AsSplitQuery()");
    }

    [Fact]
    public void Generator_WithIgnoreQueryFiltersAttribute_ShouldGenerateIgnoreQueryFilters()
    {
        // Arrange - CON atributo [IgnoreQueryFilters]
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity
    {
        public string Name { get; set; }
    }

    [IgnoreQueryFilters]
    public interface ICustomerRepository : IGet<Customer, Guid>
    {
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("query = query.IgnoreQueryFilters()");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void Generator_WithInvalidIncludePath_ShouldReportError()
    {
        // Arrange
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity
    {
        public string Name { get; set; }
    }

    [Include(""NonExistentProperty"")]
    public interface ICustomerRepository : IGet<Customer, Guid>
    {
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().Contain(d => d.Id == "FUDIE004");
        var diagnostic = result.Diagnostics.First(d => d.Id == "FUDIE004");
        diagnostic.GetMessage().Should().Contain("Property 'NonExistentProperty' does not exist on type 'Customer'");
    }

    [Fact]
    public void Generator_WithNonExistentEntityType_ShouldReportError()
    {
        // Arrange - Interfaz referencia entidad que no existe
        var source = CreateTestCode(
            interfaceCode: @"
    public interface ICustomerRepository : IGet<NonExistentEntity, Guid>
    {
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().Contain(d => d.Id == "FUDIE003");
        var diagnostic = result.Diagnostics.First(d => d.Id == "FUDIE003");
        diagnostic.GetMessage().Should().Contain("Could not find entity type");
    }

    #endregion

    #region Naming Tests

    [Fact]
    public void Generator_WithIPrefix_ShouldRemoveIPrefixFromClassName()
    {
        // Arrange - SIN atributos
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity { }

    public interface ICustomerRepository : IGet<Customer, Guid> { }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("public class CustomerRepository");
        generatedCode.Should().NotContain("public class ICustomerRepository");
    }

    [Fact]
    public void Generator_WithoutIPrefix_ShouldAppendImpl()
    {
        // Arrange - SIN atributos
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity { }

    public interface CustomerRepository : IGet<Customer, Guid> { }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("public class CustomerRepositoryImpl");
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public void Generator_WithComplexScenario_ShouldGenerateCorrectCode()
    {
        // Arrange - CON atributos (escenario complejo)
        var source = CreateTestCode(
            interfaceCode: @"
    public class OrderItem : Entity
    {
        public Guid OrderId { get; set; }
        public string ProductName { get; set; }
    }

    public class Order : Entity
    {
        public Guid CustomerId { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }

    public class Customer : Entity
    {
        public string Name { get; set; }
        public List<Order> Orders { get; set; }
    }

    [Include(""Orders"", ""Orders.OrderItems"")]
    [AsSplitQuery]
    [AsNoTracking]
    public interface ICustomerRepository : IGet<Customer, Guid>, IAdd<Customer>
    {
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();

        // Verificar container interface
        generatedCode.Should().Contain("public class CustomerRepository : ICustomerRepository");

        // Verificar Injectable attributes - only container interface should have [Injectable]
        generatedCode.Should().Contain("[Injectable(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(ICustomerRepository))]");
        // Base interfaces are marker interfaces and should NOT have [Injectable]
        generatedCode.Should().NotContain("[Injectable(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(IGet<Customer, System.Guid>))]");
        generatedCode.Should().NotContain("[Injectable(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(IAdd<Customer>))]");

        // Verificar includes
        generatedCode.Should().Contain("query = query.Include(c => c.Orders);");
        generatedCode.Should().Contain("query = query.Include(c => c.Orders)");
        generatedCode.Should().Contain(".ThenInclude(o => o.OrderItems);");

        // Verificar modificadores
        generatedCode.Should().Contain("query = query.AsSplitQuery();");
        generatedCode.Should().Contain("query = query.AsNoTracking();");

        // Verificar métodos
        generatedCode.Should().Contain("public async Task<Customer> Get(System.Guid id)");
        generatedCode.Should().Contain("public void Add(Customer entity)");
    }

    #endregion

    #region Constructor and Dependencies Tests

    [Fact]
    public void Generator_WithIGet_ShouldInjectIEntityLookup()
    {
        // Arrange - SIN atributos
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity { }

    public interface ICustomerRepository : IGet<Customer, Guid> { }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("private readonly IEntityLookup _entityLookup;");
        generatedCode.Should().Contain("public CustomerRepository(IEntityLookup entityLookup)");
        generatedCode.Should().Contain("_entityLookup = entityLookup;");
    }

    [Fact]
    public void Generator_WithIAdd_ShouldInjectIChangeTracker()
    {
        // Arrange - SIN atributos
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity { }

    public interface ICustomerRepository : IAdd<Customer> { }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("private readonly IChangeTracker _changeTracker;");
        generatedCode.Should().Contain("public CustomerRepository(IChangeTracker changeTracker)");
        generatedCode.Should().Contain("_changeTracker = changeTracker;");
    }

    [Fact]
    public void Generator_WithMultipleInterfaces_ShouldInjectBothDependencies()
    {
        // Arrange - SIN atributos
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity { }

    public interface ICustomerRepository : IGet<Customer, Guid>, IAdd<Customer> { }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("private readonly IEntityLookup _entityLookup;");
        generatedCode.Should().Contain("private readonly IChangeTracker _changeTracker;");
        generatedCode.Should().Contain("IEntityLookup entityLookup, IChangeTracker changeTracker");
    }

    #endregion

    #region Injectable Attribute Tests

    [Fact]
    public void Generator_ShouldAddInjectableAttributeOnlyForContainerInterface()
    {
        // Arrange - SIN atributos
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity { }

    public interface ICustomerRepository : IGet<Customer, Guid> { }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        // Should have [Injectable] only for container interface
        generatedCode.Should().Contain("[Injectable(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(ICustomerRepository))]");
        // Base interfaces are marker interfaces and should NOT have [Injectable]
        generatedCode.Should().NotContain("[Injectable(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(IGet<Customer, System.Guid>))]");
    }

    #endregion

    #region Nested Interface Tests

    [Fact]
    public void Generator_WithNestedInterface_ShouldImplementContainerInterface()
    {
        // Arrange - Interface anidada dentro de una clase (vertical slice pattern)
        var source = CreateTestCode(
            interfaceCode: @"
    public class Ingredient : Entity { }

    public class CreateIngredient
    {
        public interface IRepository : IAdd<Ingredient> { }
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        // Should implement container interface with full path
        generatedCode.Should().Contain("public class CreateIngredient_Repository : CreateIngredient.IRepository");
        // Should have [Injectable] only for container interface
        generatedCode.Should().Contain("[Injectable(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(CreateIngredient.IRepository))]");
        // Base interfaces are marker interfaces and should NOT have [Injectable]
        generatedCode.Should().NotContain("[Injectable(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(IAdd<Ingredient>))]");
    }

    [Fact]
    public void Generator_WithMultipleNestedInterfaces_ShouldGenerateUniqueClassNames()
    {
        // Arrange - Múltiples interfaces anidadas con el mismo nombre en diferentes clases
        var source = CreateTestCode(
            interfaceCode: @"
    public class Ingredient : Entity { }

    public class CreateIngredient
    {
        public interface IRepository : IAdd<Ingredient> { }
    }

    public class DeleteIngredient
    {
        public interface IRepository : IRemove<Ingredient, Guid> { }
    }");

        // Act
        var result = RunGenerator(source);

        // Assert - Debe generar dos archivos con nombres únicos
        result.GeneratedTrees.Should().HaveCount(2);

        var generatedCodes = result.GeneratedTrees.Select(t => t.ToString()).ToList();
        generatedCodes.Should().Contain(code => code.Contains("public class CreateIngredient_Repository : CreateIngredient.IRepository"));
        generatedCodes.Should().Contain(code => code.Contains("public class DeleteIngredient_Repository : DeleteIngredient.IRepository"));
    }

    [Fact]
    public void Generator_WithNestedInterface_ShouldUseFullTypePathForInjectable()
    {
        // Arrange - Interface anidada dentro de una clase (vertical slice pattern)
        var source = CreateTestCode(
            interfaceCode: @"
    public class Ingredient : Entity { }

    public class CreateIngredient
    {
        public interface IRepository : IAdd<Ingredient>, IGet<Ingredient, Guid> { }
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();

        // Container interface uses full path for typeof
        generatedCode.Should().Contain("[Injectable(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(CreateIngredient.IRepository))]");
        // Base interfaces are marker interfaces and should NOT have [Injectable]
        generatedCode.Should().NotContain("[Injectable(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(IAdd<Ingredient>))]");
        generatedCode.Should().NotContain("[Injectable(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(IGet<Ingredient, System.Guid>))]");
    }

    #endregion

    #region GenerateRepository Attribute Tests

    [Fact]
    public void Generator_WithGenerateRepositoryAttribute_ShouldGenerateRepositoryWithoutBaseInterfaces()
    {
        // Arrange - Solo [GenerateRepository], sin herencia de IGet/IAdd/IUpdate/IRemove
        var source = CreateTestCode(
            interfaceCode: @"
    public class Tenant : Entity
    {
        public new Guid Id { get; set; }
        public string Name { get; set; }
        public Guid RestaurantId { get; set; }
    }

    [GenerateRepository<Tenant>]
    public interface ITenantRepository
    {
        Task<Tenant?> FindFirstByIdAndRestaurantId(Guid id, Guid restaurantId);
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();

        var generatedCode = result.GeneratedTrees[0].ToString();
        // Should generate class implementing the interface
        generatedCode.Should().Contain("public class TenantRepository : ITenantRepository");
        // Should have Injectable for container interface
        generatedCode.Should().Contain("[Injectable(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(ITenantRepository))]");
        // Should NOT have Get method (no IGet inheritance)
        generatedCode.Should().NotContain("public async Task<Tenant> Get(");
        // Should have the query method
        generatedCode.Should().Contain("FindFirstByIdAndRestaurantId");
    }

    [Fact]
    public void Generator_WithGenerateRepositoryAttributeAndCustomIdType_ShouldUseCustomIdType()
    {
        // Arrange - [GenerateRepository<T, int>] con tipo de ID personalizado
        var source = CreateTestCode(
            interfaceCode: @"
    public class Product : Entity
    {
        public string Name { get; set; }
        public int ProductCode { get; set; }
    }

    [GenerateRepository<Product, int>]
    public interface IProductRepository
    {
        Task<Product?> FindFirstByProductCode(int code);
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();

        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("public class ProductRepository : IProductRepository");
    }

    [Fact]
    public void Generator_WithGenerateRepositoryAndTracking_ShouldApplyTracking()
    {
        // Arrange - [GenerateRepository] con [Tracking]
        var source = CreateTestCode(
            interfaceCode: @"
    public class Tenant : Entity
    {
        public new Guid Id { get; set; }
        public string Name { get; set; }
        public Guid RestaurantId { get; set; }
    }

    [GenerateRepository<Tenant>]
    [Tracking(true)]
    public interface ITenantRepository
    {
        Task<Tenant?> FindFirstByIdAndRestaurantId(Guid id, Guid restaurantId);
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();

        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("public class TenantRepository : ITenantRepository");
        // With tracking enabled, should NOT have AsNoTracking
        generatedCode.Should().NotContain("AsNoTracking()");
    }

    [Fact]
    public void Generator_WithGenerateRepositoryAndAsNoTracking_ShouldGenerateRepository()
    {
        // Arrange - [GenerateRepository] con [AsNoTracking]
        // Nota: Los modificadores como AsNoTracking se aplican al método Get() de IGet,
        // no a los query methods custom que tienen control total sobre su query.
        var source = CreateTestCode(
            interfaceCode: @"
    public class Tenant : Entity
    {
        public new Guid Id { get; set; }
        public string Name { get; set; }
        public Guid RestaurantId { get; set; }
    }

    [GenerateRepository<Tenant>]
    [AsNoTracking]
    public interface ITenantRepository
    {
        Task<Tenant?> FindFirstByIdAndRestaurantId(Guid id, Guid restaurantId);
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();

        var generatedCode = result.GeneratedTrees[0].ToString();
        // El repositorio se genera correctamente
        generatedCode.Should().Contain("public class TenantRepository : ITenantRepository");
        generatedCode.Should().Contain("FindFirstByIdAndRestaurantId");
    }

    [Fact]
    public void Generator_WithGenerateRepositoryMultipleQueryMethods_ShouldGenerateAllMethods()
    {
        // Arrange - Múltiples query methods
        var source = CreateTestCode(
            interfaceCode: @"
    public class Tenant : Entity
    {
        public new Guid Id { get; set; }
        public string Name { get; set; }
        public Guid RestaurantId { get; set; }
        public bool IsActive { get; set; }
    }

    [GenerateRepository<Tenant>]
    public interface ITenantRepository
    {
        Task<Tenant?> FindFirstByIdAndRestaurantId(Guid id, Guid restaurantId);
        Task<List<Tenant>> FindByRestaurantId(Guid restaurantId);
        Task<int> CountByRestaurantIdAndIsActiveTrue(Guid restaurantId);
        Task<bool> ExistsByRestaurantId(Guid restaurantId);
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();

        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("FindFirstByIdAndRestaurantId");
        generatedCode.Should().Contain("FindByRestaurantId");
        generatedCode.Should().Contain("CountByRestaurantIdAndIsActiveTrue");
        generatedCode.Should().Contain("ExistsByRestaurantId");
    }

    [Fact]
    public void Generator_WithoutGenerateRepositoryOrBaseInterfaces_ShouldNotGenerate()
    {
        // Arrange - Interfaz sin [GenerateRepository] ni herencia de interfaces base
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity
    {
        public string Name { get; set; }
    }

    public interface ICustomerService
    {
        void Handle();
    }");

        // Act
        var result = RunGenerator(source);

        // Assert - No debe generar nada para ICustomerService
        result.GeneratedTrees.Should().BeEmpty();
    }

    [Fact]
    public void Generator_WithBothGenerateRepositoryAndIGet_ShouldGenerateBoth()
    {
        // Arrange - Tiene ambos: [GenerateRepository] Y herencia de IGet
        // En este caso, IGet toma precedencia para la entidad
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    [GenerateRepository<Customer>]
    public interface ICustomerRepository : IGet<Customer, Guid>
    {
        Task<Customer?> FindFirstByEmail(string email);
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();

        var generatedCode = result.GeneratedTrees[0].ToString();
        // Should have Get from IGet
        generatedCode.Should().Contain("public async Task<Customer> Get(System.Guid id)");
        // Should also have the query method
        generatedCode.Should().Contain("FindFirstByEmail");
    }

    #endregion

    #region Method-Level Tracking Attribute Tests

    [Fact]
    public void Generator_WithMethodLevelAsNoTracking_ShouldGenerateNoTrackingForMethod()
    {
        // Arrange - Method-level [AsNoTracking] on a query method
        var source = CreateTestCode(
            interfaceCode: @"
    public class Tenant : Entity
    {
        public new Guid Id { get; set; }
        public string Name { get; set; }
        public Guid RestaurantId { get; set; }
    }

    [GenerateRepository<Tenant>]
    [Tracking(true)]
    public interface ITenantRepository
    {
        [AsNoTracking]
        Task<List<Tenant>> FindByRestaurantId(Guid restaurantId);
        Task<Tenant?> FindFirstByIdAndRestaurantId(Guid id, Guid restaurantId);
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();

        // FindByRestaurantId should use _query.Query (no tracking due to [AsNoTracking])
        generatedCode.Should().Contain("_query.Query<Tenant>()");
        // FindFirstByIdAndRestaurantId should use _entityLookup.Set (tracking from interface default)
        generatedCode.Should().Contain("_entityLookup.Set<Tenant>()");
    }

    [Fact]
    public void Generator_WithMethodLevelTracking_ShouldOverrideInterfaceDefault()
    {
        // Arrange - Method-level [Tracking(true)] overrides interface [AsNoTracking]
        var source = CreateTestCode(
            interfaceCode: @"
    public class Tenant : Entity
    {
        public new Guid Id { get; set; }
        public string Name { get; set; }
        public Guid RestaurantId { get; set; }
    }

    [GenerateRepository<Tenant>]
    [AsNoTracking]
    public interface ITenantRepository
    {
        [Tracking(true)]
        Task<Tenant?> FindFirstByIdAndRestaurantId(Guid id, Guid restaurantId);
        Task<List<Tenant>> FindByRestaurantId(Guid restaurantId);
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();

        // FindFirstByIdAndRestaurantId should use _entityLookup.Set (tracking due to [Tracking(true)])
        generatedCode.Should().Contain("_entityLookup.Set<Tenant>()");
        // FindByRestaurantId should use _query.Query (no tracking from interface [AsNoTracking])
        generatedCode.Should().Contain("_query.Query<Tenant>()");
    }

    [Fact]
    public void Generator_WithMethodLevelTrackingFalse_ShouldDisableTracking()
    {
        // Arrange - Method-level [Tracking(false)]
        var source = CreateTestCode(
            interfaceCode: @"
    public class Tenant : Entity
    {
        public new Guid Id { get; set; }
        public string Name { get; set; }
        public Guid RestaurantId { get; set; }
    }

    [GenerateRepository<Tenant>]
    [Tracking(true)]
    public interface ITenantRepository
    {
        [Tracking(false)]
        Task<List<Tenant>> FindByRestaurantId(Guid restaurantId);
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();

        // Should use _query.Query (no tracking due to [Tracking(false)])
        generatedCode.Should().Contain("_query.Query<Tenant>()");
    }

    #endregion

    #region Query Methods with Interface Modifiers Tests

    [Fact]
    public void Generator_WithIgnoreQueryFiltersAndQueryMethods_ShouldApplyToQueryMethods()
    {
        // Arrange
        var source = CreateTestCode(
            interfaceCode: @"
    public class Tenant : Entity
    {
        public new Guid Id { get; set; }
        public string Name { get; set; }
        public Guid RestaurantId { get; set; }
    }

    [GenerateRepository<Tenant>]
    [IgnoreQueryFilters]
    public interface ITenantRepository
    {
        Task<List<Tenant>> FindByRestaurantId(Guid restaurantId);
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain(".IgnoreQueryFilters()");
    }

    [Fact]
    public void Generator_WithAsSplitQueryAndQueryMethods_ShouldApplyToQueryMethods()
    {
        // Arrange
        var source = CreateTestCode(
            interfaceCode: @"
    public class Tenant : Entity
    {
        public new Guid Id { get; set; }
        public string Name { get; set; }
        public Guid RestaurantId { get; set; }
    }

    [GenerateRepository<Tenant>]
    [AsSplitQuery]
    public interface ITenantRepository
    {
        Task<List<Tenant>> FindByRestaurantId(Guid restaurantId);
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain(".AsSplitQuery()");
    }

    [Fact]
    public void Generator_WithIncludeAndQueryMethods_ShouldApplyIncludesToQueryMethods()
    {
        // Arrange
        var source = CreateTestCode(
            interfaceCode: @"
    public class Order : Entity
    {
        public Guid CustomerId { get; set; }
    }

    public class Customer : Entity
    {
        public string Name { get; set; }
        public List<Order> Orders { get; set; }
    }

    [GenerateRepository<Customer>]
    [Include(""Orders"")]
    public interface ICustomerQueryRepository
    {
        Task<List<Customer>> FindByName(string name);
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain(".Include(c => c.Orders)");
        generatedCode.Should().Contain("FindByName");
    }

    [Fact]
    public void Generator_WithAllModifiersAndQueryMethods_ShouldApplyAll()
    {
        // Arrange
        var source = CreateTestCode(
            interfaceCode: @"
    public class Order : Entity
    {
        public Guid CustomerId { get; set; }
    }

    public class Customer : Entity
    {
        public string Name { get; set; }
        public List<Order> Orders { get; set; }
    }

    [GenerateRepository<Customer>]
    [Include(""Orders"")]
    [AsSplitQuery]
    [IgnoreQueryFilters]
    public interface ICustomerQueryRepository
    {
        Task<List<Customer>> FindByName(string name);
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain(".IgnoreQueryFilters()");
        generatedCode.Should().Contain(".Include(c => c.Orders)");
        generatedCode.Should().Contain(".AsSplitQuery()");
        generatedCode.Should().Contain("FindByName");
    }

    #endregion

    #region HintName Collision Tests

    [Fact]
    public void Generator_WithSameClassNameInDifferentNamespaces_ShouldNotCollide()
    {
        // Arrange - Two interfaces with same name but different namespaces
        var source = @"
using System;
using Fudie.Infrastructure;

namespace Namespace1
{
    public class Entity { public Guid Id { get; set; } }
    public class Customer : Entity { public string Name { get; set; } }
    public interface ICustomerRepository : IGet<Customer, Guid> { }
}

namespace Namespace2
{
    public class Entity { public Guid Id { get; set; } }
    public class Product : Entity { public string Name { get; set; } }
    public interface IProductRepository : IGet<Product, Guid> { }
}

namespace Fudie.Infrastructure
{
    public interface IGet<T, ID> { }
    public interface IEntityLookup { }
    public interface IChangeTracker { }
}

namespace Fudie.DependencyInjection
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class InjectableAttribute : System.Attribute
    {
        public InjectableAttribute(Microsoft.Extensions.DependencyInjection.ServiceLifetime lifetime) { }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public enum ServiceLifetime { Scoped }
}
";

        // Act
        var result = RunGenerator(source);

        // Assert - Should generate 2 files without collision
        result.GeneratedTrees.Should().HaveCount(2);
    }

    #endregion

    #region Delete Query Method Tests

    [Fact]
    public void Generator_WithDeleteQueryMethod_ShouldGenerateDeleteMethod()
    {
        // Arrange
        var source = CreateTestCode(
            interfaceCode: @"
    public class Tenant : Entity
    {
        public new Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }

    [GenerateRepository<Tenant>]
    public interface ITenantRepository
    {
        Task<int> DeleteByIsActiveFalse();
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("DeleteByIsActiveFalse");
        generatedCode.Should().Contain(".ExecuteDeleteAsync()");
    }

    #endregion

    #region Coverage Gap Tests

    /// <summary>
    /// Standard infrastructure but with parameterless TrackingAttribute constructor
    /// </summary>
    private const string InfrastructureWithParameterlessTracking = @"
namespace Fudie.Infrastructure
{
    public interface IGet<T, ID> { }
    public interface IAdd<T> { }
    public interface IUpdate<T, ID> : IGet<T, ID> { }
    public interface IRemove<T, ID> : IGet<T, ID> { }
    public interface IEntityLookup { }
    public interface IChangeTracker { }

    [System.AttributeUsage(System.AttributeTargets.Interface, AllowMultiple = true)]
    public class IncludeAttribute : System.Attribute
    {
        public IncludeAttribute(params string[] paths) { }
    }

    [System.AttributeUsage(System.AttributeTargets.Interface | System.AttributeTargets.Method)]
    public class AsNoTrackingAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Interface | System.AttributeTargets.Method)]
    public class TrackingAttribute : System.Attribute
    {
        public TrackingAttribute() { }
        public TrackingAttribute(bool enabled) { }
    }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class AsSplitQueryAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class IgnoreQueryFiltersAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class GenerateRepositoryAttribute<TEntity> : System.Attribute where TEntity : class { }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class GenerateRepositoryAttribute<TEntity, TId> : System.Attribute where TEntity : class { }
}

namespace Fudie.DependencyInjection
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class InjectableAttribute : System.Attribute
    {
        public InjectableAttribute(Microsoft.Extensions.DependencyInjection.ServiceLifetime lifetime) { }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public enum ServiceLifetime { Scoped }
}";

    /// <summary>
    /// Standard infrastructure (same as CreateTestCode) for custom source tests
    /// </summary>
    private const string StandardInfrastructure = @"
namespace Fudie.Infrastructure
{
    public interface IGet<T, ID> { }
    public interface IAdd<T> { }
    public interface IUpdate<T, ID> : IGet<T, ID> { }
    public interface IRemove<T, ID> : IGet<T, ID> { }
    public interface IEntityLookup { }
    public interface IChangeTracker { }

    [System.AttributeUsage(System.AttributeTargets.Interface, AllowMultiple = true)]
    public class IncludeAttribute : System.Attribute
    {
        public IncludeAttribute(params string[] paths) { }
    }

    [System.AttributeUsage(System.AttributeTargets.Interface | System.AttributeTargets.Method)]
    public class AsNoTrackingAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Interface | System.AttributeTargets.Method)]
    public class TrackingAttribute : System.Attribute
    {
        public TrackingAttribute(bool enabled) { }
    }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class AsSplitQueryAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class IgnoreQueryFiltersAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class GenerateRepositoryAttribute<TEntity> : System.Attribute where TEntity : class { }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class GenerateRepositoryAttribute<TEntity, TId> : System.Attribute where TEntity : class { }
}

namespace Fudie.DependencyInjection
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class InjectableAttribute : System.Attribute
    {
        public InjectableAttribute(Microsoft.Extensions.DependencyInjection.ServiceLifetime lifetime) { }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public enum ServiceLifetime { Scoped }
}";

    [Fact]
    public void Generator_WithEntityInDifferentNamespace_ShouldFindEntityAndAddUsing()
    {
        // Covers: FindEntityType (lines 472-473), additionalUsings (lines 298-300)
        var source = @"
using System;
using Fudie.Infrastructure;

namespace DomainNamespace
{
    public class Entity { public Guid Id { get; set; } }
    public class Customer : Entity { public string Name { get; set; } }
}

namespace AppNamespace
{
    public interface ICustomerRepository : IGet<DomainNamespace.Customer, Guid> { }
}
" + StandardInfrastructure;

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("using DomainNamespace;");
        generatedCode.Should().Contain("public class CustomerRepository : ICustomerRepository");
    }

    [Fact]
    public void Generator_WithMethodLevelTrackingWithoutArgs_ShouldEnableTracking()
    {
        // Covers: GetMethodUseTracking return true (line 611)
        var source = @"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fudie.Infrastructure;

namespace TestNamespace
{
    public class Entity { public Guid Id { get; set; } }
    public class Tenant : Entity
    {
        public string Name { get; set; }
    }

    [GenerateRepository<Tenant>]
    public interface ITenantRepository
    {
        [Tracking]
        Task<Tenant?> FindFirstByName(string name);
    }
}
" + InfrastructureWithParameterlessTracking;

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        // [Tracking] without args = true → should use _entityLookup.Set (tracking)
        generatedCode.Should().Contain("_entityLookup.Set<Tenant>()");
    }

    [Fact]
    public void Generator_WithInterfaceLevelTrackingWithoutArgs_ShouldReturnNullTrackingValue()
    {
        // Covers: GetTrackingAttributeValue return null (line 457)
        var source = @"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fudie.Infrastructure;

namespace TestNamespace
{
    public class Entity { public Guid Id { get; set; } }
    public class Tenant : Entity
    {
        public string Name { get; set; }
    }

    [GenerateRepository<Tenant>]
    [Tracking]
    public interface ITenantRepository
    {
        Task<Tenant?> FindFirstByName(string name);
    }
}
" + InfrastructureWithParameterlessTracking;

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        // [Tracking] without args → GetTrackingAttributeValue returns null
        // interfaceDefaultTracking = true (trackingValue != false)
        // So methods should use tracking by default
        generatedCode.Should().Contain("_entityLookup.Set<Tenant>()");
    }

    [Fact]
    public void Generator_WithNonGenericGenerateRepository_ShouldReportFUDIE002()
    {
        // Covers: ExtractRepositoryConfiguration entityTypeName == null (lines 234-244)
        var source = @"
using System;
using Fudie.Infrastructure;

namespace TestNamespace
{
    public class Entity { public Guid Id { get; set; } }
    public class Customer : Entity { public string Name { get; set; } }

    [GenerateRepository]
    public interface ICustomerRepository
    {
    }
}

namespace Fudie.Infrastructure
{
    public interface IGet<T, ID> { }
    public interface IAdd<T> { }
    public interface IUpdate<T, ID> : IGet<T, ID> { }
    public interface IRemove<T, ID> : IGet<T, ID> { }
    public interface IEntityLookup { }
    public interface IChangeTracker { }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class GenerateRepositoryAttribute : System.Attribute { }
}

namespace Fudie.DependencyInjection
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class InjectableAttribute : System.Attribute
    {
        public InjectableAttribute(Microsoft.Extensions.DependencyInjection.ServiceLifetime lifetime) { }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public enum ServiceLifetime { Scoped }
}";

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().Contain(d => d.Id == "FUDIE002");
        var diagnostic = result.Diagnostics.First(d => d.Id == "FUDIE002");
        diagnostic.GetMessage().Should().Contain("does not implement any Fudie repository interfaces");
    }

    [Fact]
    public void Generator_WithHintNameCollision_ShouldReportFUDIE001()
    {
        // Covers: Execute catch block (lines 92-104), RepositoryInterfaceInfo.Syntax (line 636)
        // IX_Repository → class name X_Repository
        // X.IRepository → class name X_Repository (COLLISION!)
        var source = @"
using System;
using Fudie.Infrastructure;

namespace TestNamespace
{
    public class Entity { public Guid Id { get; set; } }
    public class Customer : Entity { public string Name { get; set; } }

    public interface IX_Repository : IGet<Customer, Guid> { }

    public class X
    {
        public interface IRepository : IAdd<Customer> { }
    }
}
" + StandardInfrastructure;

        // Act
        var result = RunGenerator(source);

        // Assert - One succeeds, one fails with FUDIE001 (hint name collision throws exception)
        result.Diagnostics.Should().Contain(d => d.Id == "FUDIE001");
    }

    private const string InfrastructureWithStringTracking = @"
namespace Fudie.Infrastructure
{
    public interface IGet<T, ID> { }
    public interface IAdd<T> { }
    public interface IUpdate<T, ID> : IGet<T, ID> { }
    public interface IRemove<T, ID> : IGet<T, ID> { }
    public interface IEntityLookup { }
    public interface IChangeTracker { }

    [System.AttributeUsage(System.AttributeTargets.Interface, AllowMultiple = true)]
    public class IncludeAttribute : System.Attribute
    {
        public IncludeAttribute(params string[] paths) { }
    }

    [System.AttributeUsage(System.AttributeTargets.Interface | System.AttributeTargets.Method)]
    public class AsNoTrackingAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Interface | System.AttributeTargets.Method)]
    public class TrackingAttribute : System.Attribute
    {
        public TrackingAttribute(string value) { }
    }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class AsSplitQueryAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class IgnoreQueryFiltersAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class GenerateRepositoryAttribute<TEntity> : System.Attribute where TEntity : class { }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class GenerateRepositoryAttribute<TEntity, TId> : System.Attribute where TEntity : class { }
}

namespace Fudie.DependencyInjection
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class InjectableAttribute : System.Attribute
    {
        public InjectableAttribute(Microsoft.Extensions.DependencyInjection.ServiceLifetime lifetime) { }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public enum ServiceLifetime { Scoped }
}";

    [Fact]
    public void Generator_WithStringTrackingArgAtInterfaceLevel_ShouldTreatAsDefaultTracking()
    {
        // Covers: GetTrackingAttributeValue line 455 — ConstructorArguments[0].Value is NOT bool
        // When [Tracking("enabled")] is used with string arg, value is string not bool
        // → falls through inner if → returns null → interfaceDefaultTracking = true
        var source = @"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fudie.Infrastructure;

namespace TestNamespace
{
    public class Entity { public Guid Id { get; set; } }
    public class Tenant : Entity
    {
        public string Name { get; set; }
    }

    [GenerateRepository<Tenant>]
    [Tracking(""enabled"")]
    public interface ITenantRepository
    {
        Task<Tenant?> FindFirstByName(string name);
    }
}
" + InfrastructureWithStringTracking;

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        // [Tracking("enabled")] → GetTrackingAttributeValue returns null (non-bool arg)
        // → trackingValue = null, interfaceDefaultTracking = true (trackingValue != false)
        generatedCode.Should().Contain("_entityLookup.Set<Tenant>()");
    }

    [Fact]
    public void Generator_WithStringTrackingArgAtMethodLevel_ShouldDefaultToTrackingTrue()
    {
        // Covers: GetMethodUseTracking line 605 missing branch — ConstructorArguments[0].Value is NOT bool
        // When method has [Tracking("enabled")] with string arg:
        // trackingAttr != null, ConstructorArguments.Length > 0 but Value is string not bool
        // → inner if fails → falls to return true (line 611)
        var source = @"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fudie.Infrastructure;

namespace TestNamespace
{
    public class Entity { public Guid Id { get; set; } }
    public class Tenant : Entity
    {
        public string Name { get; set; }
    }

    [GenerateRepository<Tenant>]
    public interface ITenantRepository
    {
        [Tracking(""enabled"")]
        Task<Tenant?> FindFirstByName(string name);
    }
}
" + InfrastructureWithStringTracking;

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        // [Tracking("enabled")] on method → non-bool arg → return true → tracking enabled
        generatedCode.Should().Contain("_entityLookup.Set<Tenant>()");
    }

    [Fact]
    public void IsInfrastructureMethod_WithNullContainingType_ShouldReturnFalse()
    {
        // Covers: IsInfrastructureMethod containingType == null (line 625)
        var method = typeof(RepositorySourceGenerator).GetMethod(
            "IsInfrastructureMethod",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Create a mock IMethodSymbol with null ContainingType
        var mockMethod = new Mock<IMethodSymbol>();
        mockMethod.Setup(m => m.ContainingType).Returns((INamedTypeSymbol)null!);

        // Act
        var result = (bool)method!.Invoke(null, new object[] { mockMethod.Object })!;

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("Fudie.Infrastructure.IGet<T, ID>")]
    [InlineData("Fudie.Infrastructure.IAdd<T>")]
    [InlineData("Fudie.Infrastructure.IUpdate<T, ID>")]
    [InlineData("Fudie.Infrastructure.IRemove<T, ID>")]
    public void IsInfrastructureMethod_WithKnownInfraType_ShouldReturnTrue(string infraType)
    {
        // Covers: IsInfrastructureMethod line 629 — true branches of || chain
        var method = typeof(RepositorySourceGenerator).GetMethod(
            "IsInfrastructureMethod",
            BindingFlags.NonPublic | BindingFlags.Static);

        var mockMethod = new Mock<IMethodSymbol>();
        var mockContainingType = new Mock<INamedTypeSymbol>();
        var mockConstructedFrom = new Mock<INamedTypeSymbol>();
        mockConstructedFrom.Setup(t => t.ToDisplayString(It.IsAny<SymbolDisplayFormat>())).Returns(infraType);
        mockContainingType.Setup(t => t.ConstructedFrom).Returns(mockConstructedFrom.Object);
        mockMethod.Setup(m => m.ContainingType).Returns(mockContainingType.Object);

        var result = (bool)method!.Invoke(null, new object[] { mockMethod.Object })!;

        result.Should().BeTrue();
    }

    [Fact]
    public void IsInfrastructureMethod_WithUnknownType_ShouldReturnFalse()
    {
        // Covers: IsInfrastructureMethod line 629 — all false path (no match)
        var method = typeof(RepositorySourceGenerator).GetMethod(
            "IsInfrastructureMethod",
            BindingFlags.NonPublic | BindingFlags.Static);

        var mockMethod = new Mock<IMethodSymbol>();
        var mockContainingType = new Mock<INamedTypeSymbol>();
        var mockConstructedFrom = new Mock<INamedTypeSymbol>();
        mockConstructedFrom.Setup(t => t.ToDisplayString(It.IsAny<SymbolDisplayFormat>())).Returns("MyApp.ICustomRepo<T>");
        mockContainingType.Setup(t => t.ConstructedFrom).Returns(mockConstructedFrom.Object);
        mockMethod.Setup(m => m.ContainingType).Returns(mockContainingType.Object);

        var result = (bool)method!.Invoke(null, new object[] { mockMethod.Object })!;

        result.Should().BeFalse();
    }

    [Fact]
    public void GetRepositoryInterfaceInfo_WithNullSymbol_ShouldReturnNull()
    {
        // Covers: GetRepositoryInterfaceInfo line 50 — interfaceSymbol == null
        // Mock<SemanticModel> is NOT a CSharpSemanticModel, so the GetDeclaredSymbol
        // extension method's cast fails and returns null → enters line 50
        var tree = CSharpSyntaxTree.ParseText("interface IFoo { }");
        var interfaceDecl = tree.GetRoot().DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax>().First();

        var mockModel = new Mock<SemanticModel>();

        // GeneratorSyntaxContext fields: SyntaxHelper:ISyntaxHelper, _semanticModel:Lazy<SemanticModel>, <Node>k__BackingField:SyntaxNode
        var ctx = default(GeneratorSyntaxContext);
        var tr = __makeref(ctx);

        // Set Node backing field
        var nodeField = typeof(GeneratorSyntaxContext).GetField(
            "<Node>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance);
        nodeField!.SetValueDirect(tr, (SyntaxNode)interfaceDecl);

        // Set _semanticModel as Lazy<SemanticModel> wrapping our mock
        var modelField = typeof(GeneratorSyntaxContext).GetField(
            "_semanticModel",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var lazyModel = new Lazy<SemanticModel>(() => mockModel.Object);
        modelField!.SetValueDirect(tr, lazyModel);

        // Call GetRepositoryInterfaceInfo via reflection
        var method = typeof(RepositorySourceGenerator).GetMethod(
            "GetRepositoryInterfaceInfo",
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = method!.Invoke(null, new object[] { ctx });

        // Assert
        result.Should().BeNull();
    }

    #endregion
}
