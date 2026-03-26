using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Pages.Units;

public class CreateModel : PageModel
{
    private readonly IUnitService _unitService;
    private readonly IPropertyService _propertyService;

    public CreateModel(IUnitService unitService, IPropertyService propertyService)
    {
        _unitService = unitService;
        _propertyService = propertyService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();
    public List<Property> PropertyList { get; set; } = new();

    public class InputModel
    {
        [Required, Display(Name = "Property")]
        public int PropertyId { get; set; }

        [Required, MaxLength(20), Display(Name = "Unit Number")]
        public string UnitNumber { get; set; } = string.Empty;

        public int? Floor { get; set; }

        [Required, Range(0, 5)]
        public int Bedrooms { get; set; }

        [Required, Range(0.5, 4.0)]
        public decimal Bathrooms { get; set; } = 1;

        [Required, Range(1, int.MaxValue), Display(Name = "Square Feet")]
        public int SquareFeet { get; set; }

        [Required, Range(0.01, double.MaxValue), Display(Name = "Monthly Rent"), DataType(DataType.Currency)]
        public decimal MonthlyRent { get; set; }

        [Required, Range(0.01, double.MaxValue), Display(Name = "Deposit Amount"), DataType(DataType.Currency)]
        public decimal DepositAmount { get; set; }

        public UnitStatus Status { get; set; } = UnitStatus.Available;

        [MaxLength(1000)]
        public string? Amenities { get; set; }
    }

    public async Task OnGetAsync()
    {
        var props = await _propertyService.GetPropertiesAsync(null, null, true, 1, 100);
        PropertyList = props.Items;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            var props = await _propertyService.GetPropertiesAsync(null, null, true, 1, 100);
            PropertyList = props.Items;
            return Page();
        }

        var unit = new Unit
        {
            PropertyId = Input.PropertyId,
            UnitNumber = Input.UnitNumber,
            Floor = Input.Floor,
            Bedrooms = Input.Bedrooms,
            Bathrooms = Input.Bathrooms,
            SquareFeet = Input.SquareFeet,
            MonthlyRent = Input.MonthlyRent,
            DepositAmount = Input.DepositAmount,
            Status = Input.Status,
            Amenities = Input.Amenities
        };

        await _unitService.CreateAsync(unit);
        TempData["SuccessMessage"] = $"Unit '{unit.UnitNumber}' created successfully.";
        return RedirectToPage("Details", new { id = unit.Id });
    }
}
