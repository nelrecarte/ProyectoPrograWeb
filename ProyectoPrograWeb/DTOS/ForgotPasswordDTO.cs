using System.ComponentModel.DataAnnotations;

namespace ProyectoQ3Backend.DTOs;

public class ForgotPasswordDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
