using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NoSqlU.Api.Controllers;
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

    private static bool IsValid(object model, string memberName, object value)
    {
        var context = new ValidationContext(model) { MemberName = memberName };
        return Validator.TryValidateProperty(value, context, new List<ValidationResult>());
    }
}
