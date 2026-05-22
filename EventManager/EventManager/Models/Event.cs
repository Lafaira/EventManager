using System.ComponentModel.DataAnnotations;

namespace EventManager.Models
{
    public class Event
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
    }
}
