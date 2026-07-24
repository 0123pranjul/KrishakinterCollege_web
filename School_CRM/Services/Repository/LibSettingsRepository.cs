using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Repository
{
    public class LibSettingsRepository : ILibSettingsRepository
    {
        private readonly LibmanagementContext _context;

        public LibSettingsRepository(LibmanagementContext context)
        {
            _context = context;
        }

        public async Task<string?> GetValueAsync(string key)
        {
            var setting = await _context.LibSettings
                .FirstOrDefaultAsync(x => x.SettingKey == key);
            return setting?.SettingValue;
        }

        public async Task SetValueAsync(string key, string value, int updatedBy)
        {
            var setting = await _context.LibSettings
                .FirstOrDefaultAsync(x => x.SettingKey == key);

            if (setting == null)
            {
                setting = new LibSetting
                {
                    SettingKey   = key,
                    SettingValue = value,
                    UpdatedBy    = updatedBy,
                    UpdatedAt    = DateTime.Now
                };
                _context.LibSettings.Add(setting);
            }
            else
            {
                setting.SettingValue = value;
                setting.UpdatedBy    = updatedBy;
                setting.UpdatedAt    = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }
    }
}
