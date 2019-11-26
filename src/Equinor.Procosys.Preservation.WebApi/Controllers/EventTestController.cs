using System;
using Equinor.Procosys.Preservation.Command;
using Equinor.Procosys.Preservation.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Equinor.Procosys.Preservation.WebApi.Controllers
{
    [ApiController]
    [Route("EventTest")]
    public class EventTestController : ControllerBase
    {
        private readonly IEventBus _eventBus;
        private readonly ITimeService _timeService;

        public EventTestController(IEventBus eventBus, ITimeService timeService)
        {
            _eventBus = eventBus;
            _timeService = timeService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult PublishEvent()
        {
            _eventBus.Publish(new IntegrationEvent(Guid.NewGuid(), _timeService.GetCurrentTimeUTC()));
            return NoContent();
        }
    }
}
