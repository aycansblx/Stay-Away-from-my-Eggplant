using UnityEngine;

namespace Utils
{
    public static class ScalarUtils
    {
        public static float EaseInQuad(float start, float end, float current, float duration)
        {
            return (end - start) * (current / duration) * (current / duration) + start;
        }

        public static float EaseOutQuad(float start, float end, float current, float duration)
        {
            return (start - end) * (current / duration) * (current / duration - 2f) + start;
        }

        public static float EaseInSine(float start, float end, float current, float duration)
        {
            return (start - end) * Mathf.Cos((current / duration) * (Mathf.PI / 2f)) + start;
        }

        public static float EaseOutSine(float start, float end, float current, float duration)
        {
            return (end - start) * Mathf.Sin((current / duration) * (Mathf.PI / 2f)) + start;
        }

        public static float Lerp(float start, float end, float current, float duration, EasingType type)
        {
            if (current >= duration)
            {
                return end;
            }

            switch (type)
            {
                case EasingType.EaseInQuad:
                    return EaseInQuad(start, end, current, duration);
                case EasingType.EaseOutQuad:
                    return EaseOutQuad(start, end, current, duration);
                case EasingType.EaseInSine:
                    return EaseInSine(start, end, current, duration);
                case EasingType.EaseOutSine:
                    return EaseOutSine(start, end, current, duration);
            }

            return Mathf.Lerp(start, end, current / duration);
        }

        public enum EasingType { Linear, EaseInQuad, EaseOutQuad, EaseInSine, EaseOutSine }
    }
}
