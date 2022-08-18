using Blog.Application.Interfaces;
using System;

namespace Blog.Infrastructure.Services
{
    public class DateTimeService : IDateTime
    {
        public DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }
    }
}
