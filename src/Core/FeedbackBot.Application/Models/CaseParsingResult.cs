﻿namespace FeedbackBot.Application.Models;

public record CaseParsingResult(ReadOnlyMemory<char> Segment, string Case);