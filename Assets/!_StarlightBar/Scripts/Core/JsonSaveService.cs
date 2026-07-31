using System;
using System.IO;
using UnityEngine;

namespace StarlightBar.Core
{
    /// <summary>
    /// 임시 파일과 백업 파일을 사용해 저장 손상 가능성을 줄이는 JSON 저장소입니다.
    /// </summary>
    public sealed class JsonSaveService : ISaveService
    {
        private readonly string savePath;
        private readonly string backupPath;
        private readonly string temporaryPath;

        public bool HasSave => File.Exists(savePath) || File.Exists(backupPath);

        /// <summary>
        /// 지정한 디렉터리에 원자적 교체와 백업 복구를 사용하는 저장 서비스를 만듭니다.
        /// </summary>
        public JsonSaveService(string directory, string fileName = "save_slot_01.json")
        {
            if (string.IsNullOrWhiteSpace(directory))
                throw new ArgumentException("저장 폴더 경로가 필요합니다.", nameof(directory));

            Directory.CreateDirectory(directory);
            savePath = Path.Combine(directory, fileName);
            backupPath = savePath + ".bak";
            temporaryPath = savePath + ".tmp";
        }

        /// <summary>
        /// 먼저 임시 파일을 완성한 후 기존 저장을 백업하고 교체합니다.
        /// </summary>
        public void Save(GameSaveData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            data.version = GameSaveData.CurrentVersion;
            File.WriteAllText(temporaryPath, JsonUtility.ToJson(data, true));

            if (File.Exists(savePath))
            {
                // 같은 볼륨의 File.Replace를 사용해 정상 저장과 백업 교체를 한 번에 완료한다.
                File.Replace(temporaryPath, savePath, backupPath, true);
            }
            else
            {
                File.Move(temporaryPath, savePath);
            }
        }

        /// <summary>
        /// 기본 저장이 손상되면 마지막 정상 백업을 자동으로 시도합니다.
        /// </summary>
        public bool TryLoad(out GameSaveData data)
        {
            if (TryRead(savePath, out data))
                return true;

            if (TryRead(backupPath, out data))
            {
                File.Copy(backupPath, savePath, true);
                return true;
            }

            data = null;
            return false;
        }

        /// <summary>
        /// 현재 저장 슬롯과 백업·임시 파일을 함께 삭제합니다.
        /// </summary>
        public void DeleteSave()
        {
            DeleteIfExists(savePath);
            DeleteIfExists(backupPath);
            DeleteIfExists(temporaryPath);
        }

        private static bool TryRead(string path, out GameSaveData data)
        {
            data = null;
            if (!File.Exists(path))
                return false;

            try
            {
                data = JsonUtility.FromJson<GameSaveData>(File.ReadAllText(path));
                return data != null && data.version > 0 && data.version <= GameSaveData.CurrentVersion;
            }
            catch (Exception exception) when (exception is IOException || exception is ArgumentException)
            {
                Debug.LogWarning($"저장 파일을 읽지 못했습니다: {path}\n{exception.Message}");
                return false;
            }
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
