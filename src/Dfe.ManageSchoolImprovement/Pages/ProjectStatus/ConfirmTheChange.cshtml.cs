using System.ComponentModel.DataAnnotations;
using Dfe.ManageSchoolImprovement.Application.SupportProject.Queries;
using Dfe.ManageSchoolImprovement.Domain.ValueObjects;
using Dfe.ManageSchoolImprovement.Frontend.Models;
using Dfe.ManageSchoolImprovement.Frontend.Services;
using Dfe.ManageSchoolImprovement.Frontend.ViewModels;
using Dfe.ManageSchoolImprovement.Utils;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dfe.ManageSchoolImprovement.Frontend.Pages.ProjectStatus;

public class ConfirmChangeModel(
    ISupportProjectQueryService supportProjectQueryService,
    IGetEstablishment getEstablishment,
    ErrorService errorService,
    IUserRepository userRepository)
    : BaseSupportProjectEstablishmentPageModel(supportProjectQueryService, getEstablishment, errorService),
        IDateValidationMessageProvider
{
    public string ReturnPage { get; set; }

    public ProjectStatusValue? PreviousSupportProjectStatus { get; set; }
    public SupportProjectEligibilityStatus? PreviousEligibilityStatus { get; set; }
    [BindProperty(SupportsGet = true)] public ProjectStatusValue? SupportProjectStatus { get; set; }
    [BindProperty(SupportsGet = true)] public SupportProjectEligibilityStatus? EligibilityStatus { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateSupportIsDueToEnd { get; set; }
    
    [BindProperty(Name = "status-eligibility-change-date", BinderType = typeof(DateInputModelBinder))]
    [DateValidation(DateRangeValidationService.DateRange.PastOrToday)]
    public DateTime? StatusOrEligiblityChangeDate { get; set; }
    
    public bool ShowError { get; set; }

    string IDateValidationMessageProvider.SomeMissing(string displayName, IEnumerable<string> missingParts)
    {
        return $"Date must include a {string.Join(" and ", missingParts)}";
    }
    string IDateValidationMessageProvider.AllMissing => "Enter a date";

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        ReturnPage = @Links.ProjectStatusTab.IsThisSchoolEligibleForIntervention.Page;

        await base.GetSupportProject(id, cancellationToken);

        if (SupportProject != null)
        {
            PreviousSupportProjectStatus = SupportProject.ProjectStatus;
            PreviousEligibilityStatus = SupportProject.SupportProjectEligibilityStatus!.Value;
        }
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken cancellationToken)
    {
        await base.GetSupportProject(id, cancellationToken);
        
        if (!StatusOrEligiblityChangeDate.HasValue)
        {
            ModelState.AddModelError("status-eligibility-change-date", "Enter a date");

        }

        if (!ModelState.IsValid)
        {
            _errorService.AddErrors(Request.Form.Keys, ModelState);
            ShowError = true;
            return await base.GetSupportProject(id, cancellationToken);
        }
        
        return RedirectToPage(@Links.ProjectStatusTab.EnterDetailsAboutTheChange.Page,
            new
            {
                id,
                SupportProjectStatus,
                EligibilityStatus,
                DateSupportIsDueToEnd = DateSupportIsDueToEnd?.ToString("yyyy-MM-dd"),
                StatusOrEligiblityChangeDate = StatusOrEligiblityChangeDate?.ToString("yyyy-MM-dd")
            });
    }
}

