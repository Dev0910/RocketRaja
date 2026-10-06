using RocketRaja.Core;

static partial class Program
{
    sealed class FakeCloud : ICloudSaveStore
    {
        public double Time = 1000;
        public CloudRecord Record;
        public TaskCompletionSource<bool> Hold;
        public bool LoseAck;
        event Action<CloudRecord> Changed;
        void Notify() => Changed?.Invoke(Clone(Record));
        public Task<CloudRecord> ReadAsync(string uid) => Task.FromResult(Clone(Record));
        public Task<CloudRecord> CreateAsync(AccountProfile p, SavePayload s)
        {
            if (Record != null) throw new CloudConflictException("exists");
            Record = new CloudRecord { profile = p, save = s, revision = 1, commitId = Guid.NewGuid().ToString("N"), createdAt = Time, updatedAt = Time, heartbeatAt = Time };
            return ReadAsync(p.uid);
        }
        public Task<CloudRecord> AcquireAsync(string uid, string owner)
        {
            if (Record.owner != owner && Record.owner != "" && Time - Record.heartbeatAt < 30) throw new CloudConflictException("lease active");
            if (Record.owner != owner) { Record.owner = owner; Record.generation++; Record.handoff = ""; }
            Record.heartbeatAt = Time; Notify(); return ReadAsync(uid);
        }
        public Task<CloudRecord> HeartbeatAsync(string uid, string owner, long generation)
        {
            if (!Record.OwnedBy(owner, generation) || Record.handoff != "") throw new CloudConflictException("lost owner");
            Record.heartbeatAt = Time; Notify(); return ReadAsync(uid);
        }
        public async Task<CloudRecord> CommitAsync(AccountCheckpoint c, bool release = false)
        {
            if (Hold != null) await Hold.Task;
            if (c.IsAcknowledged(Record)) return Clone(Record);
            if (!Record.OwnedBy(c.owner, c.generation) || c.baseRevision != Record.revision || Time - Record.heartbeatAt >= 30) throw new CloudConflictException("stale");
            Record.save = Clone(c.save); Record.revision++; Record.commitId = c.commitId; Record.updatedAt = Record.heartbeatAt = Time;
            if (release) Record.owner = "";
            Notify();
            if (LoseAck) { LoseAck = false; throw new IOException("response lost"); }
            return Clone(Record);
        }
        public Task RequestHandoffAsync(string uid, string target) { Record.handoff = target; Notify(); return Task.CompletedTask; }
        public Task ReleaseAsync(string uid, string owner, long generation)
        {
            if (!Record.OwnedBy(owner, generation)) throw new CloudConflictException("lost owner");
            Record.owner = ""; Notify(); return Task.CompletedTask;
        }
        public IDisposable Listen(string uid, Action<CloudRecord> changed, Action<Exception> failed)
        { Changed += changed; return new Subscription(() => Changed -= changed); }
        sealed class Subscription : IDisposable { readonly Action close; public Subscription(Action close) { this.close = close; } public void Dispose() => close(); }
        public void Dispose() { }
    }
}
