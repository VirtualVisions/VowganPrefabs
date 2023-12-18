using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

namespace Vowgan.Clocks
{
    public class ClockUtility
    {
        
        public static List<string> Timezones = new List<string>
        {
            "UTC-10:00/Hawaiian Standard Time",
            "UTC-09:00/Alaskan Standard Time",
            "UTC-08:00/Pacific Standard Time (Mexico)",
            "UTC-08:00/Pacific Standard Time",
            "UTC-07:00/US Mountain Standard Time",
            "UTC-07:00/Mountain Standard Time (Mexico)",
            "UTC-07:00/Mountain Standard Time",
            "UTC-06:00/Central America Standard Time",
            "UTC-06:00/Central Standard Time",
            "UTC-06:00/Central Standard Time (Mexico)",
            "UTC-06:00/Canada Central Standard Time",
            "UTC-05:00/SA Pacific Standard Time",
            "UTC-05:00/Eastern Standard Time",
            "UTC-05:00/US Eastern Standard Time",
            "UTC-04:30/Venezuela Standard Time",
            "UTC-04:00/Paraguay Standard Time",
            "UTC-04:00/Atlantic Standard Time",
            "UTC-04:00/Central Brazilian Standard Time",
            "UTC-04:00/SA Western Standard Time",
            "UTC-04:00/Pacific SA Standard Time",
            "UTC-03:30/Newfoundland Standard Time",
            "UTC-03:00/E. South America Standard Time",
            "UTC-03:00/Argentina Standard Time",
            "UTC-03:00/SA Eastern Standard Time",
            "UTC-03:00/Greenland Standard Time",
            "UTC-03:00/Montevideo Standard Time",
            "UTC-02:00/Mid-Atlantic Standard Time",
            "UTC-01:00/Azores Standard Time",
            "UTC-01:00/Cape Verde Standard Time",
            "UTC/Morocco Standard Time",
            "UTC/GMT Standard Time",
            "UTC/Greenwich Standard Time",
            "UTC+01:00/W. Europe Standard Time",
            "UTC+01:00/Central Europe Standard Time",
            "UTC+01:00/Romance Standard Time",
            "UTC+01:00/Central European Standard Time",
            "UTC+01:00/W. Central Africa Standard Time",
            "UTC+01:00/Namibia Standard Time",
            "UTC+02:00/Jordan Standard Time",
            "UTC+02:00/GTB Standard Time",
            "UTC+02:00/Middle East Standard Time",
            "UTC+02:00/Egypt Standard Time",
            "UTC+02:00/Syria Standard Time",
            "UTC+02:00/South Africa Standard Time",
            "UTC+02:00/FLE Standard Time",
            "UTC+02:00/Israel Standard Time",
            "UTC+02:00/E. Europe Standard Time",
            "UTC+03:00/Arabic Standard Time",
            "UTC+03:00/Arab Standard Time",
            "UTC+03:00/Russian Standard Time",
            "UTC+03:00/E. Africa Standard Time",
            "UTC+03:30/Iran Standard Time",
            "UTC+04:00/Arabian Standard Time",
            "UTC+04:00/Azerbaijan Standard Time",
            "UTC+04:00/Mauritius Standard Time",
            "UTC+04:00/Georgian Standard Time",
            "UTC+04:00/Caucasus Standard Time",
            "UTC+04:30/Afghanistan Standard Time",
            "UTC+05:00/Pakistan Standard Time",
            "UTC+05:00/West Asia Standard Time",
            "UTC+05:30/India Standard Time",
            "UTC+05:30/Sri Lanka Standard Time",
            "UTC+05:45/Nepal Standard Time",
            "UTC+06:00/Central Asia Standard Time",
            "UTC+06:00/Bangladesh Standard Time",
            "UTC+06:00/Ekaterinburg Standard Time",
            "UTC+06:30/Myanmar Standard Time",
            "UTC+07:00/SE Asia Standard Time",
            "UTC+07:00/N. Central Asia Standard Time",
            "UTC+08:00/North Asia Standard Time",
            "UTC+08:00/China Standard Time",
            "UTC+08:00/North Asia East Standard Time",
            "UTC+08:00/Singapore Standard Time",
            "UTC+08:00/W. Australia Standard Time",
            "UTC+08:00/Taipei Standard Time",
            "UTC+08:00/Ulaanbaatar Standard Time",
            "UTC+09:00/Korea Standard Time",
            "UTC+09:00/Tokyo Standard Time",
            "UTC+09:30/Cen. Australia Standard Time",
            "UTC+09:30/AUS Central Standard Time",
            "UTC+10:00/Yakutsk Standard Time",
            "UTC+10:00/E. Australia Standard Time",
            "UTC+10:00/AUS Eastern Standard Time",
            "UTC+10:00/West Pacific Standard Time",
            "UTC+10:00/Tasmania Standard Time",
            "UTC+11:00/Vladivostok Standard Time",
            "UTC+11:00/Central Pacific Standard Time",
            "UTC+12:00/New Zealand Standard Time",
            "UTC+12:00/UTC+12",
            "UTC+12:00/Fiji Standard Time",
            "UTC+12:00/Kamchatka Standard Time",
            "UTC-12:00/Dateline Standard Time",
            "UTC+13:00/Samoa Standard Time",
            "UTC+13:00/Tonga Standard Time",
        };

