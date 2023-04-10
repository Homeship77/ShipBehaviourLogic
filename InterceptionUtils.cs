using System;
using VRageMath;

namespace TestingTask
{
    public static class InterceptionUtils
    {
        public static double GetInterceptTime(float projectileSpeed, Vector3 trgRelPos, Vector3 trgRelVel)
        {
            float velocitySquared = trgRelVel.LengthSquared();
            if (velocitySquared < 0.0001f)
                return 0f;

            float a = velocitySquared - projectileSpeed * projectileSpeed;

            //handle similar velocities
            if (Math.Abs(a) < 0.001f)
            {
                float tmp = trgRelVel.Dot(trgRelPos);
                float t = -trgRelPos.LengthSquared() / (2f * tmp);
                return Math.Max(t, 0f); //don't shoot, time<0
            }

            float b = 2f * trgRelVel.Dot(trgRelPos);
            float c = trgRelPos.LengthSquared();
            float d = b * b - 4f * a * c; //calculate descriminant

            if (d > 0f)
            {
                //two intercept paths
                double t1 = (-b + Math.Sqrt(d)) / (2f * a);
                double t2 = (-b - Math.Sqrt(d)) / (2f * a);
                if (t1 > 0f)
                {
                    if (t2 > 0f)
                        return Math.Min(t1, t2); //both are positive, returns nearest
                    else
                        return t1; //only t1 is positive
                }
                else
                    return Math.Max(t2, 0f); //don't shoot, time<0
            }
            else if (d < 0f) //no intercept path
                return 0f;
            else //one intercept path
                return Math.Max(-b / (2f * a), 0f); //don't shoot, time<0
        }
    }
}
