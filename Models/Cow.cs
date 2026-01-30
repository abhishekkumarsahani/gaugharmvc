using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GauGhar.Models
{
    public class Cow
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tag number is required")]
        [Display(Name = "Tag Number")]
        [StringLength(50)]
        public string TagNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cow name is required")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Breed { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Purchase Date")]
        [DataType(DataType.Date)]
        public DateTime? PurchaseDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PurchasePrice { get; set; }

        [StringLength(20)]
        public string Color { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Health Status")]
        public string HealthStatus { get; set; } = "Healthy"; // Healthy, Sick, Under Treatment

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Sold, Dead, Pregnant

        [Display(Name = "Is Pregnant")]
        public bool IsPregnant { get; set; }

        [Display(Name = "Pregnancy Date")]
        [DataType(DataType.Date)]
        public DateTime? PregnancyDate { get; set; }

        [Display(Name = "Expected Delivery")]
        [DataType(DataType.Date)]
        public DateTime? ExpectedDeliveryDate { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<MilkRecord>? MilkRecords { get; set; }
        public virtual ICollection<Vaccination>? Vaccinations { get; set; }

        // Calculated property
        [NotMapped]
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - DateOfBirth.Year;
                if (DateOfBirth.Date > today.AddYears(-age)) age--;
                return age;
            }
        }
    }
    }
