using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace BaldiKitchen
{
    public abstract class KitchenFieldTripRoom : MonoBehaviour
    {
        protected Image background => gameObject.transform.Find("BG").GetComponent<Image>();
        public virtual KitchenRoom room => KitchenRoom.None;
        public virtual Sprite roomSprite => null;
        protected KitchenFieldTripManager manager;

        public virtual void Initialize(KitchenFieldTripManager fm)
        {
            manager = fm;
        }

        public abstract void BeginPlay();
        public abstract void StopPlay();
        public abstract void Reset();
    }
}
