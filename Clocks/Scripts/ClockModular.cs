
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.Clocks
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ClockModular : UdonSharpBehaviour
    {

        public Transform[] Hands;
        public ClockHandMode[] HandModes;
        public Image[] Fills;
        public ClockHandMode[] FillModes;
        public TextMeshProUGUI[] Labels;
        public ClockLabelMode[] LabelModes;

        [Tooltip("Whether to display hour text in a 24 hour format.")]
        public bool Use24HourTime;
        [Tooltip("Whether to use a audio for second hand ticking.")]
        public bool UseAudioTick;
        [Tooltip("Whether to use the player's local timezone, or one enforced via selection.")]
        public bool UseLocalTimeZone = true;
        
        public AudioSource AudioTick;
        public string TimeZoneID = "US Eastern Standard Time";

        private TimeZoneInfo timeZoneInfo;


        private void Start()
        {
            timeZoneInfo = UseLocalTimeZone ? TimeZoneInfo.Local : TimeZoneInfo.FindSystemTimeZoneById(TimeZoneID);
            _ClockUpdate();
        }

        public void _ClockUpdate()
        {
            if (UseAudioTick)
            {
                AudioTick.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                AudioTick.Play();
            }

            DateTime currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);

            if (Hands != null)
            {
                for (int i = 0; i < Hands.Length; i++)
                {
                    Transform hand = Hands[i];
                    switch (HandModes[i])
                    {
                        case ClockHandMode.Hour:
                            hand.localRotation = Quaternion.Euler(0, 0, currentTime.Hour * 30 + currentTime.Minute / 60f * 30);
                            break;
                        case ClockHandMode.Minute:
                            hand.localRotation = Quaternion.Euler(0, 0, currentTime.Minute * 6);
                            break;
                        case ClockHandMode.Second:
                            hand.localRotation = Quaternion.Euler(0, 0, currentTime.Second * 6);
                            break;
                    }
                }
            }

            if (Fills != null)
            {
                for (int i = 0; i < Fills.Length; i++)
                {
                    Image fill = Fills[i];
                    switch (FillModes[i])
                    {
                        case ClockHandMode.Hour:
                            fill.fillAmount = float.Parse(currentTime.ToString("hh"))/12f;
                            break;
                        case ClockHandMode.Minute:
                            fill.fillAmount = currentTime.Minute/60f;
                            break;
                        case ClockHandMode.Second:
                            fill.fillAmount = currentTime.Second/60f;
                            break;
                    }
                }
            }

            if (Labels != null)
            {
                for (int i = 0; i < Labels.Length; i++)
                {
                    TextMeshProUGUI label = Labels[i];

                    switch (LabelModes[i])
                    {
                        case ClockLabelMode.Hour:
                            label.text = currentTime.ToString("hh");
                            break;
                        case ClockLabelMode.Minute:
                            label.text = currentTime.ToString("mm");
                            break;
                        case ClockLabelMode.Second:
                            label.text = currentTime.ToString("ss");
                            break;
                        case ClockLabelMode.HourMinute:
                            if (Use24HourTime)
                            {
                                label.text = currentTime.ToString("HH:mm");
                            }
                            else
                            {
                                label.text = currentTime.ToString("hh:mm");
                            }

                            break;
                        case ClockLabelMode.HourMinuteSecond:
                            if (Use24HourTime)
                            {
                                label.text = currentTime.ToString("HH:mm:ss");
                            }
                            else
                            {
                                label.text = currentTime.ToString("hh:mm:ss");
                            }

                            break;
                        case ClockLabelMode.AmPm:
                            label.text = currentTime.ToString("tt");
                            break;
                        case ClockLabelMode.Weekday:
                            label.text = currentTime.ToString("dddd");
                            break;
                        case ClockLabelMode.MonthDayYear:
                            label.text = currentTime.ToString("MMMM dd, yyyy");
                            break;
                        case ClockLabelMode.DayMonthYear:
                            label.text = currentTime.ToString("dd MMMM, yyyy");
                            break;
                        case ClockLabelMode.WeekdayMonthDayYear:
                            label.text = currentTime.ToString("dddd  MMMM dd, yyyy");
                            break;
                        case ClockLabelMode.WeekdayDayMonthYear:
                            label.text = currentTime.ToString("dddd  dd MMMM, yyyy");
                            break;
                    }
                }
            }

            SendCustomEventDelayedSeconds(nameof(_ClockUpdate), 1);
        }
    }


    public enum ClockLabelMode
    {
        Hour,
        Minute,
        Second,
        HourMinute,
        HourMinuteSecond,
        AmPm,
        Weekday,
        MonthDayYear,
        DayMonthYear,
        WeekdayMonthDayYear,
        WeekdayDayMonthYear,
    }

    public enum ClockHandMode
    {
        Hour,
        Minute,
        Second,
    }
}