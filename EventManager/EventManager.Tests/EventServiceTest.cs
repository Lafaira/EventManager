using EventManager.Models;
using EventManager.Models.RequestModel;
using EventManager.Services;
using System.ComponentModel.DataAnnotations;

namespace EventManager.Tests
{
    public class EventServiceTest
    {
        private readonly EventService _eventService;

        public EventServiceTest()
        {
            _eventService = new EventService();
        }

        public static IEnumerable<object[]> TestData()
        {
            return
            [
                [new GetEventsQuery { Title = "ti"}, new PageInfo(), 2],
                [new GetEventsQuery { Title = ""}, new PageInfo(), 2],
                [new GetEventsQuery { From = new DateTime(2026, 01, 01), To = new DateTime(2026, 12,31) }, new PageInfo(), 1],
                [null, new PageInfo { Page=1, PageSize=1 }, 1],
                [new GetEventsQuery { Title = "ti", From = new DateTime(2027, 01, 01), To = new DateTime(2028, 12, 31) }, new PageInfo { Page=1, PageSize=1 }, 1]
            ];
        }

        [Fact]
        public void PostEvent_ReturnCorrectResult()
        {
            var eventItem = new Event()
            {
                Id = 3,
                Title = "Foo",
                Description = "Bar",
                StartAt = new DateTime(2025, 06, 10),
                EndAt = new DateTime(2026, 01, 01)
            };

            var result = _eventService.PostEvent(eventItem);

            Assert.Equal(eventItem, result);
        }

        [Fact]
        public void GetAllEvents_ReturnCorrectResult()
        {
            var result = _eventService.GetAllEvents(pageInfo: new PageInfo());

            Assert.Equal(2, result.EventArr.Count());
        }

        [Fact]
        public void GetEvent_ReturnCorrectResult()
        {
            int id = 1;

            var result = _eventService.GetEvent(id);

            Assert.NotNull(result);
        }

        [Fact]
        public void PutEvent_ReturnCorrectResult()
        {
            var id = 1;
            var eventItem = new Event()
            {
                Id = 1,
                Title = "PutTitle",
                Description = "Description",
                StartAt = new DateTime(2026, 09, 11),
                EndAt = new DateTime(2026, 10, 11),
            };

            var result = _eventService.PutEvent(1, eventItem);

            Assert.True(result);
        }

        [Fact]
        public void DeleteEvent_ReturnCorrectResult()
        {
            var id = 1;

            var result = _eventService.DeleteEvent(id);

            Assert.True(result);
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void Filter_GetAllEvents_ReturnCorrectResult(GetEventsQuery filter, PageInfo pageInfo, int expected)
        {

            var result = _eventService.GetAllEvents(pageInfo, filter);

            Assert.Equal(expected, result.EventArr.Count());
        }

        [Fact]
        public void GetEvent_ReturnThrowsNotFoundException()
        {
            int id = 5;

            Assert.Equal("Нет события с таким id", Assert.Throws<NotFoundException>(() => _eventService.GetEvent(id)).Message);
        }

        [Fact]
        public void PutEvent_ReturnThrowsNotFoundException()
        {
            var id = 5;

            var eventItem = new Event()
            {
                Id = 5,
                Title = "PutTitle",
                Description = "Description",
                StartAt = new DateTime(2026, 09, 11),
                EndAt = new DateTime(2026, 10, 11),
            };

            Assert.Equal("Нет события с таким id", Assert.Throws<NotFoundException>(() => _eventService.PutEvent(id, eventItem)).Message);
        }

        [Fact]
        public void PostEvent_ReturnThrowsNotFoundException()
        {
            var eventItem = new Event()
            {
                Id = 1,
                Title = "Foo",
                Description = "Bar",
                StartAt = new DateTime(2025, 06, 10),
                EndAt = new DateTime(2026, 01, 01)
            };

            Assert.Equal("Событие с таким id уже существует", Assert.Throws<NotFoundException>(() => _eventService.PostEvent(eventItem)).Message);
        }

        [Fact]
        public void FaileValidDate() // Тестирую валидацию, а не PutEvent, тк в PutEvent не попадет не валлидированное значение
        {
            var id = 1;

            var eventItem = new Event()
            {
                Id = 1,
                Title = "PutTitle",
                Description = "Description",
                StartAt = new DateTime(2027, 09, 11),
                EndAt = new DateTime(2026, 10, 11),
            };

            var validContext = new ValidationContext(eventItem);
            var results = eventItem.Validate(validContext).ToList();

            Assert.Single(results);
            Assert.Equal("Дата окончания должна быть позднее даты начала", results[0].ErrorMessage);
        }

    }
}
