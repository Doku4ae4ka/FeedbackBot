﻿using Microsoft.AspNetCore.Mvc;
using FeedbackBot.Application.Interfaces;
using Serilog;
using Telegram.Bot.Types;

namespace FeedbackBot.Host.Controllers;

[ApiController, Route("bot")]
public class WebhookController : ControllerBase
{
    [HttpPost("{token}")]
    public async Task<IActionResult> ReceiveUpdateAsync(
        [FromServices] IUpdateHandler updateHandler,
        [FromBody] Update update)
    {
        var cts = new CancellationTokenSource();
        await updateHandler.HandleAsync(update, cts.Token);
        cts.Token.ThrowIfCancellationRequested();
        
        return Ok();
    }
}
