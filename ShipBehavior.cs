using System;
using System.Collections.Generic;
using System.Diagnostics;
using TestingTaskFramework;
using VRageMath;

namespace TestingTask
{
    // TODO: Modify 'OnUpdate' method, find asteroids in World (property Ship.World) and shoot them.
    class ShipBehavior : IShipBehavior
    {
        /// <summary>
        /// The ship which has this behavior.
        /// </summary>
        public Ship Ship { get; set; }

        private WorldObject _lastTarget;
        private double _lastTrgTime;
        private TimeSpan _lastShootTime;

        private List<WorldObject> _mTmpList;


        /// <summary>
        /// Called when ship is being updated, Ship property is never null when OnUpdate is called.
        /// </summary>

        public void OnUpdate()
        {
            Vector3 direction;
            float projectileLifeTimeInSeconds = this.Ship.GunInfo.ProjectileLifetime.Milliseconds * 0.001f;

            if (_mTmpList == null)
                _mTmpList = new List<WorldObject>();
            else
                _mTmpList.Clear();

            //set a query params
            var radius = this.Ship.GunInfo.ProjectileSpeed * projectileLifeTimeInSeconds;
            Vector2 vector2 = new Vector2(radius, radius);
            Vector3 vector3 = new Vector3(vector2.X, 2f, vector2.Y);
            double time = 0f;
            Vector3 trgRelativeVelocity = Vector3.Zero;

            BoundingBox box;
            box.Min = this.Ship.Position - vector3;
            box.Max = this.Ship.Position + vector3;
            this.Ship.World.Query(box, this._mTmpList);
            int cnt = _mTmpList.Count;

            Asteroid nearest = null;
            //float distSquared = Single.PositiveInfinity;
            for (int i = 0; i < cnt; i++)
            {
                if (_mTmpList[i] is Asteroid)
                {
                    //relative position
                    Vector3 tmp = _mTmpList[i].Position - this.Ship.Position;
                    //float distance = tmp.LengthSquared();
                    //relative velocity
                    Vector3 trgRv = _mTmpList[i].LinearVelocity - this.Ship.LinearVelocity;
                    double tmpTime;

                    if (trgRv.Equals(Vector3.Zero))
                    {
                        //relative velocity is zero
                        tmpTime = tmp.Length() / this.Ship.GunInfo.ProjectileSpeed;
                    }
                    else
                    {
                        tmpTime = InterceptionUtils.GetInterceptTime(this.Ship.GunInfo.ProjectileSpeed, tmp, trgRv);
                    }
                    //target must be in projectile range
                    if (tmpTime > 0f && tmpTime <= projectileLifeTimeInSeconds)
                    {
                        if (nearest == null)
                        {//first target in list
                            //distSquared = distance;
                            nearest = (Asteroid) _mTmpList[i];
                            time = tmpTime;
                            trgRelativeVelocity = trgRv;
                        }
                        else
                        {
                            if (time > tmpTime || (_mTmpList[i].NeedsUpdate && !nearest.NeedsUpdate))
                            {//new target is better
                                //distSquared = distance;
                                nearest = (Asteroid) _mTmpList[i];
                                time = tmpTime;
                                trgRelativeVelocity = trgRv;
                            }
                        }
                    }
                }
            }

            if (nearest != null && (!nearest.Equals(_lastTarget) || (_lastTarget.World != null && Ship.World.Time.TotalSeconds > _lastTrgTime + _lastShootTime.TotalSeconds)))
            { //shoot only once at every target
				direction = nearest.Position + trgRelativeVelocity * (float)time - this.Ship.Position;

				if (this.Ship.CanShoot) 
					{
                        _lastTarget = nearest; //remember shooted target
                        _lastTrgTime = time;
                    }

				Ship.Shoot(Vector3.Normalize(direction));
                _lastShootTime = Ship.World.Time;
            }
        }
    }
}
