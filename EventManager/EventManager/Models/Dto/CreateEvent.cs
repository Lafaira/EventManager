using System.ComponentModel.DataAnnotations;

namespace EventManager.Models.Dto
{
    public class CreateEvent : IValidatableObject
    {
        [Required(ErrorMessage = "Id обязателен для заполнения")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Название обязателен для заполнения")]
        public string Title { get; set; }
        public string? Description { get; set; }

        [Required(ErrorMessage = "Дата начала обязательна для заполнения")]
        [Range(typeof(DateTime), "2020-01-01", "2030-12-31", ErrorMessage = "Некорректная дата")]
        public DateTime StartAt { get; set; }

        [Required(ErrorMessage = "Дата окночания обязательна для заполнения")]
        [Range(typeof(DateTime), "2020-01-01", "2030-12-31", ErrorMessage = "Некорректная дата")]
        public DateTime EndAt { get; set; }

        private int _totalSeats;
        [Required(ErrorMessage = "Значение количества мест обязательно для заполнения")]
        [Range(1, int.MaxValue, ErrorMessage = "Значение количества мест должно быть больше нуля")]
        public int TotalSeats { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndAt <= StartAt)
            {
                yield return new ValidationResult("Дата окончания должна быть позднее даты начала", new[] { nameof(EndAt) });
            }
        }
    }
}
