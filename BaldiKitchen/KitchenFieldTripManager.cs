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
using static BaldiKitchen.KitchenOven;
using Image = UnityEngine.UI.Image;

namespace BaldiKitchen
{
    public enum KitchenState
    {
        Intro = 0,
        MiniIntro = 1,
        Active = 2
    }

    public enum KitchenRoom
    {
        None = 0,
        Oven = 1,
        Main = 2, //place with cupboards and stuff
        Bowl = 3,
        Turkey = 4,
        Table = 5 //dining room table
    }

    public abstract class KitchenFieldTripRoom : MonoBehaviour
    {
        protected Image background => gameObject.transform.Find("BG").GetComponent<Image>();
        public virtual KitchenRoom room => KitchenRoom.None;
        public virtual Sprite roomSprite => null;
        protected KitchenFieldTripManager manager;

        public abstract void PrepareMiniIntro(ref SoundObject[] objs);

        public virtual void Initialize(KitchenFieldTripManager fm)
        {
            manager = fm;
        }

        public abstract void BeginPlay();
        public abstract void StopPlay();
        public abstract void Reset();
    }

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
            TemperatureText.text = String.Format("{0}°",temperature.ToString());
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
            TemperatureText.rectTransform.offsetMin = new Vector3(-37f,-102f);
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
            SetTemperature(UnityEngine.Random.Range(10,31) * 10);
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


    public class KitchenFieldTripManager : FieldTripManager
    {
        public AudioManager myMan;
        public Sprite OvenBG = BaldiKitchenPlugin.OvenBG;
        public Image Background;
        private int score = 0;
        private KitchenState state = KitchenState.Intro;
        private KitchenRoom room;
        private Dictionary<KitchenRoom, KitchenFieldTripRoom> rooms = new Dictionary<KitchenRoom, KitchenFieldTripRoom>();
        private KitchenFieldTripRoom currentRoom => rooms[room];

        public void SwitchToRoom(KitchenRoom toSwitch)
        {
            if (rooms.ContainsKey(room))
            {
                rooms[room].gameObject.SetActive(false);
            }
            room = toSwitch;
            rooms[room].gameObject.SetActive(true);
            currentRoom.Reset(); //so the room looks the same every time
        }

        public TMP_Text GetScoreText()
        {
            return this.scoreText;
        }

        // PLACEHOLDER
        public void PlaySound(SoundObject file)
        {
            myMan.PlaySingle(file);
        }

        public T InitializeRoom<T>() where T : KitchenFieldTripRoom
        {
            GameObject root = new GameObject();
            root.gameObject.layer = LayerMask.NameToLayer("UI");
            RectTransform rect = root.AddComponent<RectTransform>();
            rect.offsetMax = new Vector2(240, 180);
            rect.offsetMin = new Vector2(-240, -180);
            root.transform.SetParent(transform);
            Image bg = GameObject.Instantiate<Image>(Background, root.transform);
            bg.name = "BG";
            root.gameObject.SetActive(false);
            T room = root.gameObject.AddComponent<T>();
            root.gameObject.name = room.room + "Room";
            bg.sprite = room.roomSprite;
            room.Initialize(this);
            rooms.Add(room.room, room);
            room.transform.SetAsFirstSibling();
            root.transform.localScale = Vector3.one; //what the fuck
            return room;
        }

        public override void Initialize(BaseGameManager bgm)
        {
            base.Initialize(bgm);
            SwitchToRoom(KitchenRoom.Oven);
            audIntro = new SoundObject[]
            {
                BaldiKitchenPlugin.baldiDialogue["kitchen_intro.wav"],
                BaldiKitchenPlugin.baldiDialogue["kitchen_today_thanksgiving.wav"],
                BaldiKitchenPlugin.baldiDialogue["kitchen_end.wav"],
            };
            UpdateScore();
            scoreText.color = Color.black;
            this.rankDisplay.SetRank(3);
            base.StartCoroutine(PlayRandomBaldiAnimationsTilTalkingsDone());
            base.StartCoroutine(base.IntroDelay());
        }

        public override void IntroFinished()
        {
            base.IntroFinished();
            switch (state)
            {
                case KitchenState.Intro:
                    state = KitchenState.MiniIntro; // time to perform this rooms mini intro! (todo: this will probably be moved elsewhere)
                    currentRoom.PrepareMiniIntro(ref audIntro);
                    base.StartCoroutine(PlayRandomBaldiAnimationsTilTalkingsDone());
                    base.StartCoroutine(base.IntroDelay());
                    break;
                case KitchenState.MiniIntro:
                    state = KitchenState.Active;
                    LowerBaldi();
                    currentRoom.BeginPlay();
                    break;
            }
        }

        public IEnumerator PlayRandomBaldiAnimationsTilTalkingsDone()
        {
            while (baldiMan.filesQueued == 0) yield return null;
            while (baldiMan.IsPlaying)
            {
                RandomBaldiAnimation();
                yield return new WaitForSeconds(1.75f);
            }
            yield break;
        }

        private void UpdateScore()
        {
            this.scoreText.text = "Score: " + this.score.ToString();
        }
    }
}
