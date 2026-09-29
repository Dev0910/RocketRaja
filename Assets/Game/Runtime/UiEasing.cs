using UnityEngine;
namespace RocketRaja
{
    public static class UiEasing
    {
        public static float Bezier(float time, float x1, float y1, float x2, float y2)
        {
            if (time <= 0) return 0; if (time >= 1) return 1;
            float low = 0, high = 1, u = time;
            for (int i = 0; i < 16; i++)
            {
                u = (low + high) * .5f; float v = 1 - u;
                float x = 3 * v * v * u * x1 + 3 * v * u * u * x2 + u * u * u;
                if (x < time) low = u; else high = u;
            }
            float r = 1 - u;
            return 3 * r * r * u * y1 + 3 * r * u * u * y2 + u * u * u;
        }
        public static float Standard(float t) => Bezier(t, .2f, .75f, .25f, 1);
        public static float EaseOut(float t) => Bezier(t, 0, 0, .58f, 1);
        public static float EaseInOut(float t) => Bezier(t, .42f, 0, .58f, 1);
        public static float Pixels(Transform target)
        {
            var canvas = target.GetComponentInParent<Canvas>();
            return canvas == null ? 1 : 1 / Mathf.Max(.001f, canvas.rootCanvas.scaleFactor);
        }
    }
}
