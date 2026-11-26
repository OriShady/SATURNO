using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NoSqlU.Api.Models;
using NoSqlU.Api.Models.ViewModels;
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
        Assert.Equal(expectedValid, IsValid(new RegistroUsuarioDto(), nameof(RegistroUsuarioDto.CorreoEmpresarial), email));
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
        Assert.Equal(expectedValid, IsValid(new RegistroUsuarioDto(), nameof(RegistroUsuarioDto.Telefono), phone));
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
        Assert.False(IsValid(
            new RegistroUsuarioDto(),
            nameof(RegistroUsuarioDto.FechaNacimiento),
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

    private static bool IsValid(object model, string memberName, object value)
    {
        var context = new ValidationContext(model) { MemberName = memberName };
        return Validator.TryValidateProperty(value, context, new List<ValidationResult>());
    }
}
