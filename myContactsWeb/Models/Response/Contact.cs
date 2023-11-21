using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myContacts.Models
{
    public class Contact
    {
        [Key]
        public int ContactId{ get; set; }
        [MaxLength(50)]
        public required string Name { get; set; }
        [Phone]
        [MaxLength(10)]
        public required string Phone { get; set; }
        [Phone]
        [MaxLength(10)]
        public string? Fax { get; set; }
        [EmailAddress]
        [MaxLength(50)]
        public required string Email { get; set; }
        [Column(TypeName = "ntext")]
        public string? Notes { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        [MaxLength(20)]
        public string? LastUpdateUsername { get; set; }


        public bool IsValid()
        {
            var validationContext = new ValidationContext(this, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(this, validationContext, validationResults, validateAllProperties: true);

            return isValid;
        }

    }
}