# Event Manager

Проект содержит 3 микросервиса: UserService, EventService, BookingService. Для связи между EventService и BookingService используется Kafka. Для кэширования используется Redis

> UserService - предназначен для регистрации пользователя и входа. Он предоставляет jwt токен для авторизации в других микросервисах


> EventService - является REST API предназначенный для работы с событиями


> BookingService - предназначен для работы бронирования пользователями мест на событиях

> BookingService публикует сообщение, в формате:  {"BookingId":"a74bc171-9c09-4b6b-b03b-8abd90892d3a","EventId":1,"UserId":"2dd0d4df-e0da-45c0-a60d-71ab4843bbe2","SeatsCount":1,"ConfirmedAt":"2026-08-29T18:46:05.7785601Z"} в топик booking-confirmed. После получения сообщения EventService проводит ряд проверок брони и если все корректно уменьшает количество мест у события

## Возможности

|Тип запроса| URL | Описание | JSON | 
| --- | --- | --- | --- |
| GET | /events | Получение всех событий| | 
| GET | /events/{id}| Получение события по id|  | 
| POST | /events | Добавить новое событие| `{"id": 0,"title": "string","description": "string","startAt": "2026-06-25T18:42:09.455Z","endAt": "2026-06-25T18:42:09.455Z","totalSeats": 2147483647}` | 
| PUT | /events/{id} | Изменение данных события| `{"id": 0,"title": "string","description": "string","startAt": "2026-06-25T18:46:28.719Z","endAt": "2026-06-25T18:46:28.719Z","totalSeats": 2147483647}` |
| DELETE | /events/{id} | Удалить событие|  |
| POST | events/{id}/book | Создает брониронь на событие по id события | |
| GET | /events/top | Получить топ 10 событий | |
| GET | bookings/{id} | Получает информацию о брони по ее id | |
| POST | auth/login | Позволяет получить jwt токен для зарегестрированного пользователя |`{"login": "string", "password": "string"}` |
| POST | auth/register | Регестрирует пользователя | `{"login": "string", "password": "string", "roles": 0}` |


## Фильтрация данных
Для GET /events можно задать в запросе параметры фильтрации /events?title=ti&from=2027-01-01&to=2029-01-01&page=1&pagesize=1
| Параметр запроса | Описание | Значение по умолчанию |
| --- | --- | --- |
|title| Поиск события по заголовку | - |
|from | Дата начала события | События рассматриваются с 01.01.2020 до 12.31.2030 |
|to | Дата окончания события | События рассматриваются с 01.01.2020 до 12.31.2030 |
|page| Страница для которой запрашиваются события| 1 |
|pageSize| Сколько событий отображается на странице | 10 |

## Redis
Кэширует события по id и кэширует топ 10 событий

| CacheKey | Описание |
| --- | --- |
| event:{id} | Кеширует событие по id. При бронировании использует стратегию инвалидации|
| events:top10 | Кэширует топ 10 событий. С TTL 5 минут |

## Ответы в случае ошибок

| StatusCose | Описание |
| --- | --- |
| 400 | Ошибка валлидации данных |
| 404 | Не найдены нужные id при выполнении запросов |
| 409 | Нет мест на событии | 
| 500 | Внутренняя ошибка на микросервисе |

## Работа с PostgreSQL
Для запуска PostgreSQL необходимо запустить докер образ командой docker compose up -d 
После выполнения команды PostgreSQL поднимутся 3 базы данных будет доступеные по следующим адресам:
| Микросервис | Адресс |
| --- | --- |
| UserService | localhost:54320 |
| EventService | localhost:5433 |
| BookingService | localhost:5434 |

В appsettings.json необходимо прописать параметры подключения

| --- | --- |
| Host | localhost |
| Port | 5433 |
| Database | eventapi |
|Username|postgres|
|Password|postgres|

Схема БД создаётся через миграции. Для создании новой миграции необходимо использовать
```
Add-Migration InitialCreate -Project User.Infrastructure -StartupProject User.Api
Add-Migration InitialCreate -Project Event.Infrastructure -StartupProject Event.Api
Add-Migration InitialCreate -Project Booking.Infrastructure -StartupProject Booking.Api
```
Последняя созданная миграция автоматически применяется при запуске приложения. 
Интеграционные тесты используют Testcontainers. Для их запуска необходимо запустить Docker образ. Для запуска образа используйте
```
docker run
```

Проект разделен на слои:
- Domain — доменные сущности, доменные исключения. 
- Application — сервисы, интерфейсы репозиториев, DTO. 
- Infrastructure —  репозитории, DbContext, внешние клиенты. 
- Presentation — контроллеры/Minimal API эндпоинты, HTTP-маппинг, регистрация зависимостей.

## Наблюдение
В проект добавлены докер контейнеры Grafana и Prometheus. Они позволяют следить за состоянием микросервисов.
Для запуска необходимо выполнить docker compose up

Обращение к Prometheus выполняется по адресу: http://localhost:9090
Для получения метрик:
http://localhost:5075/metrics
http://localhost:5224/metrics
http://localhost:5232/metrics

Для обращения к Grafana: http://localhost:3000

---

> Для компиляции проекта используйте: dotnet build
> Для запуска проекта используйте: dotnet run
> Для запуска тестов: dotnet test
> Обратитесь к документации UserService по пути https://localhost:7165/swagger/index.html
> Обратитесь к документации EventService по пути https://localhost:7098/swagger/index.html
> Обратитесь к документации BookingService по пути https://localhost:7187/swagger/index.html

