using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RocketRaja.Core
{
    public sealed class SaveStore
    {
        readonly string file;
        readonly Func<GameState, string> encode;
        readonly Func<string, GameState> decode;
        public string Warning { get; private set; }
        public SaveStore(string path, Func<GameState, string> serialize, Func<string, GameState> deserialize)
        { file = path; encode = serialize; decode = deserialize; }
        static string Hash(string json)
        { using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(json))).Replace("-", ""); }
        GameState Read(string path)
        {
            string raw = File.ReadAllText(path); int newline = raw.IndexOf('\n');
            if (newline <= 0) throw new InvalidDataException("Missing save checksum");
            string json = raw.Substring(newline + 1);
            if (raw.Substring(0, newline).Trim() != Hash(json)) throw new InvalidDataException("Save checksum mismatch");
            GameState state = decode(json);
            if (state == null || !state.Valid()) throw new InvalidDataException("Save data is invalid or uses an unsupported version");
            return state;
        }
        public GameState Load()
        {
            Warning = null;
            if (!File.Exists(file) && !File.Exists(file + ".bak")) return null;
            try { return Read(file); }
            catch (Exception)
            {
                try { var backup = Read(file + ".bak"); Warning = "Your last backup was recovered."; return backup; }
                catch { Warning = "Your save could not be read. The original files were preserved."; return null; }
            }
        }
        public void Save(GameState state)
        {
            if (!state.Valid()) throw new InvalidDataException("Refusing to save invalid state");
            Directory.CreateDirectory(Path.GetDirectoryName(file));
            string json = encode(state); string raw = Hash(json) + "\n" + json;
            string temp = file + ".tmp";
            using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
            { byte[] bytes = Encoding.UTF8.GetBytes(raw); stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
            // Keep only validated backups; a corrupt main must never overwrite a good backup.
            if (File.Exists(file))
            {
                try { Read(file); File.Copy(file, file + ".bak", true); }
                catch { File.Copy(file, file + ".damaged", true); }
                File.Replace(temp, file, null);
            }
            else File.Move(temp, file);
        }
    }
}
