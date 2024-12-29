using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace PaymentGateway.Api.Application.Models.Requests;

public class PostPaymentRequest
{
    [Required(ErrorMessage = "Card number is required.")]
    [MinLength(14, ErrorMessage = "Card number must be at least 14 characters long.")]
    [MaxLength(19, ErrorMessage = "Card number must not exceed 19 characters.")]
    [RegularExpression("^[0-9]+$", ErrorMessage = "Card number must only contain numeric characters.")]
    public string CardNumber { get; set; }

    [Required(ErrorMessage = "Expiry month is required.")]
    [Range(1, 12, ErrorMessage = "Expiry month must be between 1 and 12.")]
    public int ExpiryMonth { get; set; }

    [Required(ErrorMessage = "Expiry year is required.")]
    [CustomValidation(typeof(PostPaymentRequest), nameof(ValidateExpiryDate))]
    public int ExpiryYear { get; set; }

    [Required(ErrorMessage = "Currency is required.")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be exactly 3 characters.")]
    [CustomValidation(typeof(PostPaymentRequest), nameof(ValidateCurrencyCode))]
    public string Currency { get; set; }

    [Required(ErrorMessage = "Amount is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be a positive integer.")]
    public int Amount { get; set; }

    [Required(ErrorMessage = "CVV is required.")]
    [RegularExpression("^[0-9]{3,4}$", ErrorMessage = "CVV must be 3 or 4 numeric characters.")]
    public string Cvv { get; set; } = null!;

    public static ValidationResult ValidateExpiryDate(int expiryYear, ValidationContext context)
    {
        var instance = (PostPaymentRequest)context.ObjectInstance;

        if (instance.ExpiryMonth < 1 || instance.ExpiryMonth > 12)
        {
            return new ValidationResult("Invalid expiry month.");
        }

        if (instance.ExpiryYear <= 0)
        {
            return new ValidationResult("The expiry date must be in the future.");
        }

        var today = DateTime.UtcNow;
        var expiryDate = new DateTime(instance.ExpiryYear, instance.ExpiryMonth, 1).AddMonths(1).AddDays(-1);

        if (expiryDate < today)
        {
            return new ValidationResult("The expiry date must be in the future.");
        }

        return ValidationResult.Success;
    }

    public static ValidationResult ValidateCurrencyCode(string currencyCode, ValidationContext context)
    {
        var allowedCurrencies = new List<string> { "USD", "EUR", "GBP" };

        return (!allowedCurrencies.Contains(currencyCode) ? new ValidationResult($"Currency must be one of the following: {string.Join(", ", allowedCurrencies)}.") : ValidationResult.Success)!;
    }
}
