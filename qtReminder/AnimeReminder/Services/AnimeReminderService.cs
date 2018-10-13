using System;

namespace qtReminder.AnimeReminder.Services
{
    public class AnimeReminderService
    {
        private readonly IServiceProvider service;
        
        public AnimeReminderService(IServiceProvider service)
        {
            this.service = service;
        }

        public void StartService()
        {
            AnimeReminderHandler.StartCheck(); // why is this even a service. God fucking damnit.
        }
    }
}