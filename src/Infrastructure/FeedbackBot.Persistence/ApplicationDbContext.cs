﻿using Microsoft.EntityFrameworkCore;
using FeedbackBot.Domain.Models.Entities;

namespace FeedbackBot.Persistence;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Chat> Chats { get; set; } = null!;
    public DbSet<State> States { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;

    public DbSet<EmailResponse> EmailResponses { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
}