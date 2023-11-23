using HarmonyLib;
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
        Active = 2,
        Waiting = 3
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
        private int lives = 3;
        public float timer = 0f;
        public SoundObject successSound;
        public SoundObject failSound;
        public Action timerAction = null;
        // all the potential microgames
        public WeightedMicroGame[] potentialMicroGames = new WeightedMicroGame[]
        {
            new WeightedMicroGame(new PreheatOvenMicrogame("turkey", 300), 100),
            new WeightedMicroGame(new PreheatOvenMicrogame("stuffing", 200), 100),
            new WeightedMicroGame(new PreheatOvenMicrogame("pie", 400), 100)
        };
        public List<KitchenTripMicrogame> selectedGames = new List<KitchenTripMicrogame>();
        public int selectedGameIndex = 0;
        public KitchenTripMicrogame currentGame => selectedGames[selectedGameIndex];

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

        public void SetTimer(float t, Action whenDone)
        {
            timer = t;
            timerAction = whenDone;
        }

        public void StopTimer()
        {
            timer = 0f;
            timerAction = null;
            UpdateScore();
        }

        IEnumerator PostGameDelay()
        {
            yield return new WaitForSeconds(2f);
            selectedGameIndex++;
            if ((selectedGameIndex >= selectedGames.Count) || (lives == 0))
            {
                End(lives);
                yield break;
            }
            BeginCurrentGame();
            yield break;
            
        }

        public override void End(int rank)
        {
            base.End(rank);
            Debug.Log("Ended!");
        }

        public void Update()
        {
            if (timerAction != null)
            {
                timer -= Time.deltaTime;
                this.UpdateScore();
                if (timer <= 0f)
                {
                    timerAction();
                    timerAction = null;
                }
            }
            if (state == KitchenState.Active)
            {
                switch (currentGame.Update())
                {
                    case MicroGameState.Progress:
                        break;
                    case MicroGameState.Won:
                        currentRoom.StopPlay();
                        state = KitchenState.Waiting;
                        StopTimer();
                        PlaySound(successSound);
                        currentGame.End(MicroGameState.Won);
                        StartCoroutine(PostGameDelay());
                        break;
                    case MicroGameState.Lost:
                        currentRoom.StopPlay();
                        state = KitchenState.Waiting;
                        StopTimer();
                        PlaySound(failSound);
                        currentGame.End(MicroGameState.Lost);
                        lives--;
                        rankDisplay.SetRank(lives);
                        StartCoroutine(PostGameDelay());
                        break;
                }
            }
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
            audIntro = new SoundObject[]
            {
                BaldiKitchenPlugin.baldiDialogue["kitchen_intro.wav"],
                BaldiKitchenPlugin.baldiDialogue["kitchen_today_thanksgiving.wav"],
                BaldiKitchenPlugin.baldiDialogue["kitchen_end.wav"],
            };
            UpdateScore();
            scoreText.color = Color.black;
            rankDisplay.SetRank(lives);
            selectedGames.Add(WeightedMicroGame.RandomSelection(potentialMicroGames));
            selectedGames.Add(WeightedMicroGame.RandomSelection(potentialMicroGames));
            selectedGames.Add(WeightedMicroGame.RandomSelection(potentialMicroGames));
            selectedGames.Do(x =>
            {
                x.manager = this;
                x.roomBehavior = this.rooms[x.roomToUse];
            });
            SwitchToRoom(currentGame.roomToUse);
            base.StartCoroutine(PlayRandomBaldiAnimationsTilTalkingsDone());
            base.StartCoroutine(base.IntroDelay());
        }

        public void BeginCurrentGame()
        {
            SwitchToRoom(currentGame.roomToUse);
            state = KitchenState.MiniIntro; // time to perform this rooms mini intro! (todo: this will probably be moved elsewhere)
            currentGame.PrepareMiniIntro(ref audIntro);
            base.StartCoroutine(base.IntroDelay());
            base.StartCoroutine(PlayRandomBaldiAnimationsTilTalkingsDone());
        }

        public override void IntroFinished()
        {
            base.IntroFinished();
            switch (state)
            {
                case KitchenState.Intro:
                    BeginCurrentGame();
                    break;
                case KitchenState.MiniIntro:
                    state = KitchenState.Active;
                    LowerBaldi();
                    currentRoom.BeginPlay();
                    currentGame.Start();
                    break;
            }
        }

        public IEnumerator PlayRandomBaldiAnimationsTilTalkingsDone()
        {
            while (baldiMan.filesQueued <= 0) yield return null;
            while (baldiMan.IsPlaying)
            {
                RandomBaldiAnimation();
                yield return new WaitForSeconds(1.75f);
            }
            yield break;
        }

        private void UpdateScore()
        {
            this.scoreText.text = String.Format("Score: {0}\nTime: {1}", score.ToString(), timer.ToString("F1"));
        }
    }
}
