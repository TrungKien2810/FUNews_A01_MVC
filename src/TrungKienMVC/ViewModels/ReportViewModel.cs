using System.ComponentModel.DataAnnotations;
using Services;

namespace TrungKienMVC.ViewModels;

public class ReportViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Start date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Start date")]
    public DateTime? StartDate { get; set; }

    [Required(ErrorMessage = "End date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "End date")]
    public DateTime? EndDate { get; set; }

    public NewsReport? Report { get; set; }

    /// <summary>
    /// How many articles the period immediately before this one produced, so the total can
    /// be read as "more or less than usual" instead of a bare number. Null when the period
    /// itself is not valid or no report was built.
    /// </summary>
    public int? PreviousPeriodTotal { get; set; }

    /// <summary>Difference against the previous period; null when there is nothing to compare.</summary>
    public int? Trend => Report is null || PreviousPeriodTotal is null ? null : Report.Total - PreviousPeriodTotal.Value;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate.HasValue && EndDate.HasValue && StartDate.Value.Date > EndDate.Value.Date)
        {
            yield return new ValidationResult("Start date must be before or equal to end date.", new[] { nameof(EndDate) });
        }
    }
}
