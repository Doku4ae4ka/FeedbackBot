﻿using FeedbackBot.Application.Models.Contexts;

namespace FeedbackBot.Application.Delegates;

public delegate Task CallbackQueryContextHandler(CallbackQueryContext context);