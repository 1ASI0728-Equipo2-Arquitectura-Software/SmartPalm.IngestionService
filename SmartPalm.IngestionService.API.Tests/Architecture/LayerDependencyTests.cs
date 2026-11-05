using System.Reflection;
using SmartPalm.IngestionService.Domain.Readings;

namespace SmartPalm.IngestionService.API.Tests.Architecture;

public class LayerDependencyTests
{
    [Fact]
    public void DomainPublicApi_DoesNotExposeOuterLayers()
    {
        var forbidden = new[] { ".Application", ".Infrastructure", ".Interfaces", ".Api" };
        var violations = typeof(SensorReading).Assembly.GetTypes()
            .Where(type => type.Namespace?.StartsWith("SmartPalm.IngestionService.Domain", StringComparison.Ordinal) == true)
            .SelectMany(ExposedTypes).Where(type => forbidden.Any(segment => type.Namespace?.Contains(segment, StringComparison.Ordinal) == true))
            .Select(type => type.FullName).Distinct().ToArray();
        Assert.Empty(violations);
    }

    [Fact]
    public void CommandAndQueryTypes_UseDomainModelNamespaces()
    {
        var domainTypes = typeof(SensorReading).Assembly.GetTypes();
        var commands = domainTypes.Where(type => type.Name.EndsWith("Command", StringComparison.Ordinal)).ToArray();
        var queries = domainTypes.Where(type => type.Name.EndsWith("Query", StringComparison.Ordinal)).ToArray();

        Assert.NotEmpty(commands);
        Assert.NotEmpty(queries);
        Assert.All(commands, type => Assert.Equal("SmartPalm.IngestionService.Domain.Model.Commands", type.Namespace));
        Assert.All(queries, type => Assert.Equal("SmartPalm.IngestionService.Domain.Model.Queries", type.Namespace));
    }

    private static IEnumerable<Type> ExposedTypes(Type type)
    {
        foreach (var constructor in type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)) foreach (var parameter in constructor.GetParameters()) yield return Unwrap(parameter.ParameterType);
        foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)) { yield return Unwrap(method.ReturnType); foreach (var parameter in method.GetParameters()) yield return Unwrap(parameter.ParameterType); }
        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)) yield return Unwrap(property.PropertyType);
    }
    private static Type Unwrap(Type type) => type.IsArray ? type.GetElementType() ?? type : type.IsGenericType ? type.GetGenericArguments().FirstOrDefault() ?? type : type;
}
