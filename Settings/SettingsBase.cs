using System;

namespace qtReminder.Settings
{
    public abstract class SettingsBase<T>
    {
        public readonly string filename;
        
        public SettingsBase(string filename)
        {
            this.filename = filename;
        }
        
        public T Save()
        {
            try
            {
            }
            catch(Exception) {}
            
            throw new NotImplementedException();
        }
    }
}