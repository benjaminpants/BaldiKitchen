using System;
using System.Collections.Generic;
using System.Text;

namespace BaldiKitchen
{
    public enum MicroGameState
    {
        Progress = 0,
        Won = 1,
        Lost = 2,
        HowDidYouFuckUpThisBadlyEndInstantlyDIE = 3
    }


    public abstract class KitchenTripMicrogame
    {
        public virtual KitchenRoom roomToUse => KitchenRoom.None;

        public KitchenFieldTripManager manager;

        public abstract void Start();

        public abstract void Update();

        public abstract void End();
    }
}
