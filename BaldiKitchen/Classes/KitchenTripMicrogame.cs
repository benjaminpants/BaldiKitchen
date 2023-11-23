using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BaldiKitchen
{
    public enum MicroGameState
    {
        Progress = 0,
        Won = 1,
        Lost = 2,
        Death = 3
    }

    public class WeightedMicroGame : WeightedSelection<KitchenTripMicrogame>
    {
        public WeightedMicroGame(KitchenTripMicrogame game, int weight)
        {
            selection = game;
            this.weight = weight;
        }
    }


    public abstract class KitchenTripMicrogame
    {
        public virtual KitchenRoom roomToUse => KitchenRoom.None;
        public KitchenFieldTripRoom roomBehavior;
        public KitchenFieldTripManager manager;

        public abstract void Start();

        public abstract MicroGameState Update();

        public abstract void End(MicroGameState state);

        public abstract void PrepareMiniIntro(ref SoundObject[] objs);
    }

    public class PreheatOvenMicrogame : KitchenTripMicrogame
    {
        public override KitchenRoom roomToUse => KitchenRoom.Oven;
        private bool doomed = false; //if that timer expires, you are DOOMED!
        private bool timeIsOut = false;
        public int targetTemp = 400;
        public string food = "turkey";
        private float timeAtRightTemp = 0f;

        public PreheatOvenMicrogame(string food, int targetTemp)
        {
            this.targetTemp = targetTemp;
            this.food = food;
        }

        public override void End(MicroGameState state)
        {
            // nothing needed
        }

        public override void PrepareMiniIntro(ref SoundObject[] objs)
        {
            objs = new SoundObject[]
            {
                BaldiKitchenPlugin.baldiDialogue["oven_preheat_1.wav"],
                BaldiKitchenPlugin.baldiDialogue[String.Format("food_{0}.wav",food)],
                BaldiKitchenPlugin.baldiDialogue["oven_preheat_2.wav"],
            };
        }

        public override void Start()
        {
            timeIsOut = false;
            doomed = false;
            timeAtRightTemp = 0f;
            manager.SetTimer(10f,TimeRanOut);
        }

        public void TimeRanOut()
        {
            if (timeAtRightTemp <= 0)
            {
                timeIsOut = true;
            }
            else
            {
                // to prevent you from loosing by 0.25 seconds
                doomed = true;
            }
        }

        public override MicroGameState Update()
        {
            int temp = ((KitchenOven)roomBehavior).temperature;
            if (temp == targetTemp)
            {
                timeAtRightTemp += Time.deltaTime;
                return (timeAtRightTemp >= 0.25f) ? MicroGameState.Won : MicroGameState.Progress;
            }
            else if (temp >= 1000)
            {
                return MicroGameState.Death;
            }
            if (timeIsOut)
            {
                return MicroGameState.Lost;
            }
            timeAtRightTemp = Mathf.Max(0f,timeAtRightTemp - Time.deltaTime);
            if (doomed && (timeAtRightTemp == 0))
            {
                timeIsOut = true;
                return MicroGameState.Lost;
            }
            return MicroGameState.Progress;
        }
    }
}
