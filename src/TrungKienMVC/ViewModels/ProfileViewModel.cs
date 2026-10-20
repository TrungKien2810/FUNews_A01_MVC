using System.ComponentModel.DataAnnotations;

namespace TrungKienMVC.ViewModels;

public class ProfileViewModel
{
    public short AccountId { get; set; }

    [Required(ErrorMessage = "Account name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Account name must be from 2 to 100 characters.")]
    [Display(Name = "Account name")]
    public string AccountName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email is not valid.")]
    [StringLength(70, ErrorMessage = "Email must be at most 70 characters.")]
    [Display(Name = "Email")]
    public string AccountEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Current password is required to save changes.")]
    [StringLength(70, ErrorMessage = "Current password must be at most 70 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "Current password")]
    public string CurrentPassword { get; set; } = string.Empty;

    [StringLength(70, MinimumLength = 2, ErrorMessage = "New password must be from 2 to 70 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "New password (leave blank to keep)")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [StringLength(70, ErrorMessage = "Confirm password must be at most 70 characters.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Confirm password does not match.")]
    [Display(Name = "Confirm new password")]
    public string? ConfirmNewPassword { get; set; }
}
