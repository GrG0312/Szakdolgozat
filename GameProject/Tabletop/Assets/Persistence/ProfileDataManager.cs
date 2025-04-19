using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Model;

namespace Persistence
{
    public class ProfileDataManager : IDataManager<Profile>
    {
        private string persistentDataPath;
        private BinaryFormatter formatter;
        public ProfileDataManager(string persistentDataPath)
        {
            this.persistentDataPath = persistentDataPath;
            this.formatter = new BinaryFormatter();
        }
        public void Save(Profile data)
        {
            FileStream fileStream = new FileStream(persistentDataPath + "/profile.tbt", FileMode.Create);
            formatter.Serialize(fileStream, data);
            fileStream.Close();
        }

        public Profile Load()
        {
            try
            {
                if (File.Exists(persistentDataPath + "/profile.tbt"))
                {
                    FileStream fileStream = new(persistentDataPath + "/profile.tbt", FileMode.Open);
                    Profile loaded = formatter.Deserialize(fileStream) as Profile;
                    fileStream.Close();
                    return loaded;
                } else
                {
                    return Profile.Default;
                }
            }
            catch
            {
                return Profile.Default;
            }
        }
    }
}
