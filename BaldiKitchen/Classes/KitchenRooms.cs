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
    public class KitchenOven : KitchenFieldTripRoom
    {
        public override KitchenRoom room => KitchenRoom.Oven;
        public override Sprite roomSprite => BaldiKitchenPlugin.OvenBG;

        public Knob[] knobs = new Knob[2];

        public int temperature;

        public SoundObject windSound;

        public TMP_Text TemperatureText; //todo: make the oven explode and you lose the minigame instantly if you crank the temp up to 1000

        public void SetTemperature(int toSet)
        {
            temperature = toSet;
            TemperatureText.text = String.Format("{0}°", temperature.ToString());
        }

        public void AddTemperature(int toAdd)
        {
            SetTemperature(temperature + toAdd);
        }

        public override void Initialize(KitchenFieldTripManager fm)
        {
            base.Initialize(fm);
            windSound = BaldiKitchenPlugin.SoundObjects.First(x => x.name == "ClockWind");
            knobs[0] = CreateKnob(new Vector3(155f, 230f), () =>
            {
                manager.PlaySound(windSound);
                AddTemperature(-10);
            }, 15f);
            knobs[1] = CreateKnob(new Vector3(318f, 230f), () =>
            {
                manager.PlaySound(windSound);
                AddTemperature(10);
            }, -15f);
            TemperatureText = GameObject.Instantiate<TMP_Text>(manager.GetScoreText(), transform);
            TemperatureText.name = "TempText";
            TemperatureText.text = "NaN";
            TemperatureText.color = Color.red;
            TemperatureText.alignment = TextAlignmentOptions.Midline;
            TemperatureText.rectTransform.offsetMin = new Vector3(-37f, -102f);
            TemperatureText.rectTransform.offsetMax = new Vector3(30f, -2f);
            //TemperatureText.transform.localPosition = new Vector3(-37f, -2f);
        }

        public Knob CreateKnob(Vector3 pos, UnityAction onPress, float toRotate)
        {
            Image img = BaldiKitchenPlugin.CreateImage(BaldiKitchenPlugin.Knob, transform, pos);
            img.name = "Knob";
            Knob knob = img.gameObject.AddComponent<Knob>();
            knob.Initialize(toRotate);
            knob.tag = "Button";
            knob.onSpin.AddListener(onPress);
            return knob;
        }

        public override void BeginPlay()
        {
            for (int i = 0; i < knobs.Length; i++)
            {
                knobs[i].SetButtonActive(true);
            }
        }

        public override void StopPlay()
        {
            for (int i = 0; i < knobs.Length; i++)
            {
                knobs[i].SetButtonActive(false);
            }
        }

        public override void Reset()
        {
            StopPlay();
            SetTemperature(UnityEngine.Random.Range(10, 31) * 10);
            for (int i = 0; i < knobs.Length; i++)
            {
                knobs[i].transform.eulerAngles = Vector3.zero;
            }
        }

        public override void PrepareMiniIntro(ref SoundObject[] objs)
        {
            objs = new SoundObject[]
            {
                BaldiKitchenPlugin.baldiDialogue["oven_preheat_1.wav"],
                BaldiKitchenPlugin.baldiDialogue["food_turkey.wav"],
                BaldiKitchenPlugin.baldiDialogue["oven_preheat_2.wav"],
            };
        }

        public class Knob : MonoBehaviour
        {
            public StandardMenuButton button;

            public UnityEvent onSpin = new UnityEvent();

            private float spinAmount;

            public void Initialize(float spin)
            {
                button = gameObject.AddComponent<StandardMenuButton>();
                button.swapOnHigh = false;
                button.swapOnHold = false;
                button.image = gameObject.GetComponent<Image>();
                button.InitializeAllEvents();
                spinAmount = spin;
                onSpin.AddListener(() =>
                {
                    transform.Rotate(new Vector3(0f, 0f, spinAmount));
                });
                SetButtonActive(false);
            }

            public void SetButtonActive(bool active)
            {
                button.enabled = active;
                if (active)
                {
                    button.OnPress = onSpin;
                }
                else
                {
                    button.OnPress = new UnityEvent();
                }
            }
        }
    }
}
