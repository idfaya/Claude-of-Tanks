using System;

namespace ClaudeOfTanks.Runtime
{
    public enum AdaptiveQualityAction
    {
        None,
        TrimDown,
        ResolutionDown,
        TierDown,
        ResolutionUp,
        TrimUp,
        TierUp
    }

    public readonly struct AdaptiveQualityWindow
    {
        public AdaptiveQualityWindow(
            float clockS,
            float frameEmaMs,
            float frameBudgetMs,
            float missedFrameRatio)
        {
            ClockS = clockS;
            FrameEmaMs = frameEmaMs;
            FrameBudgetMs = frameBudgetMs;
            MissedFrameRatio = missedFrameRatio;
        }

        public float ClockS { get; }
        public float FrameEmaMs { get; }
        public float FrameBudgetMs { get; }
        public float MissedFrameRatio { get; }
    }

    public sealed class AdaptiveQualityPolicy
    {
        public const int MaximumTrim = 2;
        public const float ResolutionStep = 0.1f;
        private const float WarmupS = 6f;
        private const float TierRecoveryS = 30f;
        private const int ReliefStrikes = 2;
        private const int TierReliefStrikes = 3;
        private const int RecoveryStrikes = 8;

        private int _overloadStrikes;
        private int _tierStrikes;
        private int _recoveryStrikes;
        private float _resetAt;

        public AdaptiveQualityPolicy(int requestedQuality)
        {
            Reset(requestedQuality, 0f);
        }

        public int RequestedQuality { get; private set; }
        public int EffectiveQuality { get; private set; }
        public int PerformanceTrim { get; private set; }
        public float RenderScale { get; private set; }

        public void Reset(int requestedQuality, float clockS)
        {
            RequestedQuality = Math.Max(0, requestedQuality);
            EffectiveQuality = RequestedQuality;
            PerformanceTrim = 0;
            RenderScale = 1f;
            _overloadStrikes = 0;
            _tierStrikes = 0;
            _recoveryStrikes = 0;
            _resetAt = clockS;
        }

        public AdaptiveQualityAction Evaluate(
            AdaptiveQualityWindow window)
        {
            if (window.FrameBudgetMs <= 0f ||
                window.ClockS - _resetAt < WarmupS)
            {
                return AdaptiveQualityAction.None;
            }

            bool overloaded =
                window.FrameEmaMs > window.FrameBudgetMs * 1.12f &&
                window.MissedFrameRatio > 0.12f;
            bool clean =
                window.FrameEmaMs < window.FrameBudgetMs * 0.9f &&
                window.MissedFrameRatio < 0.04f;
            if (overloaded)
                return ApplyRelief();
            _overloadStrikes = 0;
            _tierStrikes = 0;
            if (!clean)
            {
                _recoveryStrikes = 0;
                return AdaptiveQualityAction.None;
            }
            return ApplyRecovery(window.ClockS);
        }

        private AdaptiveQualityAction ApplyRelief()
        {
            _recoveryStrikes = 0;
            _overloadStrikes++;
            if (_overloadStrikes < ReliefStrikes)
                return AdaptiveQualityAction.None;
            _overloadStrikes = 0;

            if (PerformanceTrim < MaximumTrim)
            {
                PerformanceTrim++;
                return AdaptiveQualityAction.TrimDown;
            }

            float floor = ResolutionFloor(RequestedQuality);
            if (RenderScale > floor + 0.001f)
            {
                RenderScale = Math.Max(
                    floor,
                    RenderScale - ResolutionStep);
                return AdaptiveQualityAction.ResolutionDown;
            }

            _tierStrikes++;
            if (_tierStrikes < TierReliefStrikes ||
                EffectiveQuality == 0)
            {
                return AdaptiveQualityAction.None;
            }
            _tierStrikes = 0;
            EffectiveQuality--;
            return AdaptiveQualityAction.TierDown;
        }

        private AdaptiveQualityAction ApplyRecovery(float clockS)
        {
            _recoveryStrikes++;
            if (_recoveryStrikes < RecoveryStrikes)
                return AdaptiveQualityAction.None;
            _recoveryStrikes = 0;

            if (RenderScale < 0.999f)
            {
                RenderScale = Math.Min(
                    1f,
                    RenderScale + ResolutionStep);
                return AdaptiveQualityAction.ResolutionUp;
            }
            if (PerformanceTrim > 0)
            {
                PerformanceTrim--;
                return AdaptiveQualityAction.TrimUp;
            }
            if (EffectiveQuality < RequestedQuality &&
                clockS - _resetAt >= TierRecoveryS)
            {
                EffectiveQuality++;
                return AdaptiveQualityAction.TierUp;
            }
            return AdaptiveQualityAction.None;
        }

        private static float ResolutionFloor(int quality)
        {
            if (quality >= 4) return 0.8f;
            if (quality >= 2) return 0.9f;
            return 1f;
        }
    }
}
