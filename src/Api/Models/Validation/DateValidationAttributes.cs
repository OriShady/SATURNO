using System;
using System.ComponentModel.DataAnnotations;

namespace NoSqlU.Api.Models.Validation;

[AttributeUsage(AttributeTargets.Property)]
public sealed class MinimumAgeAttribute : ValidationAttribute
{
    private readonly int _years;

    public MinimumAgeAttribute(int years) => _years = years;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is not DateTime birthDate || birthDate == default ||
            birthDate.Date > DateTime.Today.AddYears(-_years))
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));

        return ValidationResult.Success;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class DateOnOrAfterAttribute : ValidationAttribute
{
    public DateOnOrAfterAttribute(string comparisonProperty) => ComparisonProperty = comparisonProperty;

    public string ComparisonProperty { get; }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;
        if (value is not DateTime date)
            throw new InvalidOperationException($"{nameof(DateOnOrAfterAttribute)} requires date properties.");

        var comparisonProperty = validationContext.ObjectType.GetProperty(ComparisonProperty)
            ?? throw new InvalidOperationException($"Property '{ComparisonProperty}' was not found on {validationContext.ObjectType.Name}.");
        var comparisonValue = comparisonProperty.GetValue(validationContext.ObjectInstance);
        if (comparisonValue is null)
            return ValidationResult.Success;
        if (comparisonValue is not DateTime comparisonDate)
            throw new InvalidOperationException($"Property '{ComparisonProperty}' must be a date.");

        if (date.Date >= comparisonDate.Date)
            return ValidationResult.Success;

        return new ValidationResult(
            FormatErrorMessage(validationContext.DisplayName),
            new[] { validationContext.MemberName ?? ComparisonProperty });
    }
}
