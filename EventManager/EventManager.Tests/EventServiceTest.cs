using EventManager.Models;
using EventManager.Models.Dto;
using EventManager.Models.RequestModel;
using EventManager.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;

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
            var eventItem = new Event()
            {
                Id = 5,
                Title = "Foo",
                Description = "Bar",
                StartAt = new DateTime(2025, 06, 10),
                EndAt = new DateTime(2026, 01, 01),
                TotalSeats = 3

            };

            var resultEvent = _eventService.PostEvent(eventItem);

            var result = _eventService.GetAllEvents(pageInfo: new PageInfo());

            Assert.Equal(1, result.EventArr.Count());
        }

        [Fact]
        public void GetEvent_ReturnCorrectResult()
        {
            var eventItem = new Event()
            {
                Id = 5,
                Title = "Foo",
                Description = "Bar",
                StartAt = new DateTime(2025, 06, 10),
                EndAt = new DateTime(2026, 01, 01),
                TotalSeats = 3

            };

            var resultEvent = _eventService.PostEvent(eventItem);

            var result = _eventService.GetEvent(eventItem.Id);

            Assert.NotNull(result);
        }

        [Fact]
        public void PutEvent_ReturnCorrectResult()
        {
            var eventItem = new Event()
            {
                Id = 1,
                Title = "Foo",
                Description = "Bar",
                StartAt = new DateTime(2025, 06, 10),
                EndAt = new DateTime(2026, 01, 01),
                TotalSeats = 3

            };

            var resultEvent = _eventService.PostEvent(eventItem);


            var eventItem2 = new Event()
            {
                Id = 1,
                Title = "PutTitle",
                Description = "Description",
                StartAt = new DateTime(2026, 09, 11),
                EndAt = new DateTime(2026, 10, 11),
                TotalSeats = 2
            };

            var result = _eventService.PutEvent(1, eventItem2);

            Assert.True(result);
        }

        [Fact]
        public void DeleteEvent_ReturnCorrectResult()
        {
            var eventItem = new Event()
            {
                Id = 5,
                Title = "Foo",
                Description = "Bar",
                StartAt = new DateTime(2025, 06, 10),
                EndAt = new DateTime(2026, 01, 01),
                TotalSeats = 3

            };

            var resultEvent = _eventService.PostEvent(eventItem);

            var result = _eventService.DeleteEvent(eventItem.Id);

            Assert.True(result);
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void Filter_GetAllEvents_ReturnCorrectResult(GetEventsQuery filter, PageInfo pageInfo, int expected)
        {
            var eventItem = new Event()
            {
                Id = 1,
                Title = "Title1",
                Description = "Description",
                StartAt = new DateTime(2026, 09, 11),
                EndAt = new DateTime(2026, 10, 11),
                TotalSeats = 3
            };

            _eventService.PostEvent(eventItem);

            var eventItem2 = new Event()
            {
                Id = 2,
                Title = "Title2",
                Description = "Description",
                StartAt = new DateTime(2027, 09, 11),
                EndAt = new DateTime(2028, 09, 11),
                TotalSeats = 3
            };

            _eventService.PostEvent(eventItem2);


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
                TotalSeats =2
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
                EndAt = new DateTime(2026, 01, 01),
                TotalSeats =2
            };

            _eventService.PostEvent(eventItem);

            Assert.Equal("Событие с таким id уже существует", Assert.Throws<NotFoundException>(() => _eventService.PostEvent(eventItem)).Message);
        }

        [Fact]
        public void FaileValidDate() // Тестирую валидацию, а не PutEvent, тк в PutEvent не попадет не валлидированное значение
        {
            var id = 1;

            var eventItem = new CreateEvent()
            {
                Id = 1,
                Title = "PutTitle",
                Description = "Description",
                StartAt = new DateTime(2027, 09, 11),
                EndAt = new DateTime(2026, 10, 11),
                TotalSeats =2
            };

            var validContext = new ValidationContext(eventItem);
            var results = eventItem.Validate(validContext).ToList();

            Assert.Single(results);
            Assert.Equal("Дата окончания должна быть позднее даты начала", results[0].ErrorMessage);
        }

        [Fact]
        public async Task PutEvent_ReturnDataValidationError()
        {
            var eventItem = new Event()
            {
                Id = 1,
                Title = "PutTitle",
                Description = "Description",
                StartAt = new DateTime(2027, 09, 11),
                EndAt = new DateTime(2026, 10, 11),
                TotalSeats =2
            };

            using var factory = new WebApplicationFactory<Program>();

            using var client = factory.CreateClient();

            var response = await client.PutAsJsonAsync("events/1", eventItem);

            var content = await response.Content.ReadAsStringAsync();

            Assert.True(content.Contains("Дата окончания должна быть позднее даты начала"));
        }
    }
}
