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
    public class KitchenOven : KitchenFieldTripRoom
    {
        public override KitchenRoom room => KitchenRoom.Oven;
        public override Sprite roomSprite => BaldiKitchenPlugin.OvenBG;

        public Knob[] knobs = new Knob[2];

        public Book myBook;

        public StandardMenuButton bookObject;

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
            myBook = GameObject.Instantiate<Image>(background, transform).gameObject.AddComponent<Book>();
            myBook.name = "Book";
            myBook.Initialize(manager);
            myBook.gameObject.SetActive(false);
            myBook.transform.SetAsLastSibling();
            bookObject = BaldiKitchenPlugin.CreateImage(BaldiKitchenPlugin.Book, transform, new Vector3(350f,173f)).gameObject.AddComponent<StandardMenuButton>().InitializeAllEvents();
            bookObject.name = "Book Object";
            bookObject.tag = "Untagged";
            bookObject.OnPress.AddListener(() =>
            {
                myBook.SetPage(0);
                myBook.gameObject.SetActive(true);
            });
            myBook.transform.SetAsLastSibling();
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
            bookObject.tag = "Button";
        }

        public override void StopPlay()
        {
            for (int i = 0; i < knobs.Length; i++)
            {
                knobs[i].SetButtonActive(false);
            }
            bookObject.tag = "Untagged";
        }

        public override void Reset()
        {
            StopPlay();
            SetTemperature(UnityEngine.Random.Range(10, 31) * 10);
            for (int i = 0; i < knobs.Length; i++)
            {
                knobs[i].transform.eulerAngles = Vector3.zero;
            }
            myBook.SetPage(0);
            myBook.gameObject.SetActive(false);
        }

        public class Book : MonoBehaviour
        {
            public Sprite frontPage;
            public Sprite middlePage;
            public Sprite backPage;
            public Image bookImage;
            public StandardMenuButton leftButton;
            public StandardMenuButton rightButton;
            public TMP_Text leftText;
            public TMP_Text rightText;
            public int currentPageIndex = 0;
            public Page currentPage => pages[currentPageIndex];
            public List<Page> pages = new List<Page>
            {
                //front page
                new Page
                {
                    text="YOU SHOULDNT BE SEEING THIS"
                },
                //dummy page
                new Page
                {
                    text="YOU SHOULDNT BE SEEING THIS"
                },
                //actual pages go here
                new Page
                {
                    text="YOU SHOULD BE SEEING THIS"
                },
                new Page
                {
                    text="YOU SHOULD BE SEEING THIS"
                },
                //end page
                new Page
                {
                    text="YOU SHOULDNT BE SEEING THIS"
                }
            };

            public void SetPage(int index)
            {
                currentPageIndex = Mathf.Clamp(index,0,pages.Count - 1);
                if (currentPageIndex == 0)
                {
                    leftText.color = Color.clear;
                    rightText.color = Color.clear;
                    bookImage.sprite = frontPage;
                    return;
                }
                if (currentPageIndex == pages.Count - 1)
                {
                    leftText.color = Color.clear;
                    rightText.color = Color.clear;
                    bookImage.sprite = backPage;
                    return;
                }
                leftText.color = Color.black;
                rightText.color = Color.black;
                leftText.text = currentPage.text;
                rightText.text = pages[currentPageIndex + 1].text;
                bookImage.sprite = middlePage;
            }

            public void ChangePage(int amount)
            {
                SetPage(currentPageIndex + amount);
            }


            public void Initialize(KitchenFieldTripManager manager)
            {
                frontPage = BaldiKitchenPlugin.bookSprites[0];
                middlePage = BaldiKitchenPlugin.bookSprites[1];
                backPage = BaldiKitchenPlugin.bookSprites[2];
                bookImage = gameObject.GetComponent<Image>();
                bookImage.sprite = frontPage;
                Image left = BaldiKitchenPlugin.CreateImage(BaldiKitchenPlugin.pageHitbox, bookImage.transform, new Vector3(135f,180f));
                left.tag = "Button";
                leftButton = left.gameObject.AddComponent<StandardMenuButton>().InitializeAllEvents();
                Image right = BaldiKitchenPlugin.CreateImage(BaldiKitchenPlugin.pageHitbox, bookImage.transform, new Vector3(350f, 180f));
                right.tag = "Button";
                rightButton = right.gameObject.AddComponent<StandardMenuButton>().InitializeAllEvents();
                rightButton.OnPress.AddListener(() => {
                    ChangePage(2);
                });
                leftButton.OnPress.AddListener(() => {
                    if (currentPageIndex == 0)
                    {
                        gameObject.SetActive(false);
                        return;
                    }
                    ChangePage(-2);
                });
                left.color = Color.clear;
                right.color = Color.clear;
                leftText = GameObject.Instantiate<TMP_Text>(manager.GetScoreText(), transform);
                leftText.alignment = TextAlignmentOptions.TopLeft;
                leftText.rectTransform.anchorMin = Vector2.up;
                leftText.rectTransform.anchorMax = Vector2.up;
                leftText.rectTransform.offsetMin = left.rectTransform.offsetMin;
                leftText.rectTransform.offsetMax = left.rectTransform.offsetMax;
                leftText.text = "LOL";
                rightText = GameObject.Instantiate<TMP_Text>(manager.GetScoreText(), transform);
                rightText.alignment = TextAlignmentOptions.TopLeft;
                rightText.rectTransform.anchorMin = Vector2.up;
                rightText.rectTransform.anchorMax = Vector2.up;
                rightText.rectTransform.offsetMin = right.rectTransform.offsetMin;
                rightText.rectTransform.offsetMax = right.rectTransform.offsetMax;
                rightText.text = "LOL";
                left.transform.SetAsLastSibling();
                right.transform.SetAsLastSibling();
            }

            public struct Page
            {
                public string text;
                public Sprite image;
            }
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
