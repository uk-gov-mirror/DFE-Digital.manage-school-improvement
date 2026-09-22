using Dfe.ManageSchoolImprovement.Application.SupportProject.Queries;
using Dfe.ManageSchoolImprovement.Domain.ValueObjects;
using Dfe.ManageSchoolImprovement.Frontend.Models;
using Dfe.ManageSchoolImprovement.Frontend.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.ManageSchoolImprovement.Frontend.Pages.ProjectStatus;

public class EnterDetailsAboutTheChangeModel(
    ISupportProjectQueryService supportProjectQueryService,
    IGetEstablishment getEstablishment,
    ErrorService errorService)
    : BaseSupportProjectEstablishmentPageModel(supportProjectQueryService, getEstablishment, errorService),
        IDateValidationMessageProvider
{
    public string ReturnPage { get; set; }
    
    [BindProperty(SupportsGet = true)] public ProjectStatusValue? SupportProjectStatus { get; set; }
    [BindProperty(SupportsGet = true)] public SupportProjectEligibilityStatus? EligibilityStatus { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateSupportIsDueToEnd { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? StatusOrEligiblityChangeDate { get; set; }
    
    [BindProperty(Name = "change-details")]
    public string? ChangeDetails { get; set; }
    
    public bool ShowError { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        ReturnPage = @Links.ProjectStatusTab.ConfirmTheChange.Page;

        await base.GetSupportProject(id, cancellationToken);
        
        ChangeDetails =  SupportProject?.ProjectStatusChangedDetails;
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken cancellationToken)
    {
        await base.GetSupportProject(id, cancellationToken);
        
        if (string.IsNullOrEmpty(ChangeDetails))
        {
            ModelState.AddModelError("change-details", "Enter details");
        }
        
        if (ChangeDetails?.Length > 500)
        {
            ModelState.AddModelError("change-details", "Details must be 500 characters or less");
        }
        
        if (!ModelState.IsValid)
        {
            _errorService.AddErrors(Request.Form.Keys, ModelState);
            ShowError = true;
            return await base.GetSupportProject(id, cancellationToken);
        }
        
        return RedirectToPage(Links.ProjectStatusTab.CheckYourAnswers.Page, new
        {
            id,
            SupportProjectStatus,
            EligibilityStatus,
            DateSupportIsDueToEnd = DateSupportIsDueToEnd?.ToString("yyyy-MM-dd"),
            StatusOrEligiblityChangeDate = StatusOrEligiblityChangeDate?.ToString("yyyy-MM-dd"),
            ChangeDetails
        });
    }
}
