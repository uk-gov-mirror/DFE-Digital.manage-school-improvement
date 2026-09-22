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

public class IsThisSchoolEligibleForInterventionModel(
    ISupportProjectQueryService supportProjectQueryService,
    IGetEstablishment getEstablishment,
    ErrorService errorService)
    : BaseSupportProjectEstablishmentPageModel(supportProjectQueryService, getEstablishment, errorService),
        IDateValidationMessageProvider
{
    public string ReturnPage { get; set; }

    [BindProperty(SupportsGet = true)] public ProjectStatusValue SupportProjectStatus { get; set; }
    
    [BindProperty(SupportsGet = true)] public bool ProjectStatusChanged { get; set; }

    [BindProperty]
    [Display(Name = "Is this school still eligible for targeted intervention?")]
    public bool? SchoolIsEligible { get; set; } = false;
    
    [BindProperty(Name = "support-is-due-to-end-date", BinderType = typeof(DateInputModelBinder))]
    public DateTime? DateSupportIsDueToEnd { get; set; }
    
    public SupportProjectEligibilityStatus? EligibilityStatus { get; set; }
    
    public required IList<RadioButtonsLabelViewModel> RadioButtons { get; set; }
    
    public string? ErrorMessage { get; set; }

    public bool ShowError { get; set; }

    string IDateValidationMessageProvider.SomeMissing(string displayName, IEnumerable<string> missingParts)
    {
        return $"Date must include a {string.Join(" and ", missingParts)}";
    }
    
    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        ReturnPage = @Links.ProjectStatusTab.ChangeProjectStatus.Page;

        await base.GetSupportProject(id, cancellationToken);
        
        SchoolIsEligible = SupportProject?.SupportProjectEligibilityStatus == SupportProjectEligibilityStatus.EligibleForSupport;

        RadioButtons = RadioButtonModel;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken cancellationToken)
    {
        await base.GetSupportProject(id, cancellationToken);

        if (!ModelState.IsValid)
        {
            _errorService.AddErrors(Request.Form.Keys, ModelState);
            ShowError = true;
            RadioButtons = RadioButtonModel;
            return await base.GetSupportProject(id, cancellationToken);
        }

        EligibilityStatus = SchoolIsEligible == true ? SupportProjectEligibilityStatus.EligibleForSupport : SupportProjectEligibilityStatus.NotEligibleForSupportMidIntervention;
        
        return RedirectToPage(@Links.ProjectStatusTab.ConfirmTheChange.Page,
            new
            {
                id,
                SupportProjectStatus,
                EligibilityStatus,
                DateSupportIsDueToEnd = DateSupportIsDueToEnd?.ToString("yyyy-MM-dd")
            });
    }

    private IList<RadioButtonsLabelViewModel> RadioButtonModel
    {
        get
        {
            var list = new List<RadioButtonsLabelViewModel>
            {
                new()
                {
                    Id = "yes",
                    Name = "Yes",
                    Value = "True"
                },
                new()
                {
                    Id = "no",
                    Name = "No",
                    Value = "False",
                }
            };

            return list;
        }
    }
}

