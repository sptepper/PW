using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace BarData
{
    public class BarLoader : MonoBehaviour
    {
        public List<Bar> bars = new List<Bar>();
        public static BarLoader instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
            }
            else
            {
                instance = this;
            }

            LoadBars("G:\\Code\\PW\\Binary.txt");
            //PrintAllBars();
        }

        public void LoadBars(string filename)
        {
            var lines = File.ReadLines(filename);
            foreach (var line in lines)
                bars.Add(new Bar(line));

            Debug.Log("Bars Loaded from " + filename);
        }

        public void PrintAllBars()
        {
            foreach (var bar in bars)
                Debug.Log(bar.DataString());
        }
    }


    public class Bar
    {
        public const int LEN = 30;

        public bool[] data;

        public Bar(string text)
        {
            LoadBarData(text);

        }

        private void LoadBarData(string text)
        {
            if (text.Length != LEN)
            {
                Debug.LogError("INVALID BAR DATA: " + text + " | Must be length " + LEN);
                return;
            }

            data = new bool[LEN];

            int x = 0;
            foreach (char c in text)
            {
                if (c == '1')
                { data[x] = true; }
                else if (c == '0')
                { data[x] = false; }
                else
                {
                    Debug.LogError("INVALID BAR DATA: " + text + " | Must be length " + LEN);
                    return;
                }
                x++;
            }
        }

        public string DataString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (bool b in data)
            {
                sb.Append(b ? "1" : "0");
            }
            return sb.ToString();
        }

        /// Total of value difference at each position
        public int Distance(Bar b)
        {
            int dist = 0;
            for (int i = 0; i < LEN; i++)
            {
                if (data[i] != b.data[i])
                    dist++;
            }
            return dist;
        }

        public float On_Percent()
        {
            float val = 0;
            foreach (bool b in data)
                if(b) val++;
            return val / LEN;
        }

        public int First_On()
        {
            int n = 0;
            while (!data[n])
                n++;     
            return n;
        }

        public int First_Change()
        {
            bool first = data[0];
            int n = 0;
            while (data[n] != first) n++;
            return n;
        }

        public int Second_Change()
        {
            bool first = data[0];
            int n = 0;
            while (data[n] != first) n++;
            while (data[n] == first) n++;
            return n;
        }

        public int Number_Of_Changes()
        {
            int count = 0;
            bool current = data[0];
            for (int n = 1;n < LEN;n++)
            {
                if (data[n] != current) count++;
                current = data[n];
            }
            return count;
        }


    }

}