        public class ClockHand
        {
            public Transform Hand;
            public ClockHandMode Mode;
        }

        public class ClockFill
        {
            public Image Fill;
            public ClockHandMode Mode;
        }

        public class ClockLabel
        {
            public TextMeshProUGUI Label;
            public ClockLabelMode Mode;
        }
        
        
        
        public class TimezoneSearchProvider : ScriptableObject, ISearchWindowProvider
        {
            
            public string Title;
            public List<string> SearchItems;
            public Action<string> OnIndexCallback;
            
            
            public TimezoneSearchProvider(string title, List<string> searchItems, Action<string> callback)
            {
                Title = title;
                SearchItems = searchItems;
                OnIndexCallback = callback;
            }

            public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
            {
                List<SearchTreeEntry> searchList = new List<SearchTreeEntry>
                {
                    new SearchTreeGroupEntry(new GUIContent(Title), 0),
                };
                
                PrepareList();
                
                List<string> groups = new List<string>();
                foreach (string item in SearchItems)
                {
                    string[] entryTitle = item.Split('/');
                    string groupName = "";

                    for (int i = 0; i < entryTitle.Length - 1; i++)
                    {
                        groupName += entryTitle[i];
                        if (!groups.Contains(groupName))
                        {
                            searchList.Add(new SearchTreeGroupEntry(new GUIContent(entryTitle[i]), i + 1));
                            groups.Add(groupName);
                        }

                        groupName += "/";
                    }

                    SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(entryTitle.Last()))
                    {
                        level = entryTitle.Length,
                        userData = entryTitle.Last(),
                    };

                    searchList.Add(entry);
                }
                
                return searchList;
            }

            public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
            {
                OnIndexCallback?.Invoke((string) searchTreeEntry.userData);
                return true;
            }
            
            private void PrepareList()
            {
                List<string> sortedSearchItems = SearchItems;
                
                sortedSearchItems.Sort((a, b) =>
                {
                    string[] splits1 = a.Split('/');
                    string[] splits2 = b.Split('/');
                    for (int i = 0; i < splits1.Length; i++)
                    {
                        if (i >= splits2.Length)
                        {
                            return 1;
                        }

                        int value = String.Compare(splits1[i], splits2[i], StringComparison.Ordinal);
                        if (value != 0)
                        {
                            if (splits1.Length != splits2.Length && (i == splits1.Length - 1 || i == splits2.Length - 1))
                            {
                                return splits1.Length < splits2.Length ? 1 : -1;
                            }
                            return value;
                        }
                    }
                    return 0;
                });
                
                SearchItems = sortedSearchItems;
            }
        }
        
    }
}
