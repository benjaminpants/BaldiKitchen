using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BaldiKitchen
{
    [BepInPlugin("mtm101.rulerp.baldiplus.baldikitchen", "Baldi's Kitchen", "0.0.0.0")]
    public class BaldiKitchenPlugin : BaseUnityPlugin
    {
        static FieldInfo FTaudCorrect = AccessTools.Field(typeof(FieldTripManager), "audCorrect");
        static FieldInfo FTbaldiAnimator = AccessTools.Field(typeof(FieldTripManager), "baldiAnimator");
        static FieldInfo FTbaldiMan = AccessTools.Field(typeof(FieldTripManager), "baldiMan");
        static FieldInfo AMAanimator = AccessTools.Field(typeof(AudioManagerAnimator), "animator");

        public static List<SoundObject> SoundObjects;

        public static CampingTripManager CampingTrip;
        public static GameObject EmptyTrip;
        public static KitchenFieldTripManager kitchenManager;

        public static Dictionary<string, SoundObject> baldiDialogue = new Dictionary<string, SoundObject>();

        public static Sprite OvenBG;
        public static Sprite Knob;
        public static Sprite pageHitbox;
        public static Sprite Book;
        public static Sprite[] bookSprites = new Sprite[3];

        public static Image CreateImage(Sprite spr, Transform parent, Vector3 position)
        {
            Image img = new GameObject().AddComponent<Image>();
            img.gameObject.layer = LayerMask.NameToLayer("UI");
            img.transform.SetParent(parent);
            img.sprite = spr;
            img.gameObject.transform.localScale = Vector3.one;
            img.rectTransform.offsetMin = new Vector2(-spr.texture.width / 2f, -spr.texture.height / 2f);
            img.rectTransform.offsetMax = new Vector2(spr.texture.width / 2f, spr.texture.height / 2f);
            img.rectTransform.anchorMin = new Vector2(0f, 1f);
            img.rectTransform.anchorMax = new Vector2(0f, 1f);
            img.transform.localPosition = new Vector3(-240f, 180f) + (new Vector3(position.x, position.y * -1f));
            return img;
        }

        void Awake()
        {
            Harmony harmony = new Harmony("mtm101.rulerp.baldiplus.baldikitchen");
            OvenBG = AssetManager.SpriteFromTexture2D(AssetManager.TextureFromMod(this, "Backgrounds", "OvenBG.png"));
            Knob = AssetManager.SpriteFromTexture2D(AssetManager.TextureFromMod(this, "Interactables", "Knob.png"));
            Book = AssetManager.SpriteFromTexture2D(AssetManager.TextureFromMod(this, "Interactables", "Book.png"));
            pageHitbox = AssetManager.SpriteFromTexture2D(AssetManager.TextureFromMod(this, "Interactables", "Page.png"));
            bookSprites[0] = AssetManager.SpriteFromTexture2D(AssetManager.TextureFromMod(this, "Backgrounds", "CookBookClosed.png"));
            bookSprites[1] = AssetManager.SpriteFromTexture2D(AssetManager.TextureFromMod(this, "Backgrounds", "CookBookOpen.png"));
            bookSprites[2] = AssetManager.SpriteFromTexture2D(AssetManager.TextureFromMod(this, "Backgrounds", "CookBookClosedBack.png"));
            CreateNewBaldiVL("kitchen_intro.wav","Baldi_Vfx_Kitchen_Intro");
            CreateNewBaldiVL("kitchen_end.wav", "Baldi_Vfx_Kitchen_End");
            CreateNewBaldiVL("kitchen_today.wav", "Baldi_Vfx_Kitchen_Today");
            CreateNewBaldiVL("kitchen_today_thanksgiving.wav", "Baldi_Vfx_Kitchen_Today_Thanksgiving");
            CreateNewBaldiVL("oven_preheat_1.wav", "Baldi_Vfx_Kitchen_Preheat_1");
            CreateNewBaldiVL("oven_preheat_2.wav", "Baldi_Vfx_Kitchen_Preheat_2");
            CreateNewBaldiVL("food_turkey.wav", "Baldi_Vfx_Kitchen_Food_Turkey");
            CreateNewBaldiVL("food_stuffing.wav", "Baldi_Vfx_Kitchen_Food_Stuffing");
            CreateNewBaldiVL("food_pie.wav", "Baldi_Vfx_Kitchen_Food_Pie");
            harmony.PatchAll();
        }

        private void CreateNewBaldiVL(string audioname, string subkey)
        {
            baldiDialogue.Add(audioname, ObjectCreatorHandlers.CreateSoundObject(AssetManager.AudioClipFromMod(this, "BaldiAudio", audioname), subkey, SoundType.Voice, Color.green));
        }

        public static T CreateNewFieldTrip<T>(FieldTripObject tripObject) where T : FieldTripManager
        {
            T manager = GameObject.Instantiate(EmptyTrip).AddComponent<T>();
            // initialize all the variables the manager needs to work properly
            manager.SetVariable("endlessResultsPre", manager.gameObject.GetComponentInChildren<EndlessTripResults>());
            manager.SetVariable("tripObject", tripObject);
            FTbaldiAnimator.SetValue(manager, manager.gameObject.GetComponentInChildren<Animator>()); //there should only be one left after the cleaning
            manager.SetVariable("scoreText", manager.gameObject.transform.Find("Score").gameObject.GetComponent<TMP_Text>());
            manager.SetVariable("rankDisplay", manager.gameObject.GetComponentInChildren<TripRankDisplay>());
            manager.SetVariable("baldi", manager.gameObject.transform.Find("Baldi").GetComponent<RectTransform>());
            FTbaldiMan.SetValue(manager, manager.gameObject.GetComponentInChildren<AudioManagerAnimator>());
            AMAanimator.SetValue(FTbaldiMan.GetValue(manager), FTbaldiAnimator.GetValue(manager)); //update the animator property of the baldi
            FTaudCorrect.SetValue(manager, ((SoundObject[])FTaudCorrect.GetValue(CampingTrip)).Clone());
            manager.SetVariable("baldiStartHeight", 0f);
            //finalize
            DontDestroyOnLoad(manager.gameObject);
            manager.gameObject.name = String.Format("{0}FieldTrip", EnumExtensions.GetExtendedName<FieldTrips>((int)tripObject.trip));
            tripObject.tripPre = manager;
            return manager;
        }
    }

    public static class Extensions
    {
        public static void SetVariable(this object me, string name, object setTo)
        {
            AccessTools.Field(me.GetType(),name).SetValue(me, setTo);
        }

        public static StandardMenuButton InitializeAllEvents(this StandardMenuButton smb)
        {
            smb.OnPress = new UnityEngine.Events.UnityEvent();
            smb.OnHighlight = new UnityEngine.Events.UnityEvent();
            smb.OnRelease = new UnityEngine.Events.UnityEvent();
            return smb;
        }
    }

    [HarmonyPatch(typeof(NameManager))]
    [HarmonyPatch("Awake")]
    class NamePatch
    {
        static void Prefix()
        {
            BaldiKitchenPlugin.SoundObjects = Resources.FindObjectsOfTypeAll<SoundObject>().Where(x => x.name != null).ToList();
            BaldiKitchenPlugin.CampingTrip = Resources.FindObjectsOfTypeAll<CampingTripManager>().First();
            BaldiKitchenPlugin.CampingTrip.gameObject.SetActive(false);
            GameObject emptyTrip = GameObject.Instantiate<CampingTripManager>(BaldiKitchenPlugin.CampingTrip).gameObject;
            BaldiKitchenPlugin.CampingTrip.gameObject.SetActive(true);
            UnityEngine.Object.DestroyImmediate(emptyTrip.GetComponent<CampingTripManager>());
            GameObject.DestroyImmediate(emptyTrip.transform.Find("Campfires").gameObject);
            GameObject.DestroyImmediate(emptyTrip.transform.Find("Buttons3").gameObject);
            GameObject.DestroyImmediate(emptyTrip.transform.Find("Arithmesticks").gameObject);
            GameObject.DestroyImmediate(emptyTrip.transform.Find("Buttons4").gameObject);
            GameObject.DestroyImmediate(emptyTrip.transform.Find("Buttons5").gameObject);
            for (int _ = 0; _ < 2; _++)
            {
                GameObject.DestroyImmediate(emptyTrip.gameObject.GetComponent<AudioManager>());
                GameObject.DestroyImmediate(emptyTrip.gameObject.GetComponent<AudioSource>());
            }
            emptyTrip.name = "Fieldtrip Template";
            GameObject.DontDestroyOnLoad(emptyTrip.gameObject);
            BaldiKitchenPlugin.EmptyTrip = emptyTrip;

            // create the kitchen manager
            BaldiKitchenPlugin.kitchenManager = BaldiKitchenPlugin.CreateNewFieldTrip<KitchenFieldTripManager>(ObjectCreatorHandlers.CreateFieldTripObject(EnumExtensions.ExtendEnum<FieldTrips>("Kitchen"), null, "ERROR", "Unknown"));
            KitchenFieldTripManager km = BaldiKitchenPlugin.kitchenManager;
            km.Background = BaldiKitchenPlugin.kitchenManager.gameObject.transform.Find("BG").GetComponent<Image>();
            km.InitializeRoom<KitchenOven>();
            AudioManager am = km.gameObject.AddComponent<AudioManager>();
            am.audioDevice = km.gameObject.AddComponent<AudioSource>();
            am.audioDevice.outputAudioMixerGroup = Resources.FindObjectsOfTypeAll<UnityEngine.Audio.AudioMixerGroup>().First(x => x.name == "Effects");
            am.audioDevice.ignoreListenerPause = true;
            km.myMan = am;
            km.successSound = BaldiKitchenPlugin.SoundObjects.Find(x => x.name == "BellGeneric");
            km.failSound = BaldiKitchenPlugin.SoundObjects.Find(x => x.name == "BAL_Ohh");
            GameObject.Destroy(BaldiKitchenPlugin.kitchenManager.Background.gameObject);
        }
    }
}
