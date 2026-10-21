using System;
using System.IO;

namespace RocketRaja.Core
{
    /// <summary>Foreground time and command gate shared by the runtime and lifecycle tests.</summary>
    public sealed class RedesignLifecycle
    {
        public RedesignEngine Engine { get; }
        public bool Suspended { get; private set; }
        public bool GameplayInputEnabled { get; set; } = true;
        public string SaveFailure { get; private set; }
        // Local diagnostics retain the underlying I/O reason; player-facing copy stays concise.
        public Exception LastSaveException { get; private set; }
        public Func<bool> OwnsSession { get; set; } = () => true;
        readonly Func<bool> enginePermission;
        public bool CanMutate => !Suspended && SaveFailure == null && OwnsSession() && enginePermission();
        double saveElapsed;

        public RedesignLifecycle(RedesignEngine engine)
        {
            Engine = engine ?? throw new ArgumentNullException(nameof(engine));
            enginePermission = Engine.MutationAllowed;
            Engine.MutationAllowed = () => CanMutate;
        }
        // Overlays disable the gameplay hit surface; they never suspend production or skills.
        public bool TapGameplaySurface() => GameplayInputEnabled && Run(e => e.Tap());
        public bool Run(Func<RedesignEngine, bool> command)
        {
            if (!CanMutate) return false;
            try { return command(Engine); }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is InvalidDataException)
            {
                LastSaveException = error;
                SaveFailure = "Saving failed. Free device storage and retry before continuing.";
                return false;
            }
        }
        public void Tick(double unscaledSeconds)
        {
            if (!CanMutate) return;
            if (double.IsNaN(unscaledSeconds) || double.IsInfinity(unscaledSeconds) || unscaledSeconds < 0)
                throw new ArgumentOutOfRangeException(nameof(unscaledSeconds));
            Engine.Advance(unscaledSeconds);
            saveElapsed += unscaledSeconds;
            if (saveElapsed >= 10 && Run(e => e.Save())) saveElapsed = 0;
        }
        public bool Suspend()
        {
            if (Suspended) return SaveFailure == null;
            bool saved = Run(e => e.Save());
            Suspended = true;
            return saved;
        }
        public bool Resume(double utc)
        {
            if (!Suspended) return CanMutate;
            Suspended = false;
            // A failed departure save is retried before any offline settlement or commands.
            if (SaveFailure != null && !RetrySave()) { Suspended = true; return false; }
            if (!OwnsSession()) { Suspended = true; return false; }
            Run(e => e.ResolveOffline(utc));
            saveElapsed = 0;
            if (SaveFailure != null) Suspended = true;
            return CanMutate;
        }
        public bool RetrySave()
        {
            if (Suspended || !OwnsSession()) return false;
            SaveFailure = null;
            LastSaveException = null;
            return Run(e => e.Save());
        }
    }
}
