using UnityEngine;

namespace Utils
{
    public static class VectorUtils
    {
        public static Vector3 EaseInQuad(Vector3 start, Vector3 end, float current, float duration)
        {
            return (end - start) * (current / duration) * (current / duration) + start;
        }

        public static Vector3 EaseOutQuad(Vector3 start, Vector3 end, float current, float duration)
        {
            return (start - end) * (current / duration) * (current / duration - 2f) + start;
        }

        public static Vector3 EaseInSine(Vector3 start, Vector3 end, float current, float duration)
        {
            return (start - end) * Mathf.Cos((current / duration) * (Mathf.PI / 2f)) + start;
        }

        public static Vector3 EaseOutSine(Vector3 start, Vector3 end, float current, float duration)
        {
            return (end - start) * Mathf.Sin((current / duration) * (Mathf.PI / 2f)) + start;
        }

        public static Vector3 Lerp(Vector3 start, Vector3 end, float current, float duration, EasingType type)
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

            return Vector3.Lerp(start, end, current / duration);
        }


        public enum EasingType { Linear, EaseInQuad, EaseOutQuad, EaseInSine, EaseOutSine }
    }
}
