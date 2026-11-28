using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NoSqlU.Api.Models.Empleados;
using NoSqlU.Domain.Entities;
using Xunit;

namespace NoSqlU.Tests;

public class EmployeeInputValidationTests
{
    [Theory]
    [InlineData("empleado@empresa.mx", true)]
    [InlineData("correo-sin-arroba", false)]
    public void EmployeeFormValidatesEmailFormat(string email, bool expectedValid)
    {
        Assert.Equal(expectedValid, IsValid(new EmpleadoFormViewModel(), nameof(EmpleadoFormViewModel.CorreoEmpresarial), email));
    }

    [Theory]
    [InlineData("7221234567", true)]
    [InlineData("", true)]
    [InlineData(null, true)]
    [InlineData("722123456", false)]
    [InlineData("722-123-4567", false)]
    public void EmployeeFormAllowsOnlyOptionalTenDigitPhone(string phone, bool expectedValid)
    {
        Assert.Equal(expectedValid, IsValid(new EmpleadoFormViewModel(), nameof(EmpleadoFormViewModel.Telefono), phone));
    }

    [Fact]
    public void EmployeeBirthDateRequiresAtLeastEighteenYears()
    {
        Assert.True(IsValid(
            new EmpleadoFormViewModel(),
            nameof(EmpleadoFormViewModel.FechaNacimiento),
            DateTime.Today.AddYears(-18)));
        Assert.False(IsValid(
            new EmpleadoFormViewModel(),
            nameof(EmpleadoFormViewModel.FechaNacimiento),
            DateTime.Today.AddYears(-18).AddDays(1)));
    }

    [Fact]
    public void EmployeeHireDateCannotPrecedeBirthDate()
    {
        var model = new EmpleadoFormViewModel { FechaNacimiento = new DateTime(1990, 5, 10) };
        Assert.True(IsValid(model, nameof(EmpleadoFormViewModel.FechaIngreso), new DateTime(1990, 5, 10)));
        Assert.False(IsValid(model, nameof(EmpleadoFormViewModel.FechaIngreso), new DateTime(1990, 5, 9)));
    }

    [Fact]
    public void CourseCertificateCannotPrecedeCourseStart()
    {
        var model = new CursoEmpleadoFormViewModel { FechaInicio = new DateTime(2025, 2, 10) };
        Assert.True(IsValid(model, nameof(CursoEmpleadoFormViewModel.FechaEntregaConstancia), new DateTime(2025, 2, 10)));
        Assert.False(IsValid(model, nameof(CursoEmpleadoFormViewModel.FechaEntregaConstancia), new DateTime(2025, 2, 9)));
    }

    [Fact]
    public void EmployeeDocumentWithLegacyCredentialsCanStillBeRead()
    {
        var document = new BsonDocument
        {
            { "_id", 42 },
            { "nombre", "Ana" },
            { "credencial", new BsonDocument("usuario", "ana") }
        };

        var employee = BsonSerializer.Deserialize<Usuario>(document);

        Assert.Equal(42, employee.Id);
        Assert.Equal("Ana", employee.Nombres);
    }

    private static bool IsValid(object model, string memberName, object value)
    {
        var context = new ValidationContext(model) { MemberName = memberName };
        return Validator.TryValidateProperty(value, context, new List<ValidationResult>());
    }
}
