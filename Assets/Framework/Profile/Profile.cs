using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Framework.Profile.Internal
{
    public class Profile<T> : IProfile<T> where T : class, new()
    {
        private bool _loaded = false;
        private T _instance = null;
        private readonly UTF8Encoding _utf8Encoding = new UTF8Encoding();

        public T Instance => GetInstance();

        public void Save() => SaveToFile();

        private T GetInstance()
        {
            LoadOnce();
            return _instance;
        }

        private void LoadOnce()
        {
            if (_loaded)
            {
                return;
            }
            _loaded = true;
            LoadFromFile();
            if (_instance == null)
            {
                _instance = new T();
            }
        }

        private void LoadFromFile()
        {
            string filePath = GetFilePath();
            if (!File.Exists(filePath))
            {
                return;
            }

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, (int)stream.Length);
                    _instance = JsonUtility.FromJson<T>(_utf8Encoding.GetString(bytes));
                    stream.Close();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void SaveToFile()
        {
            string filePath = GetFilePath();
            CheckFolderExists(filePath);
            DoSaveFile(filePath);
        }

        private void CheckFolderExists(string filePath)
        {
            string folderPath = Path.GetDirectoryName(filePath);
            if (folderPath != null)
            {
                DirectoryInfo folder = new DirectoryInfo(folderPath);
                if (!folder.Exists)
                {
                    folder.Create();
                }
            }
        }

        private void DoSaveFile(string filePath)
        {
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    byte[] bytes = _utf8Encoding.GetBytes(JsonUtility.ToJson(_instance));
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                    stream.Close();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public string GetFilePath()
        {
            string fileName = Path.ChangeExtension(Path.Combine("Profile", typeof(T).Name), "dat");

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            return Path.Combine(Environment.CurrentDirectory, fileName);
#else
            return Path.Combine(Application.persistentDataPath, fileName);
#endif
        }
    }
}
